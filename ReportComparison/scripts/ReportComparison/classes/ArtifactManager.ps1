# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.IO

class ArtifactManager {
    hidden [string]$_artifactDirectory
    hidden [string]$_reportDirectory
    hidden [string]$_htmlReportDirectory
    hidden [string]$_comparisonDirectory
    hidden [string]$_instrumentationDirectory
    hidden [string]$_diagDirectory
    hidden [string]$_diagMainLogFile

    ArtifactManager([string]$artifactDir) {
        $artifactDir = [Path]::GetFullPath($artifactDir)
        $this._artifactDirectory = $artifactDir
        $this._reportDirectory = [Path]::GetFullPath($artifactDir + '/reports')
        $this._htmlReportDirectory = [Path]::GetFullPath($artifactDir + '/html-reports')
        $this._comparisonDirectory = [Path]::GetFullPath($artifactDir + '/comparisons')
        $this._instrumentationDirectory = [Path]::GetFullPath($artifactDir + '/instrumentations')
    }

    [string]GetArtifactDirectory() {
        return [Directory]::CreateDirectory($this._artifactDirectory).FullName
    }

    [string]GetReportDirectory([string]$config, [ArtifactKind]$reportKind) {
        $path = ''
        if ($config) {
            if ($reportKind -eq [ArtifactKind]::CollectedReport) {
                $path = '/c'
            }
            elseif ($reportKind -band [ArtifactKind]::MergedReport) {
                $path = '/m'
            }
            $path = $config.ToLowerInvariant() + $path
        }
        return [Directory]::CreateDirectory([Path]::Combine($this._reportDirectory, $path)).FullName
    }

    [string]GetHtmlReportDirectory([string]$config) {
        return [Directory]::CreateDirectory([Path]::Combine($this._htmlReportDirectory, $config.ToLowerInvariant())).FullName
    }

    [string]GetComparisonDirectory([string]$config) {
        return [Directory]::CreateDirectory([Path]::Combine($this._comparisonDirectory, $config.ToLowerInvariant())).FullName
    }

    [string]GetInstrumententationDirectory([string]$config) {
        return [Directory]::CreateDirectory([Path]::Combine($this._instrumentationDirectory, $config.ToLowerInvariant())).FullName
    }

    [string]GetDiagDirectory([string]$config) {
        if (-not $this._diagDirectory) {
            $diag = [Path]::GetFullPath($this._artifactDirectory + '/diag/')
            $stamp = [datetime]::Now
            do {
                $path = $diag + $stamp.ToString('yyyy-MM-dd_hh-mm-ss_fff')
                $stamp += [timespan]::FromMilliseconds(1)
            } while ([Directory]::Exists($path))
            $this._diagDirectory = $path
        }
        return [Directory]::CreateDirectory([Path]::Combine($this._diagDirectory, $config.ToLowerInvariant())).FullName
    }

    [string]GetMainLogFile() {
        if (-not $this._diagMainLogFile) {
            $this._diagMainLogFile = [Path]::Combine($this.GetDiagDirectory(''), 'ReportComparison.log')
            [File]::Create($this._diagMainLogFile).Dispose()
        }
        return $this._diagMainLogFile
    }

    [void]EnableBinlog([string]$config) {
        $path = $this.GetDiagDirectory($config)
        $script:Context.SetEnvironmentVariable('MSBuildDebugEngine', '1')
        $script:Context.SetEnvironmentVariable('MSBUILDDEBUGPATH', $path)
    }
}
