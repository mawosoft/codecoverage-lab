# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.Collections
using namespace System.Collections.Generic

class BaseTask {
    [PackageInfo]$PackageInfo
    [Object]$Inputs
    [Object]$Outputs
}

class CollectTask {
    [PackageInfo]$PackageInfo
    [string]$Config
    [string[]]$Options
    [string]$Format

    [string]GetKey() {
        # TODO refine
        return $this.PackageInfo.Meta.Moniker, $this.PackageInfo.Version, $this.Config, $this.Options, $this.Format -join '/'
    }
}

class MergeTask {
    [PackageInfo]$PackageInfo
    [string[]]$Options
    [string]$Format
    [CollectTask]$CollectTask
}

class TaskManager {
    [List[CollectTask]]$CollectTasks
    TaskManager() {
        $data = Import-PowerShellDataFile -LiteralPath "$PSScriptRoot/../data/Tasks.psd1"
        $pkgman = $script:Context.GetPackageManager()
        $this.CollectTasks = [List[CollectTask]]::new()
        foreach ($taskdef in $data.collect) {
            $this.ExpandPackageInfos($taskdef, $pkgman)
            $this.ExpandCommonProperties($taskdef)
            foreach ($packageInfo in $taskdef.PackageInfos) {
                foreach ($config in $taskdef.Config) {
                    foreach ($options in $taskdef.Options) {
                        foreach ($formatEntry in $taskdef.Format.GetEnumerator()) {
                            $task = [CollectTask]::new()
                            $task.PackageInfo = $packageInfo
                            $task.Config = $config
                            $task.Options = $options
                            $task.Format = $formatEntry.Key
                            # TODO $task.MergeTo = $formatEntry.Value
                            $this.CollectTasks.Add($task)
                        }
                    }
                }
            }
        }
    }

    hidden [void]ExpandPackageInfos([IDictionary]$dict, [PackageManager]$pkgman) {
        if ($dict.Contains('Family') -and $dict.Contains('Moniker')) { throw [FormatException]::new() }
        $monikers = [ArrayList]::new()
        if ($dict['Moniker']) {
            $monikers.AddRange([string[]]$dict['Moniker'])
        }
        else {
            if (-not $dict['Family'] -or -not $dict['Platform']) { throw [FormatException]::new() }
            $families = 'all' -eq $dict['Family'] ? $pkgman.PlatformsByFamily.Keys : [string[]]$dict['Family']
            $value = 'all' -eq $dict['Platform'] ? $null : [string[]]$dict['Platform']
            foreach ($family in $families) {
                $platforms = $value ?? $pkgman.PlatformsByFamily[$family]
                foreach ($platform in $platforms) {
                    $monikers.Add("$family-$platform")
                }
            }
        }
        $versionStart = 0
        $versionEnd = [int]::MaxValue
        if ('latest' -eq $dict['Version']) {
            $versionEnd = 1
        }
        elseif ('notlatest' -eq $dict['Version']) {
            $versionStart = 1
        }
        elseif ('all' -ne $dict['Version']) {
            throw [FormatException]::new()
        }
        $dict['PackageInfos'] = $packageInfos = [List[PackageInfo]]::new()
        foreach ($moniker in $monikers) {
            $list = $pkgman.VersionsByMoniker[$moniker]
            if (-not $list) {
                # Special case. Visual Stdudio can be missing
                if ($moniker -ne 'mscc-vstudio') { throw "Package moniker not found: $moniker" }
            }
            else {
                $packageInfos.AddRange($list.GetRange($versionStart, [Math]::Min($versionEnd, $list.Count) - $versionStart))
            }
        }
    }

    hidden [void]ExpandCommonProperties([IDictionary]$dict) {
        if (-not $dict['Config'] -or 'all' -eq $dict['Config']) {
            $dict['Config'] = @('Debug', 'Release')
        }
        else {
            $dict['Config'] = [string[]]$dict['Config']
        }
        if (-not $dict['Options']) {
            $dict['Options'] = @(, @())
        }
        else {
            $dict['Options'] = $options = [string[][]]$dict['Options']
            for ($i = 0; $i -lt $options.Length; $i++) {
                $option = $options[$i]
                if ($option.Length -eq 1) {
                    $option = $option[0]
                    if ($option -eq 'default' -or $option -eq '-' -or $option -eq '--' -or $option -eq '') {
                        $options[$i] = @()
                    }
                }
            }
        }
        $format = $dict['Format']
        if ($format -is [IDictionary]) {
            $keys = [array]$format.Keys
            foreach ($key in $keys) { $format[$key] = [array]$format[$key] }
        }
        else {
            $keys = [string[]]$dict['Format']
            $dict['Format'] = $format = @{}
            foreach ($key in $keys) { $format.Add($key, $null) }
        }
        if ($format.Count -eq 0) { throw [FormatException]::new() }
    }
}
