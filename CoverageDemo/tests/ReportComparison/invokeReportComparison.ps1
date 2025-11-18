# Copyright (c) Matthias Wolf, Mawosoft.

[CmdletBinding()]
param()

[bool]$backupPreference = $PSNativeCommandUseErrorActionPreference
try {
    $PSNativeCommandUseErrorActionPreference = $true

    $resultPath = "$PSScriptRoot/TestResults"
    $logPath = "$resultPath/logs"
    $diagPath = "$resultPath/diag"
    $coberturaPath = "$resultPath/cobertura"
    $msccxmlPath = "$resultPath/mscc-xml"
    $markdownPath = "$resultPath/report-markdown"
    $htmlPath = "$resultPath/report-html"
    $diffableCoberturaPath = "$resultPath/diffable/cobertura"
    $diffableMsccxmlPath = "$resultPath/diffable/mscc-xml"
    $reportPath = "$resultPath/report"
    $projectPath = "$PSScriptRoot/ReportComparison.csproj"

    $reportParam = @('-reporttypes:Html,Markdown,Cobertura', "-targetdir:$reportPath")
    $testParam = @('--no-build', '--collect')
    $configs = @('Debug', 'Release')

    foreach ($config in $configs) {
        Remove-Item "$resultPath-$config" -Recurse -ErrorAction Ignore
        Remove-Item $resultPath -Recurse -ErrorAction Ignore
        $null = New-Item $coberturaPath -ItemType Directory
        $null = New-Item $msccxmlPath -ItemType Directory
        $null = New-Item $markdownPath -ItemType Directory
        $null = New-Item $htmlPath -ItemType Directory
        $null = New-Item $diffableCoberturaPath -ItemType Directory
        $null = New-Item $diffableMsccxmlPath -ItemType Directory

        $projectParam = @($projectPath, '--configuration', $config)
        dotnet build $projectParam

        $testId = 'coverlet-cobertura'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=cobertura' --diag "$diagPath/$testId.log"
        $reportSource = "$coberturaPath/$testId.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        $reportId = "report-$testId"
        Move-Item "$reportPath/Summary.md" "$markdownPath/$reportId.md"
        Move-Item "$reportPath/Cobertura.xml" "$coberturaPath/$reportId.xml"
        Move-Item $reportPath "$htmlPath/$reportId"

        $testId = 'coverlet-cobertura-singlehit'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=cobertura' --diag "$diagPath/$testId.log" '--' 'DataCollectionRunSettings.DataCollectors.DataCollector[1].Configuration.SingleHit=true'
        $reportSource = "$coberturaPath/$testId.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource" 'settings:rawMode=true'
        $reportId = "report-$testId-rawmode"
        Move-Item "$reportPath/Summary.md" "$markdownPath/$reportId.md"
        Move-Item "$reportPath/Cobertura.xml" "$coberturaPath/$reportId.xml"
        Move-Item $reportPath "$htmlPath/$reportId"

        $testId = 'coverlet-json'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=json' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$resultPath/$testId.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'coverlet-multi(json+cobertura)'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=json,cobertura' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$coberturaPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$resultPath/$testId-json.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'coverlet-multi(cobertura+json)'
        dotnet test $projectParam $testParam 'XPlat Code Coverage;Format=cobertura,json' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$coberturaPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter 'coverage.json' -Recurse | Move-Item -Destination "$resultPath/$testId-json.json"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-xml'
        dotnet test $projectParam $testParam 'Code Coverage;Format=xml' --diag "$diagPath/$testId.log"
        $reportSource = "$msccxmlPath/$testId.xml"
        $mergeSource = $reportSource
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        $reportId = "report-$testId"
        Move-Item "$reportPath/Summary.md" "$markdownPath/$reportId.md"
        Move-Item "$reportPath/Cobertura.xml" "$coberturaPath/$reportId.xml"
        Move-Item $reportPath "$htmlPath/$reportId"
        dotnet-coverage merge $mergeSource --output-format cobertura --output "$coberturaPath/merged-$testId-to-cobertura.xml"

        $testId = 'mscc-cobertura'
        dotnet test $projectParam $testParam 'Code Coverage;Format=cobertura' --diag "$diagPath/$testId.log"
        $reportSource = "$coberturaPath/$testId.xml"
        $mergeSource = $reportSource
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination $reportSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        reportgenerator $reportParam "-reports:$reportSource"
        $reportId = "report-$testId"
        Move-Item "$reportPath/Summary.md" "$markdownPath/$reportId.md"
        Move-Item "$reportPath/Cobertura.xml" "$coberturaPath/$reportId.xml"
        Move-Item $reportPath "$htmlPath/$reportId"
        reportgenerator $reportParam "-reports:$reportSource" 'settings:rawMode=true'
        $reportId = "report-$testId-rawmode"
        Move-Item "$reportPath/Summary.md" "$markdownPath/$reportId.md"
        Move-Item "$reportPath/Cobertura.xml" "$coberturaPath/$reportId.xml"
        Move-Item $reportPath "$htmlPath/$reportId"
        dotnet-coverage merge $mergeSource --output-format xml --output "$msccxmlPath/merged-$testId-to-xml.xml"

        $testId = 'mscc-coverage'
        dotnet test $projectParam $testParam 'Code Coverage;Format=coverage' --diag "$diagPath/$testId.log"
        $mergeSource = "$resultPath/$testId.coverage"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination $mergeSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        dotnet-coverage merge $mergeSource --output-format cobertura --output "$coberturaPath/merged-$testId-to-cobertura.xml"
        dotnet-coverage merge $mergeSource --output-format xml --output "$msccxmlPath/merged-$testId-to-xml.xml"

        $testId = 'mscc-multi(coverage+cobertura)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=coverage,cobertura' --diag "$diagPath/$testId.log"
        $mergeSource = "$resultPath/$testId-coverage.coverage"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$coberturaPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination $mergeSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        dotnet-coverage merge $mergeSource --output-format xml --output "$msccxmlPath/merged-$testId-coverage-to-xml.xml"

        $testId = 'mscc-multi(cobertura+coverage)'
        $mergeSource = "$resultPath/$testId-coverage.coverage"
        dotnet test $projectParam $testParam 'Code Coverage;Format=cobertura,coverage' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$coberturaPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination $mergeSource
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        dotnet-coverage merge $mergeSource --output-format xml --output "$msccxmlPath/merged-$testId-coverage-to-xml.xml"

        $testId = 'mscc-multi(xml+cobertura)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=xml,cobertura' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$coberturaPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$msccxmlPath/$testId-xml.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-multi(cobertura+xml)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=cobertura,xml' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.cobertura.xml' -Recurse | Move-Item -Destination "$coberturaPath/$testId-cobertura.xml"
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$msccxmlPath/$testId-xml.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse

        $testId = 'mscc-multi(coverage+xml)'
        dotnet test $projectParam $testParam 'Code Coverage;Format=coverage,xml' --diag "$diagPath/$testId.log"
        $mergeSource = "$resultPath/$testId-coverage.coverage"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination $mergeSource
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$msccxmlPath/$testId-xml.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        dotnet-coverage merge $mergeSource --output-format xml --output "$msccxmlPath/merged-$testId-coverage-to-xml.xml"

        $testId = 'mscc-multi(xml+coverage)'
        $mergeSource = "$resultPath/$testId-coverage.coverage"
        dotnet test $projectParam $testParam 'Code Coverage;Format=xml,coverage' --diag "$diagPath/$testId.log"
        Get-ChildItem -LiteralPath $logPath -Filter '*.coverage' -Recurse | Move-Item -Destination $mergeSource
        Get-ChildItem -LiteralPath $logPath -Filter '*.xml' -Recurse | Move-Item -Destination "$msccxmlPath/$testId-xml.xml"
        Get-ChildItem -LiteralPath $logPath -Directory | Remove-Item -Recurse
        dotnet-coverage merge $mergeSource --output-format xml --output "$msccxmlPath/merged-$testId-coverage-to-xml.xml"

        Get-ChildItem -LiteralPath $coberturaPath -Filter '*.xml' | ForEach-Object {
            & "$PSScriptRoot/convertToDiffableXml.ps1" $_.FullName "$diffableCoberturaPath/$($_.Name)"
        }
        Get-ChildItem -LiteralPath $msccxmlPath -Filter '*.xml' | ForEach-Object {
            & "$PSScriptRoot/convertToDiffableXml.ps1" $_.FullName "$diffableMsccxmlPath/$($_.Name)"
        }

        Rename-Item $resultPath "TestResults-$config"
    }
}
finally {
    $PSNativeCommandUseErrorActionPreference = $backupPreference
}
