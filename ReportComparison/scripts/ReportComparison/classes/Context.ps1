# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.Collections.Generic
using namespace System.IO

class Context {
    [string]$RepoRoot
    [string]$WorkingDirectory
    [string]$ProjectDirectory
    [string]$ArtifactDirectory
    [string]$StartupDirectory

    hidden [ArtifactManager]$_artifactManager
    hidden [NuGetHelper]$_nuGetHelper
    hidden [PackageManager]$_packageManager
    hidden [ProjectManager]$_projectManager
    hidden [TaskManager]$_taskManager
    hidden [TempFileManager]$_tempFileManager

    hidden [hashtable]$_envBackup

    Context() {
        # Alternatively, walk backwards to hidden .git
        $this.RepoRoot = [Path]::GetFullPath("$PSScriptRoot/../../../..")
        $this.WorkingDirectory = $this.RepoRoot
        $this.ProjectDirectory = [Path]::GetFullPath($this.RepoRoot + '/ReportComparison')
        $this.ArtifactDirectory = [Path]::GetFullPath($this.RepoRoot + '/artifacts/ReportComparisonResults')
        $this._envBackup = [hashtable]::new($global:IsWindows ? [StringComparer]::OrdinalIgnoreCase : [StringComparer]::Ordinal)
    }

    [ArtifactManager]GetArtifactManager() {
        $this._artifactManager ??= [ArtifactManager]::new($this.ArtifactDirectory)
        return $this._artifactManager
    }

    [NuGetHelper]GetNuGetHelper() {
        $this._nuGetHelper ??= [NuGetHelper]::new()
        return $this._nuGetHelper
    }

    [PackageManager]GetPackageManager() {
        $this._packageManager ??= [PackageManager]::new()
        return $this._packageManager
    }

    [ProjectManager]GetProjectManager() {
        $this._projectManager ??= [ProjectManager]::new()
        return $this._projectManager
    }

    [TaskManager]GetTaskManager() {
        $this._taskManager ??= [TaskManager]::new()
        return $this._taskManager
    }

    # TODO Should we have this as a global?
    [TempFileManager]GetTempFileManager() {
        $this._tempFileManager ??= [TempFileManager]::new()
        return $this._tempFileManager
    }

    [void]SetEnvironmentVariable([string]$name, [Object]$value) {
        if (-not $this._envBackup.ContainsKey($name)) {
            $this._envBackup[$name] = [Environment]::GetEnvironmentVariable($name)
        }
        [Environment]::SetEnvironmentVariable($name, $value ?? [NullString]::Value)
    }

    [void]Enter() {
        $this.StartupDirectory = (Get-Location -PSProvider 'FileSystem').Path
        Set-Location -LiteralPath $this.WorkingDirectory
    }

    [void]Exit() {
        $this._artifactManager = $null
        # Won't unload once it has been used (static self-reference in NuGet.Versioning)
        # $this._nuGetHelper?.Dispose()
        # $this._nuGetHelper = $null
        $this._packageManager = $null
        $this._projectManager = $null
        $this._tempFileManager?.Dispose()
        $this._tempFileManager = $null
        foreach ($kvp in $this._envBackup.GetEnumerator()) {
            [Environment]::SetEnvironmentVariable($kvp.Key, $kvp.Value ?? [NullString]::Value)
        }
        $this._envBackup.Clear()
        if ($this.StartupDirectory) {
            Set-Location -LiteralPath $this.StartupDirectory
            $this.StartupDirectory = $null
        }
    }
}
