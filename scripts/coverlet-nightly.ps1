# Copyright (c) Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Downloads and copies the latest nuget packages from the coverlet-nightly feed to the local feed.
.DESCRIPTION
    Downloads the packages into the nuget package root folder, then copies the actual *.nupkg
    to the local feed.

    Uses the 'dotnet package download' command, which requires .NET SDK 10.0.2xx or later.
#>

[CmdletBinding()]
param(
    # Only download the coverlet.MTP package
    [switch]$MTP
)

$PSNativeCommandUseErrorActionPreference = $true
$search = $MTP ? 'coverlet.MTP' : 'coverlet'
$nugetSource = 'coverlet-nightly'
$nugetLocalPath = "$PSScriptRoot/../../.nuget"
$nugetPackageRoot = dotnet nuget locals global-packages --list --force-english-output
if ($nugetPackageRoot -notlike 'global-packages:*') {
    throw 'Could not find nuget package root.'
}
$nugetPackageRoot = $nugetPackageRoot.Substring(16).Trim()
$common = @('--prerelease', '--source', $nugetSource)
$result = dotnet package search $search $common --format json
$result = $result | ConvertFrom-Json
$packages = $result.searchResult.packages.ForEach({ $_.id + '@' + $_.latestVersion })
dotnet package download $packages $common --output $nugetPackageRoot --verbosity minimal
$result.searchResult.packages | ForEach-Object {
    $path = [System.IO.Path]::Combine($nugetPackageRoot, $_.id, $_.latestVersion)
    Get-ChildItem -LiteralPath $path -File -Filter '*.nupkg'
} | Copy-Item -Destination $nugetLocalPath
