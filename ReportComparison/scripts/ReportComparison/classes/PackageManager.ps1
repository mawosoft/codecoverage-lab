# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.Collections
using namespace System.Collections.Generic
using namespace System.IO
using namespace System.Linq

class PackageMetaData {
    [string]$PackageId
    [string]$Moniker
    [string]$Family
    [string]$Platform
    [string]$CoreDllName
    [string]$ToolName
    [bool]$NoDownload
    [string[]]$Sources
}

class PackageInfo {
    [PackageMetaData]$Meta
    [Object]$Version
    [string]$PackagePath
    [string]$PropsPath
    [string]$TargetsPath
    [string]$CoreDllPath
    [string]$ToolDllPath
    [bool]$Required
    hidden [string]$_targetFramework
    hidden [sbyte]$_targetFrameworkResolved
    hidden [sbyte]$_packagePathExists

    PackageInfo([PackageMetaData]$meta, [Object]$version, [string]$packagePath) {
        $this.Meta = $meta
        $this.Version = $version
        $this.PackagePath = $packagePath
    }

    [bool]Exists() {
        if ($this._packagePathExists -eq 0) {
            $this._packagePathExists = [Directory]::Exists($this.PackagePath) ? 1 : -1
        }
        return $this._packagePathExists -gt 0
    }

    [bool]Resolve([string]$targetFramework) {
        if (-not $targetFramework) { throw [ArgumentException]::new([NullString]::Value, 'targetFramework') }
        if ($this._targetFrameworkResolved -ne 0 -and
            ($targetFramework -eq $this._targetFramework -or $this._targetFramework -eq 'any')) {
            return $this._targetFrameworkResolved -gt 0
        }
        $this.PropsPath = ''
        $this.TargetsPath = ''
        $this.CoreDllPath = ''
        $this.ToolDllPath = ''
        $this._targetFrameworkResolved = 0
        $this._targetFramework = ''
        $nuget = $script:Context.GetNuGetHelper()
        $tfm = $nuget.GetNuGetFrameworkType()::Parse($targetFramework)
        $this._targetFramework = $tfm.GetShortFolderName()
        if (-not $this.Exists()) {
            $this._targetFrameworkResolved = -1
            return $false
        }
        $buildDir = 'build'
        if ($this.Meta.Moniker -eq 'vstest-netsdk' -or $this.Meta.Moniker -eq 'mscc-vstudio') {
            $buildDir = ''
            $tfm = $null
            $this._targetFramework = 'any'
        }
        elseif ($this.Meta.Platform -ne 'vstest' -and $this.Meta.Platform -ne 'msbuild') {
            $buildDir = ''
        }
        if ($buildDir) {
            $this.PropsPath = $this.FindPath($this.Meta.PackageId + '.props', $buildDir, $tfm, $nuget)
            $this.TargetsPath = $this.FindPath($this.Meta.PackageId + '.targets', $buildDir, $tfm, $nuget)
            if (-not $this.PropsPath -and -not $this.TargetsPath) { $this._targetFrameworkResolved = -1 }
        }
        if ($this.Meta.CoreDllName) {
            $this.CoreDllPath = $this.FindPath($this.Meta.CoreDllName, '', $tfm, $nuget)
            if (-not $this.CoreDllPath) { $this._targetFrameworkResolved = -1 }
        }
        if ($this.Meta.ToolName) {
            $this.ToolDllPath = $this.FindPath($this.Meta.ToolName, '', $tfm, $nuget)
            if (-not $this.ToolDllPath) { $this._targetFrameworkResolved = -1 }
        }
        if ($this._targetFrameworkResolved -lt 0) {
            return $false
        }
        $this._targetFrameworkResolved = 1
        return $true
    }

    hidden [string]FindPath([string]$fileName, [string]$subdirName, [Object]$tfm, [NuGetHelper]$nuget) {
        $path = [Path]::GetFullPath([Path]::Combine($this.PackagePath, $subdirName))
        $options = $null -eq $tfm ? [SearchOption]::TopDirectoryOnly : [SearchOption]::AllDirectories
        $candidates = [Directory]::GetFiles($path, $fileName, $options)
        if (-not $candidates) { return '' }
        if ($candidates.Length -eq 1) { return $candidates[0] }
        if ($null -eq $tfm) { return '' }
        $tiFramework = $nuget.GetNuGetFrameworkType()
        $tfmCandidates = [array]::CreateInstance($tiFramework, $candidates.Length)
        $anyOf = '/\'.ToCharArray()
        $start = $path.Length
        if ($path[-1] -cnotin $anyOf) { $start++ }
        for ($i = 0; $i -lt $candidates.Length; $i++) {
            $segment = $candidates[$i]
            $pos1 = $start
            $pos2 = $segment.IndexOfAny($anyOf, $pos1)
            if ($pos2 -ge 0 -and -not $subdirName) {
                $pos1 = $pos2 + 1
                $pos2 = $segment.IndexOfAny($anyOf, $pos1)
            }
            $tfmCandidates[$i] = $pos2 -lt 0 ? $tiFramework::UnsupportedFramework : $tiFramework::Parse($segment.Substring($pos1, $pos2 - $pos1))
        }
        $reducer = $nuget.GetType('NuGet.Frameworks.FrameworkReducer')::new()
        $nearest = $reducer.GetNearest($tfm, $tfmCandidates)
        $i = [array]::IndexOf($tfmCandidates, $nearest)
        return $i -lt 0 ? '' : $candidates[$i]
    }
}

class PackageManager {
    [Dictionary[string, HashSet[string]]]$PlatformsByFamily
    [Dictionary[string, List[PackageInfo]]]$VersionsById
    [Dictionary[string, List[PackageInfo]]]$VersionsByMoniker
    [string]$GlobalPackagesPath

    PackageManager() {
        $this.GlobalPackagesPath = $this.GetGlobalPackagesPath()
        $this.PlatformsByFamily = [Dictionary[string, HashSet[string]]]::new([StringComparer]::OrdinalIgnoreCase)
        $this.VersionsById = [Dictionary[string, List[PackageInfo]]]::new([StringComparer]::OrdinalIgnoreCase)
        $this.VersionsByMoniker = [Dictionary[string, List[PackageInfo]]]::new([StringComparer]::OrdinalIgnoreCase)
        $data = Import-PowerShellDataFile -LiteralPath "$PSScriptRoot/../data/PackageMetaData.psd1"
        $nuget = $script:Context.GetNuGetHelper()
        foreach ($kvp in $data.GetEnumerator()) {
            $versions = $kvp.Value['Versions']
            $kvp.Value.Remove('Versions')
            $meta = [PackageMetaData]$kvp.Value
            $meta.PackageId = $kvp.Key
            if (-not $meta.Moniker) {
                $meta.Moniker = $meta.Family + '-' + $meta.Platform
            }
            $infos = [List[PackageInfo]]::new()
            # Moniker is required and Family/Platform must either both exist or not exist.
            $isValid = $meta.Moniker -and [bool]$meta.Family -eq [bool]$meta.Platform
            if ($isValid) {
                if ($versions) {
                    $versions = [array][Enumerable]::OrderDescending($versions.PSForEach({ $nuget.ParseVersion($_) }))
                    if ([Enumerable]::Count([Enumerable]::Distinct[Object]($versions)) -eq $versions.Length) {
                        # TODO Should we mark MTP platform as NoDownload?
                        foreach ($version in $versions) {
                            $path = [Path]::GetFullPath($this.GlobalPackagesPath + "/$($meta.PackageId)/$version".ToLowerInvariant())
                            $info = [PackageInfo]::new($meta, $version, $path)
                            $infos.Add($info)
                        }
                    }
                    else {
                        $isValid = $false
                    }
                }
                elseif ($meta.Moniker -eq 'vstest-netsdk') {
                    $meta.NoDownload = $true
                    $tuple = $this.GetLatestNetSdk()
                    $info = [PackageInfo]::new($meta, $nuget.ParseVersion($tuple.Item2), $tuple.Item1)
                    $infos.Add($info)
                }
                elseif ($meta.Moniker -eq 'mscc-vstudio') {
                    $meta.NoDownload = $true
                    $tuple = $this.GetLatestVStudio()
                    if ($tuple) {
                        $path = [Path]::GetFullPath($tuple.Item1 + '/Common7/IDE/Extensions/Microsoft/CodeCoverage.Console')
                        $info = [PackageInfo]::new($meta, $nuget.ParseVersion($tuple.Item2), $path)
                        $infos.Add($info)
                    }
                }
                else {
                    $isValid = $false
                }
            }
            if ($isValid -and $infos) {
                $this.VersionsById.Add($meta.PackageId, $infos)
                $this.VersionsByMoniker.Add($meta.Moniker, $infos)
                if ($meta.Family) {
                    $platforms = $this.PlatformsByFamily[$meta.Family]
                    if ($null -eq $platforms) {
                        $platforms = [HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
                        $this.PlatformsByFamily[$meta.Family] = $platforms
                    }
                    $platforms.Add($meta.Platform)
                }
            }
            if (-not $isValid) {
                throw 'Invalid package meta data for ' + $meta.PackageId
            }
        }
    }

    [array]ValidateWithDirectoryPackageProps() {
        $nuget = $script:Context.GetNuGetHelper()
        $path = $script:Context.GetProjectManager().DirectoryPackagesPropsPath
        $xml = [xml][File]::ReadAllText($path)
        $mismatches = [ArrayList]::new()
        foreach ($item in $xml.SelectNodes('//PackageVersion')) {
            $id = $item.Include
            $versions = $this.VersionsById[$id]
            if ($versions) {
                $pv = $nuget.ParseVersion($item.Version)
                $latest = $versions[0].Version
                if ($latest -ne $pv) {
                    $mismatches.Add([PSCustomObject]@{
                            PackageId      = $id
                            PackageVersion = $pv
                            LatestMeta     = $latest
                        })
                }
            }
        }
        return $mismatches
    }

    [void]DownloadRequiredPackages([bool]$force) {
        $downloadInfoBySource = @{}
        foreach ($list in $this.VersionsById.Values) {
            $meta = $list[0].Meta
            if ($meta.NoDownload) { continue }
            $sourceKey = "$($meta.Sources)"
            $downloads = $downloadInfoBySource[$sourceKey]?.Downloads
            foreach ($info in $list) {
                if (-not $info.Required -or (-not $force -and $info.Exists())) { continue }
                $id = "$($meta.PackageId)@$($info.Version)"
                if ($null -eq $downloads) {
                    $downloads = [ArrayList]::new()
                    $downloadInfoBySource[$sourceKey] = [PSCustomObject]@{
                        Meta      = $meta
                        Downloads = $downloads
                    }
                }
                $downloads.Add($id)
            }
        }
        foreach ($downloadInfo in $downloadInfoBySource.Values) {
            $params = @(
                'package', 'download', $downloadInfo.Downloads
                '--output', $this.GlobalPackagesPath
                '--prerelease'
                '--verbosity', 'minimal'
                $downloadInfo.Meta.Sources.PSForEach({ '--source', $_ })
            )
            InvokeNativeCommand dotnet $params
        }
    }

    hidden [string]GetGlobalPackagesPath() {
        $path = InvokeNativeCommand dotnet nuget locals global-packages --list --force-english-output -PassThru
        if ($path -like 'global-packages:*') {
            return $path.Substring(16).Trim()
        }
        else {
            throw 'Could not find nuget package root.'
        }
    }

    hidden [Tuple[string, string]]GetLatestNetSdk() {
        $path = InvokeNativeCommand dotnet --list-sdks -PassThru
        if ($path -is [array]) { $path = $path[-1] }
        $m = [regex]::Match($path, '(.+?)\s+\[(.+?)\]')
        if ($m.Success) {
            $path = [Path]::Combine($m.Groups[2].Value, $m.Groups[1].Value)
            return [Tuple]::Create($path, $m.Groups[1].Value)
        }
        else {
            throw 'Could not find dotnet SDK.'
        }
    }

    hidden [Tuple[string, string]]GetLatestVStudio() {
        $vswherePath = [Path]::Combine([string]${env:ProgramFiles(x86)}, 'Microsoft Visual Studio/Installer/vswhere.exe')
        if ([File]::Exists($vswherePath)) {
            $info = InvokeNativeCommand $vswherePath -latest -format json -utf8 -PassThru | ConvertFrom-Json -AsHashtable
            if ($info -is [hashtable]) {
                return [Tuple]::Create($info.installationPath, $info.catalog.productSemanticVersion)
            }
        }
        return $null
    }
}
