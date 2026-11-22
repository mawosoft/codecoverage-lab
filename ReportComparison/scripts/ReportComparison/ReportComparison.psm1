# Copyright (c) Matthias Wolf, Mawosoft.

# TODO ordered dot-sourcing of classes
. "$PSScriptRoot/classes/ArtifactName.ps1"
. "$PSScriptRoot/classes/ArtifactManager.ps1"
. "$PSScriptRoot/classes/CoverageSettings.ps1"
. "$PSScriptRoot/classes/NuGetHelper.ps1"
. "$PSScriptRoot/classes/PackageManager.ps1"
. "$PSScriptRoot/classes/ProjectManager.ps1"
. "$PSScriptRoot/classes/TaskManager.ps1"
. "$PSScriptRoot/classes/TempFileManager.ps1"
. "$PSScriptRoot/classes/Context.ps1"


Get-ChildItem -LiteralPath "$PSScriptRoot/functions" -Filter '*.ps1' | ForEach-Object {
    . $_.FullName
}

$script:Context = [Context]::new()

# TODO alias irc?
Export-ModuleMember -Function @(
    'Invoke-ReportComparison'
)
