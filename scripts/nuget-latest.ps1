# Copyright (c) Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Gets the latest package version from NuGet feed for packages defined in Directory.Packages.props.
.DESCRIPTION
    This considers the regular PackageVersion entries as well as our own PackageDownloadVersion.
.OUTPUTS
    [pscustomobject] with id, latestVersion, currentVersion properties.
#>

[CmdletBinding()]
param()

$packagesProps = [xml](Get-Content -LiteralPath "$PSScriptRoot/../Directory.Packages.props" -Raw)
$packages = @{}
$packagesProps.SelectNodes('//PackageVersion').ForEach({
        $packages.Add($_.Include, $_.Version)
    })
$packagesProps.SelectNodes('//PackageDownloadVersion').ForEach({
        $v = $packages[$_.Include]
        $packages[$_.Include] = $v ? $v + ', ' + $_.Version : $_.Version
    })
$searchTerm = $packages.Keys | Join-String -FormatString 'packageid:{0}' -Separator ' '
$result = dotnet package search $searchTerm --source nuget --format json --verbosity minimal | ConvertFrom-Json
$result.searchResult.packages | Sort-Object -Property id | ForEach-Object {
    $_.psobject.Properties.Add([psnoteproperty]::new('currentVersion', $packages[$_.id]))
    $_
}
