# Copyright (c) Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Gets the latest package version from NuGet feed for packages defined in Directory.Packages.props
    and PackageMetaData.psd1.
.OUTPUTS
    [pscustomobject] with id, latest, current, metaData properties.
#>

[CmdletBinding()]
param()

. "$PSScriptRoot/../ReportComparison/scripts/classes/NuGetHelper.ps1"

$bakFEL = $global:FormatEnumerationLimit
$nuget = [NuGetHelper]::new()
try {
    $packages = @{}
    $pkgProps = [xml](Get-Content -LiteralPath "$PSScriptRoot/../Directory.Packages.props" -Raw)
    $pkgProps.SelectNodes('//PackageVersion').PSForEach({
            $packages.Add($_.Include, [pscustomobject]@{
                    id       = $_.Include
                    latest   = $null
                    current  = $nuget.ParseVersion($_.Version)
                    metaData = $null
                })
        })
    $pkgMeta = Import-PowerShellDataFile -LiteralPath "$PSScriptRoot/../ReportComparison/scripts/data/PackageMetaData.psd1"
    $pkgMeta.GetEnumerator().PSForEach({
            if (-not $_.Value['Versions']) { return }
            $data = $packages[$_.Key]
            if ($null -eq $data) {
                $data = [pscustomobject]@{
                    id       = $_.Key
                    latest   = $null
                    current  = $null
                    metaData = $null
                }
                $packages[$_.Key] = $data
            }
            $data.metaData = [array]($_.Value['Versions'].PSForEach({ $nuget.ParseVersion($_) }) | Sort-Object -Descending)
        })
    $badMeta = $packages.Values.PSWhere({ $_.current -and $_.metaData -and $_.current -ne $_.metaData[0] })
    if ($badMeta) {
        @(
            'Mismatch between PackageVersion and PackageMetaData.'
            $badMeta | Select-Object id, current, metaData
        ) | Out-String | Write-Warning
    }
    $searchTerm = $packages.Values.PSWhere({ ($_.current ?? $_.metaData[0]).IsPrerelease }).PSForEach('id') | 
        Join-String -FormatString 'packageid:{0}' -Separator ' '
    if ($searchTerm) {
        $result = dotnet package search $searchTerm --source nuget --prerelease --format json --verbosity minimal | ConvertFrom-Json
        $result.searchResult.packages.PSForEach({ $packages[$_.id].latest = $nuget.ParseVersion($_.latestVersion) })
    }
    $searchTerm = $packages.Values.PSWhere({ -not ($_.current ?? $_.metaData[0]).IsPrerelease }).PSForEach('id') | 
        Join-String -FormatString 'packageid:{0}' -Separator ' '
    if ($searchTerm) {
        $result = dotnet package search $searchTerm --source nuget --format json --verbosity minimal | ConvertFrom-Json
        $result.searchResult.packages.PSForEach({ $packages[$_.id].latest = $nuget.ParseVersion($_.latestVersion) })
    }
    $global:FormatEnumerationLimit = 9999
    $packages.Values | Sort-Object -Property id
}
finally {
    $global:FormatEnumerationLimit = $bakFEL
    ${nuget}?.Dispose()
}
