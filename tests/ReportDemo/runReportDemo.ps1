[CmdletBinding()]
param()

[bool]$backupPreference = $PSNativeCommandUseErrorActionPreference
try {
    $PSNativeCommandUseErrorActionPreference = $true
    $reportParams = @(
        "-reports:$PSScriptRoot/TestResults/logs/*/*cobertura.xml"
        "-targetdir:$PSScriptRoot/TestResults/report"
        '-reporttypes:Html,Markdown,Cobertura'
    )
    dotnet build "$PSScriptRoot/ReportDemo.csproj"
    Remove-Item "$PSScriptRoot/TestResults" -Recurse -ErrorAction Ignore
    dotnet test "$PSScriptRoot/ReportDemo.csproj" --no-build --collect 'XPlat Code Coverage'
    reportgenerator $reportParams
    Rename-Item "$PSScriptRoot/TestResults/logs" 'logs-coverlet'
    Rename-Item "$PSScriptRoot/TestResults/report" 'report-coverlet'
    dotnet test "$PSScriptRoot/ReportDemo.csproj" --no-build --collect 'XPlat Code Coverage' -- 'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.SingleHit=true'
    reportgenerator $reportParams
    Rename-Item "$PSScriptRoot/TestResults/logs" 'logs-coverlet-singlehit'
    Rename-Item "$PSScriptRoot/TestResults/report" 'report-coverlet-singlehit'
    dotnet test "$PSScriptRoot/ReportDemo.csproj" --no-build --collect 'Code Coverage'
    reportgenerator $reportParams
    Rename-Item "$PSScriptRoot/TestResults/logs" 'logs-mscodecoverage'
    Rename-Item "$PSScriptRoot/TestResults/report" 'report-mscodecoverage'
}
finally {
    $PSNativeCommandUseErrorActionPreference = $backupPreference
}
