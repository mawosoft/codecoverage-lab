# Copyright (c) Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Delete bin/obj subfolders and call any cleaner.ps1 in subfolders.
    Optionally delete binlog files/subfolders and .vs subfolders.
#>

[CmdletBinding(SupportsShouldProcess)]
Param (
    [Alias("bl")]
    # Also delete *.binlog files/subfolders
    [switch]$BinLog,
    [Alias("vs")]
    # Also delete *.binlog files/subfolders
    [switch]$VStudio
)

Get-ChildItem -Path $PSScriptRoot -Directory -Recurse -Include bin, obj | Remove-Item -Recurse
Get-ChildItem -Path $PSScriptRoot -Filter cleaner.ps1 -Recurse -File | Where-Object DirectoryName -ne $PSScriptRoot | ForEach-Object { & $_.FullName }
if ($BinLog.IsPresent) {
    Get-ChildItem -Path $PSScriptRoot -Directory -Recurse -Include MSBuild_Logs | Remove-Item -Recurse
    Get-ChildItem -Path $PSScriptRoot -File -Recurse -Include *.binlog | Remove-Item
}
if ($VStudio.IsPresent) {
    Get-ChildItem -Path $PSScriptRoot -Directory -Recurse -Force -Include .vs | Remove-Item -Recurse -Force
}
