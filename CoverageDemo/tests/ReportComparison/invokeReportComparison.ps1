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

    $reportParam = @('-reporttypes:Html,Markdown,Cobertura', "-targetdir:$reportPath")
    $testParam = @('--no-build', '--collect')
    $configs = @('Debug', 'Release')

    Remove-Item $resultPath -Recurse -ErrorAction Ignore
    foreach ($config in $configs) {
        Remove-Item "$resultPath-$config" -Recurse -ErrorAction Ignore

        $projectParam = @($projectPath, '--configuration', $config)
        dotnet build $projectParam

        $testId = 'coverlet'
        dotnet test $projectParam $testParam 'XPlat Code Coverage'
        $reportSource = "$resultPath/$testId.cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$resultPath/$testId.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        Rename-Item $reportPath "report-$testId"

        $testId = 'coverlet-singlehit-rawmode'
        dotnet test $projectParam $testParam 'XPlat Code Coverage' '--' 'DataCollectionRunSettings.DataCollectors.DataCollector[1].Configuration.SingleHit=true'
        $reportSource = "$resultPath/$testId.cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$resultPath/$testId.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource" 'settings:rawMode=true'
        Rename-Item $reportPath "report-$testId"

        $testId = 'mscc-multi'
        dotnet test $projectParam $testParam 'Code Coverage;Format=cobertura,xml'
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$resultPath/$testId.cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$resultPath/$testId.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-xml'
        dotnet test $projectParam $testParam 'Code Coverage;Format=xml'
        $reportSource = "$resultPath/$testId.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        Rename-Item $reportPath "report-$testId"

        $testId = 'mscc-cobertura'
        dotnet test $projectParam $testParam 'Code Coverage'
        $reportSource = "$resultPath/$testId.cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        Rename-Item $reportPath "report-$testId"
        $testId = 'mscc-cobertura-rawmode'
        reportgenerator $reportParam "-reports:$reportSource" 'settings:rawMode=true'
        Rename-Item $reportPath "report-$testId"

        $xmlfiles = Get-ChildItem -LiteralPath $resultPath -Filter '*.xml' | Select-Object -ExpandProperty FullName
        foreach ($xmlfile in $xmlfiles) {
            $destination = [System.IO.Path]::ChangeExtension($xmlfile, '.diffable.xml')
            & "$PSScriptRoot/convertToDiffableXml.ps1" $xmlfile $destination
        }

        Rename-Item $resultPath "TestResults-$config"
    }
}
finally {
    $PSNativeCommandUseErrorActionPreference = $backupPreference
}
