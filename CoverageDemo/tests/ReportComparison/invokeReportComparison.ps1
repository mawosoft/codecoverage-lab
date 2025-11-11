# Copyright (c) Matthias Wolf, Mawosoft.

[CmdletBinding()]
param()

[bool]$backupPreference = $PSNativeCommandUseErrorActionPreference
try {
    $PSNativeCommandUseErrorActionPreference = $true

    $resultPath = "$PSScriptRoot/TestResults"
    $logPath = "$resultPath/logs"
    $reportPath = "$resultPath/report"
    $projectPath = "$PSScriptRoot/ReportComparison.csproj"

    $reportParam = @(
        "-reports:$logPath/*/*cobertura.xml"
        "-targetdir:$reportPath"
        '-reporttypes:Html,Markdown,Cobertura'
    )
    $reportParamRawMode = $reportParam + 'settings:rawMode=true'

    $testParam = @('--no-build', '--collect')
    $testParamCoverlet = $testParam + 'XPlat Code Coverage'
    $testParamCoverletSingleHit = $testParamCoverlet + @('--', 'DataCollectionRunSettings.DataCollectors.DataCollector[1].Configuration.SingleHit=true')
    $testParamMsCodeCoverage = $testParam + 'Code Coverage'

    $configs = @('Debug', 'Release')

    Remove-Item $resultPath -Recurse -ErrorAction Ignore
    foreach ($config in $configs) {
        Remove-Item "$resultPath-$config" -Recurse -ErrorAction Ignore

        $projectParam = @($projectPath, '--configuration', $config)
        dotnet build $projectParam

        dotnet test $projectParam $testParamCoverlet
        reportgenerator $reportParam
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$logPath/coverlet.cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$logPath/coverlet.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        Rename-Item $logPath 'logs-coverlet'
        Rename-Item $reportPath 'report-coverlet'

        dotnet test $projectParam $testParamCoverletSingleHit
        reportgenerator $reportParamRawMode
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$logPath/coverlet.cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$logPath/coverlet.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        Rename-Item $logPath 'logs-coverlet-singlehit-rawmode'
        Rename-Item $reportPath 'report-coverlet-singlehit-rawmode'

        dotnet test $projectParam $testParamMsCodeCoverage
        reportgenerator $reportParam
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$logPath/mscc.cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        Rename-Item $logPath 'logs-mscc'
        Rename-Item $reportPath 'report-mscc'

        dotnet test $projectParam $testParamMsCodeCoverage
        reportgenerator $reportParamRawMode
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$logPath/mscc.cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        Rename-Item $logPath 'logs-mscc-rawmode'
        Rename-Item $reportPath 'report-mscc-rawmode'

        Rename-Item $resultPath "TestResults-$config"
    }
}
finally {
    $PSNativeCommandUseErrorActionPreference = $backupPreference
}
