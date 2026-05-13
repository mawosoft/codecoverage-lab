# Copyright (c) Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Force-pushes a single orphaned commit with the content of the specified folder
    to the gh-pages branch of the remote origin of the current repository.
.DESCRIPTION
    The script creates a temporary git repo in the specified folder. As such, .gitignore
    and .gitattributes files within the folder hierarchy apply relative to that folder.
    Config settings are taken from the global and system scope, which must provide any
    credentials needed. Settings from the parent repo have no effect.

    The script checks that the folder itself is not a git repo or worktree and that it
    contains an index.html file at its root.
#>

[CmdletBinding()]
param(
    # Path of the directory to publish. Defaults to the current directory.
    [string]$Path,
    # Publishes an empty commit. The Path parameter is ignored.
    [switch]$Clear,
    # Skips the confirmation prompt before invoking 'git push'.
    [switch]$Force
)

$PSNativeCommandUseErrorActionPreference = $true
$remoteUrl = git remote get-url origin --push
$startupDir = (Get-Location -PSProvider 'FileSystem').Path
try {
    if ($Clear) {
        $Path = [System.IO.Directory]::CreateTempSubdirectory()
    }
    elseif (-not $Path) {
        $Path = $startupDir
    }
    else {
        $Path = Convert-Path -LiteralPath $Path
    }
    # This fails if path is not an existing directory
    Set-Location -LiteralPath $Path
    if (-not $Clear) {
        if (-not (Test-Path -LiteralPath 'index.html' -PathType Leaf)) {
            throw 'Root must contain index.html.'
        }
        $isWorktree = Test-Path -LiteralPath '.git'
        if (-not $isWorktree) {
            $result = git worktree list --porcelain
            $result = $result.Where({ $_ -like 'worktree *' }).ForEach({ Convert-Path -LiteralPath $_.SubString(9) })
            $isWorktree = $Path -in $result
        }
        if ($isWorktree) {
            throw 'Already a git worktree: ' + $Path
        }
    }
    git init -b 'gh-pages'
    git config set core.safecrlf false # Disable eol conversion warnings
    git add --all
    git commit -m 'Update gh-pages.' --allow-empty --quiet
    git show --shortstat --oneline
    $choice = $Force ? 0 : $Host.UI.PromptForChoice(
        $null,
        "Force-push gh-pages to $remoteUrl ?",
        @(
            '&Yes', '&No',
            [System.Management.Automation.Host.ChoiceDescription]::new(
                '&Abort',
                'Same as No, but does not delete the temporary repo.'
            )
        ),
        1
    )
    if ($choice -eq 0) {
        git push -u -f $remoteUrl 'gh-pages'
    }
    if ($choice -eq 2) {
        # Unset -Clear to keep temp dir on Abort
        $Clear = $false
    }
    elseif (-not $Clear) {
        Remove-Item -LiteralPath '.git' -Recurse -Force
    }
}
finally {
    Set-Location -LiteralPath $startupDir
    if ($Clear) {
        Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction Ignore
    }
}
