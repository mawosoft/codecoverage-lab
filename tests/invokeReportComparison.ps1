# Copyright (c) Matthias Wolf, Mawosoft.

[CmdletBinding()]
param()

[bool]$backupPreference = $PSNativeCommandUseErrorActionPreference
try {
    $PSNativeCommandUseErrorActionPreference = $true

    $resultPath = "$PSScriptRoot/../TestResults"
    $logPath = "$resultPath/logs"
    $reportPath = "$resultPath/report"
    $projectPath = "$PSScriptRoot/CoverletVStudioVSTest/Default/Default.csproj"

    $reportParam = @(
        "-reports:$logPath/*/*cobertura.xml"
        "-targetdir:$reportPath"
        '-reporttypes:Html,Markdown,Cobertura'
        '-verbosity:verbose'
    )
    $reportParamRawMode = $reportParam + 'settings:rawMode=true'

    $testParam = @(
        $projectPath
        '--no-build'
        '--'
    )
    $testParamCoverlet = $testParam + 'DataCollectionRunSettings.DataCollectors.DataCollector[2].@enabled=false'
    $testParamCoverletSingleHit = $testParamCoverlet + 'DataCollectionRunSettings.DataCollectors.DataCollector[1].Configuration.SingleHit=true'
    $testParamMsCodeCoverage = $testParam + 'DataCollectionRunSettings.DataCollectors.DataCollector[1].@enabled=false'

    dotnet build $projectPath
    Remove-Item $resultPath -Recurse -ErrorAction Ignore

    dotnet test $testParamCoverlet
    reportgenerator $reportParam
    Rename-Item $logPath 'logs-coverlet'
    Rename-Item $reportPath 'report-coverlet'

    dotnet test $testParamCoverletSingleHit
    reportgenerator $reportParamRawMode
    Rename-Item $logPath 'logs-coverlet-singlehit-rawmode'
    Rename-Item $reportPath 'report-coverlet-singlehit-rawmode'

    dotnet test $testParamMsCodeCoverage
    reportgenerator $reportParam
    Rename-Item $logPath 'logs-mscodecoverage'
    Rename-Item $reportPath 'report-mscodecoverage'

    dotnet test $testParamMsCodeCoverage
    reportgenerator $reportParamRawMode
    Rename-Item $logPath 'logs-mscodecoverage-rawmode'
    Rename-Item $reportPath 'report-mscodecoverage-rawmode'
}
finally {
    $PSNativeCommandUseErrorActionPreference = $backupPreference
}
