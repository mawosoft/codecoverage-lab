# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.Management.Automation

<#
.SYNOPSIS
    Invokes a native command and logs to the Information stream via Write-Host.
.NOTES
    - All stderr is logged after stdout. Ideally, we would use 2>&1, but PowerShell does not
      preserve the order and arbitrarily mixes output from both streams.
    - If -PassThru is specified, stdout is returned and not logged. stderr is still logged.
    - If a non-zero exit code is not ignored, an ErrorRecord is written to the Error stream
      after stderr has been logged.
#>
function InvokeNativeCommand {
    [CmdletBinding()]
    param(
        # Path or name of the native command.
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Path,

        # Command arguments.
        [Parameter(ValueFromRemainingArguments)]
        [Alias('Args')]
        $ArgumentList,

        # Returns stdout instead of logging it. stderr is still logged.
        [switch]$PassThru,

        # Ignores non-zero exit code.
        [switch]$IgnoreExitCode
    )

    # This flattens only one level, just like using a splatted array.
    $ArgumentList = $ArgumentList.PSForEach({ $_ })
    # Setting this to true prevents 2>variable:stderr from being populated.
    $PSNativeCommandUseErrorActionPreference = $false
    $stderr = $null
    $catched = $null
    $exitCode = 0
    try {
        if ($PassThru) {
            & $Path $ArgumentList 2>variable:stderr
        }
        else {
            Write-Host "NativeCommand: $Path $ArgumentList"
            & $Path $ArgumentList 2>variable:stderr | Write-Host
        }
        if (-not $IgnoreExitCode) {
            $exitCode = $LASTEXITCODE
        }
    }
    catch {
        $catched = $_
    }
    if ($stderr -or $catched -or $exitCode) {
        if ($PassThru) {
            Write-Host "NativeCommand: $Path $ArgumentList"
        }
        $stderr | Write-Host
        if ($catched) {
            $PSCmdlet.ThrowTerminatingError($catched)
        }
        elseif ($exitCode) {
            $PSCmdlet.WriteError([ErrorRecord]::new(
                    [RuntimeException]::new('Non-zero exit code: {0} (0x{0:x})' -f $exitCode),
                    'ProgramExitedWithNonZeroCode',
                    [ErrorCategory]::InvalidResult,
                    $Path))
        }
    }
}
