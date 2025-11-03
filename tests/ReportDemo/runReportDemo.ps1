[CmdletBinding()]
param()

[bool]$backupPreference = $PSNativeCommandUseErrorActionPreference
try {
    $PSNativeCommandUseErrorActionPreference = $true
    dotnet build "$PSScriptRoot/ReportDemo.csproj"
    Remove-Item "$PSScriptRoot/TestResults" -Recurse -ErrorAction Ignore
    dotnet test "$PSScriptRoot/ReportDemo.csproj" --no-build --settings "$PSScriptRoot/.runsettings" --collect 'XPlat Code Coverage'
    reportgenerator "-reports:$PSScriptRoot/TestResults/logs/*/*cobertura.xml" "-targetdir:$PSScriptRoot/TestResults/report-coverlet" '-reporttypes:Html,Markdown,Cobertura'
    Rename-Item "$PSScriptRoot/TestResults/logs" 'logs-coverlet'
    dotnet test "$PSScriptRoot/ReportDemo.csproj" --no-build --settings "$PSScriptRoot/.runsettings" --collect 'Code Coverage'
    reportgenerator "-reports:$PSScriptRoot/TestResults/logs/*/*cobertura.xml" "-targetdir:$PSScriptRoot/TestResults/report-mscodecoverage" '-reporttypes:Html,Markdown,Cobertura'
    Rename-Item "$PSScriptRoot/TestResults/logs" 'logs-mscodecoverage'
}
finally {
    $PSNativeCommandUseErrorActionPreference = $backupPreference
}
