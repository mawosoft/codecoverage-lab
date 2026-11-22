# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System

<#
.SYNOPSIS
    TODO
#>
function Invoke-ReportComparison {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Collect', 'Compare', 'Instrument', 'Test')]
        [string]$Operation,

        # TODO Version Latest/All?

        # Force operations to run regardless of existing results.
        [switch]$Force
    )

    $script:Context.Enter()
    $logfile = $script:Context.GetArtifactManager().GetMainLogFile()
    try {
        & {
            # TODO actual code
        } *>&1 | Tee-Object -LiteralPath $logfile | Out-Host
    }
    catch {
        $_ | Out-File -LiteralPath $logfile -Append
        throw
    }
    finally {
        # TODO cleanup here or nested inside actual code?
        $script:Context.Exit()
    }
}
