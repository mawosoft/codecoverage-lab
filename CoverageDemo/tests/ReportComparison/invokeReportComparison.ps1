# Copyright (c) Matthias Wolf, Mawosoft.

[CmdletBinding()]
param()

[bool]$backupPreference = $PSNativeCommandUseErrorActionPreference
try {
    $PSNativeCommandUseErrorActionPreference = $true

    $resultPath = "$PSScriptRoot/TestResults"
    $logPath = "$resultPath/logs"
    $diagPath = "$resultPath/diag"
    $diffablePath = "$resultPath/diffable"
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

        $testId = 'coverlet-cobertura'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=cobertura' --diag "$diagPath/$testId.log"
        $reportSource = "$resultPath/$testId.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        Rename-Item $reportPath "report-$testId"

        $testId = 'coverlet-cobertura-singlehit'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=cobertura' --diag "$diagPath/$testId.log" '--' 'DataCollectionRunSettings.DataCollectors.DataCollector[1].Configuration.SingleHit=true'
        $reportSource = "$resultPath/$testId.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource" 'settings:rawMode=true'
        Rename-Item $reportPath "report-$testId-rawmode"

        $testId = 'coverlet-json'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=json' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$resultPath/$testId.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'coverlet-multi(json+cobertura)'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=json,cobertura' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$resultPath/$testId-json.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'coverlet-multi(cobertura+json)'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=cobertura,json' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$resultPath/$testId-json.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-xml'
        dotnet test $projectParam $testParam 'Code Coverage;Format=xml' --diag "$diagPath/$testId.log"
        $reportSource = "$resultPath/$testId.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        Rename-Item $reportPath "report-$testId"
        dotnet-coverage merge $reportSource --output-format cobertura --output "$resultPath/merged-$testId-cobertura.xml"

        $testId = 'mscc-cobertura'
        dotnet test $projectParam $testParam 'Code Coverage;Format=cobertura' --diag "$diagPath/$testId.log"
        $reportSource = "$resultPath/$testId.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        Rename-Item $reportPath "report-$testId"
        reportgenerator $reportParam "-reports:$reportSource" 'settings:rawMode=true'
        Rename-Item $reportPath "report-$testId-rawmode"
        dotnet-coverage merge $reportSource --output-format xml --output "$resultPath/merged-$testId-xml.xml"

        $testId = 'mscc-coverage'
        dotnet test $projectParam $testParam 'Code Coverage;Format=coverage' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination "$resultPath/$testId.coverage"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        dotnet-coverage merge "$resultPath/$testId.coverage" --output-format cobertura --output "$resultPath/merged-$testId-cobertura.xml"
        dotnet-coverage merge "$resultPath/$testId.coverage" --output-format xml --output "$resultPath/merged-$testId-xml.xml"

        $testId = 'mscc-multi(coverage+cobertura)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=coverage,cobertura' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination "$resultPath/$testId-coverage.coverage"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-multi(cobertura+coverage)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=cobertura,coverage' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination "$resultPath/$testId-coverage.coverage"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-multi(xml+cobertura)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=xml,cobertura' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-xml.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-multi(cobertura+xml)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=cobertura,xml' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-xml.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-multi(coverage+xml)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=coverage,xml' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination "$resultPath/$testId-coverage.coverage"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-xml.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-multi(xml+coverage)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=xml,coverage' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination "$resultPath/$testId-coverage.coverage"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$resultPath/$testId-xml.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $null = New-Item $diffablePath -ItemType Directory
        Get-ChildItem -LiteralPath $resultPath -Filter '*.xml' | Select-Object -ExpandProperty Name | ForEach-Object {
            & "$PSScriptRoot/convertToDiffableXml.ps1" "$resultPath/$_" "$diffablePath/$_"
        }

        Rename-Item $resultPath "TestResults-$config"
    }
}
finally {
    $PSNativeCommandUseErrorActionPreference = $backupPreference
}
