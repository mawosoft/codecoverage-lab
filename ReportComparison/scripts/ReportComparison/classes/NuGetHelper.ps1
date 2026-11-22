# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.Collections.Generic
using namespace System.IO
using namespace System.Reflection
using namespace System.Reflection.Metadata
using namespace System.Runtime.Loader

<#
    The [NuGetHelper] class provides access to NuGet.* types in a dedicated
    assembly load context. The NuGet assemblies are loaded either from the current
    .NET SDK or from a specified directory.

    Access to frequently used tasks and types is provided via dedicated methods, but
    any NuGet type can be queried by full name or assembly-qualified name.

    Requires explicit call of its Dispose() method for clean-up.
#>
class NuGetHelper {
    hidden [Dictionary[string, type]]$_typeCache
    hidden [string]$_nugetDir
    hidden [AssemblyLoadContext]$_alc

    NuGetHelper() {
        # Class can be used outside module
        if (Test-Path -LiteralPath 'function:InvokeNativeCommand') {
            $nugetDir = InvokeNativeCommand dotnet --list-sdks -PassThru
        }
        else {
            $nugetDir = dotnet --list-sdks
        }
        if ($nugetDir -is [array]) { $nugetDir = $nugetDir[-1] }
        $m = [regex]::Match($nugetDir, '(.+?)\s+\[(.+?)\]')
        $nugetDir = [Path]::Combine($m.Groups[2].Value, $m.Groups[1].Value)
        $this.Initialize($nugetDir)
    }

    NuGetHelper([string]$nugetDir) { $this.Initialize($nugetDir) }

    # Returns the parsed NuGetVersion, but also accepts VersionRange notation as long as
    # it produces a single, non-floating version.
    [Object]ParseVersion([string]$value) {
        $t = $this.GetVersionRangeType()
        $vr = $t::Parse($value, $false) # allowFloating = false
        $v = $vr.MinVersion
        if ($null -eq $v -or ($null -ne $vr.MaxVersion -and $v -ne $vr.MaxVersion)) {
            throw [FormatException]:: new()
        }
        return $v
    }

    [type]GetNuGetFrameworkType() { return $this.GetType('NuGet.Frameworks.NuGetFramework') }
    [type]GetNuGetVersionType() { return $this.GetType('NuGet.Versioning.NuGetVersion') }
    [type]GetVersionRangeType() { return $this.GetType('NuGet.Versioning.VersionRange') }

    [type]GetType([string]$typeName) {
        [type]$type = $this._typeCache[$typeName]
        if ($null -ne $type) { return $type }
        [TypeName]$parsedName = [TypeName]::Parse($typeName)
        [string]$fullName = $parsedName.FullName
        [string]$qualifiedName = $parsedName.AssemblyQualifiedName
        if ($null -eq $parsedName.AssemblyName) {
            # NuGet assemblies are MOSTLY named NuGet.Something with the
            # name also being the root namespace.
            [int]$pos = $fullName.IndexOf([char]'.')
            $pos = $fullName.IndexOf([char]'.', $pos + 1)
            $qualifiedName = $fullName + ', ' + $fullName.Substring(0, $pos)
        }
        $scope = $this._alc.EnterContextualReflection()
        try {
            $type = [type]::GetType($qualifiedName, $true) # throwOnError
            $this._typeCache[$typeName] = $type
            $this._typeCache[$fullName] = $type
            $this._typeCache[$qualifiedName] = $type
            return $type
        }
        finally {
            $scope.Dispose()
        }
    }

    [void]Dispose() {
        if ($null -eq $this._alc) { return }
        $this._typeCache.Clear()
        $this._alc.remove_Resolving($this.OnResolving)
        $this._alc.Unload()
        $this._alc = $null
    }

    hidden [void]Initialize([string]$nugetDir) {
        $this._typeCache = [Dictionary[string, type]]::new()
        $this._nugetDir = $nugetDir
        $this._alc = [AssemblyLoadContext]::new($null, $true) # isCollectible
        $this._alc.add_Resolving($this.OnResolving)
    }

    hidden [Assembly]OnResolving([AssemblyLoadContext]$alc, [AssemblyName]$assemblyName) {
        $path = [Path]::Combine($this._nugetDir, $assemblyName.Name + '.dll')
        return $alc.LoadFromAssemblyPath($path)
    }
}
