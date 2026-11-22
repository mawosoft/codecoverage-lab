# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.IO

# TODO Not sure yet what's the best way to handle this
[Flags()]
enum ArtifactKind {
    NonReport       = 0
    Report          = 0x01
    CollectedReport = 0x03
    MergedReport    = 0x05
    MergedInternal  = 0x0D
    MergedExternal  = 0x15
}

class ArtifactName : ICloneable {
    [string]$ReportMoniker
    [Object]$ReportVersion
    [array]$ReportOptions
    [string]$ReportFormat
    [string]$MergeMoniker
    [Object]$MergeVersion
    [array]$MergeOptions
    [string]$MergeFormat
    [string]$Directory
    [string]$Suffix
    [bool]$IsReportArtifact

    ArtifactName() {}

    ArtifactName([string]$reportMoniker, [Object]$reportVersion) {
        $this.ReportMoniker = $reportMoniker
        $this.ReportVersion = $reportVersion
    }

    [string]GetActualFormat() {
        if ($this.MergeMoniker -and $this.MergeFormat) { return $this.MergeFormat }
        return $this.ReportFormat
    }

    [ArtifactKind]GetArtifactKind() {
        if (-not $this.IsReportArtifact) { return [ArtifactKind]::NonReport }
        if (-not $this.MergeMoniker) { return [ArtifactKind]::CollectedReport }
        if ($this.MergeMoniker -eq $this.ReportMoniker -and $this.MergeVersion -eq $this.ReportVersion) {
            return [ArtifactKind]::MergedInternal
        }
        return [ArtifactKind]::MergedExternal
    }

    [ArtifactName]AsReportArtifact() {
        if (-not $this.GetActualFormat()) { throw [InvalidOperationException]::new() }
        $result = [ArtifactName]$this.Clone()
        $result.IsReportArtifact = $true
        return $result
    }

    [ArtifactName]AsOtherArtifact([string]$suffix) {
        $result = [ArtifactName]$this.Clone()
        $result.Suffix = $suffix
        $result.IsReportArtifact = $false
        return $result
    }

    [ArtifactName]WithoutFormat() {
        $result = [ArtifactName]$this.Clone()
        $result.ReportFormat = ''
        $result.MergeFormat = ''
        $result.IsReportArtifact = $false
        return $result
    }

    [ArtifactName]WithoutMerge() {
        $result = [ArtifactName]$this.Clone()
        $result.MergeMoniker = ''
        return $result
    }

    [ArtifactName]WithReport([string]$format) {
        $result = [ArtifactName]$this.Clone()
        $result.ReportFormat = $format
        $result.IsReportArtifact = $true
        return $result
    }

    [ArtifactName]WithReport([array]$options, [string]$format) {
        $result = [ArtifactName]$this.Clone()
        $result.ReportOptions = $options
        $result.ReportFormat = $format
        $result.IsReportArtifact = $true
        return $result
    }

    [ArtifactName]WithMerge([string]$format) {
        if (-not $this.ReportFormat) { throw [InvalidOperationException]::new() }
        $result = [ArtifactName]$this.Clone()
        $result.MergeMoniker = $result.ReportMoniker
        $result.MergeVersion = $result.ReportVersion
        $result.MergeOptions = $null
        $result.MergeFormat = $format
        $result.IsReportArtifact = $true
        return $result
    }

    [ArtifactName]WithMerge([string]$moniker, [Object]$version, [array]$options, [string]$format) {
        if (-not $this.ReportFormat) { throw [InvalidOperationException]::new() }
        $result = [ArtifactName]$this.Clone()
        $result.MergeMoniker = $moniker
        $result.MergeVersion = $version
        $result.MergeOptions = $options
        $result.MergeFormat = $format
        $result.IsReportArtifact = $true
        return $result
    }

    [void]ConvertToNugetVersion([type]$nugetVersionType) {
        if ($this.ReportVersion -and $this.ReportVersion.GetType() -ne $nugetVersionType) {
            $this.ReportVersion = $nugetVersionType::Parse($this.ReportVersion)
        }
        if ($this.MergeVersion -and $this.MergeVersion.GetType() -ne $nugetVersionType) {
            $this.MergeVersion = $nugetVersionType::Parse($this.MergeVersion)
        }
    }

    [Object]Clone() {
        $clone = $this.MemberwiseClone()
        if ($null -ne $this.ReportOptions) { $clone.ReportOptions = $this.ReportOptions.Clone() }
        if ($null -ne $this.MergeOptions) { $clone.MergeOptions = $this.MergeOptions.Clone() }
        return $clone
    }

    [string]ToString() {
        $name = ''
        $kind = $this.GetArtifactKind()
        if ($kind -eq [ArtifactKind]::MergedExternal) {
            $name = "$($this.MergeMoniker) $($this.MergeVersion)"
            if ($this.MergeOptions) {
                $name += ' -' + [string]::Join(' -', $this.MergeOptions) + ', '
            }
            else {
                $name += ' --, '
            }
        }
        $name += "$($this.ReportMoniker) $($this.ReportVersion)"
        if ($this.ReportOptions) {
            $name += ' -' + [string]::Join(' -', $this.ReportOptions)
        }
        else {
            $name += ' --'
        }
        if ($this.ReportFormat) {
            $name += ' ' + $this.ReportFormat
        }
        if ($kind -band [ArtifactKind]::MergedReport -and $this.MergeFormat) {
            $name += ' → ' + $this.MergeFormat
        }
        if ($this.IsReportArtifact) {
            $name += $this.GetActualFormat() -eq 'mscc-bin' ? '.coverage' : '.xml'
        }
        elseif ($this.Suffix) {
            if ($this.Suffix -notlike '[. ]*' ) { $name += ' ' }
            $name += $this.Suffix
        }
        # TODO Should we add Directory here or should caller do it?
        return $name
    }

    static [ArtifactName]Parse([string]$value) {
        $result = [ArtifactName]::new()
        $result.Directory = [Path]::GetDirectoryName($value)
        $name = [Path]::GetFileName($value)
        $m = [ArtifactName]::s_rxParse.Match($name)
        if (-not $m.Success) { throw [FormatException]::new() }
        $result.MergeMoniker = $m.Groups['moniker2'].Value
        $result.MergeVersion = $m.Groups['version2'].Value
        if ($m.Groups['option2'].Success) {
            $result.MergeOptions = $m.Groups['option2'].Captures.Value
        }
        $result.ReportMoniker = $m.Groups['moniker'].Value
        $result.ReportVersion = $m.Groups['version'].Value
        if ($m.Groups['option'].Success) {
            $result.ReportOptions = $m.Groups['option'].Captures.Value
        }
        $fmt = $result.ReportFormat = $m.Groups['format'].Value
        $result.MergeFormat = $m.Groups['format2'].Value
        if ($result.MergeFormat) {
            $fmt = $result.MergeFormat
            if (-not $result.MergeMoniker) {
                $result.MergeMoniker = $result.ReportMoniker
                $result.MergeVersion = $result.ReportVersion
            }
        }
        $ext = $result.Suffix = $name.Substring($m.Length)
        if (($ext -eq '.xml' -and $fmt -and $fmt -ne 'mscc-bin') -or
            ($ext -eq '.coverage' -and $fmt -eq 'mscc-bin')) {
            $result.IsReportArtifact = $true
            $result.Suffix = ''
        }
        return $result
    }

    static [ArtifactName]TryParse([string]$value) {
        try { return [ArtifactName]::Parse($value) } catch { return $null }
    }

    hidden static [regex]$s_rxParse = [regex]::new(@'
(?nx)
^
(
    (?<moniker2>[\w\-]+) \s (?<version2>\d[\w\-.]+)
    (\s-- | (\s-(?<option2>\w+))+)
    ,\s
    (?<moniker>[\w\-]+) \s (?<version>\d[\w\-.]+)
    (\s-- | (\s-(?<option>\w+))+)
    \s (?<format>[\w\-]+) \s → \s (?<format2>[\w\-]+)
|
    (?<moniker>[\w\-]+) \s (?<version>\d[\w\-.]+)
    (\s-- | (\s-(?<option>\w+))+)
    (
        \s (?<format>[\w\-]+) \s → \s (?<format2>[\w\-]+)
    |
        # Optional format must be explicit.
        # Otherwise we may capture some suffix.
        (?i:\s (?<format>cobertura|mscc-bin|mscc-xml) \b )?
    )
)
'@, [System.Text.RegularExpressions.RegexOptions]::ECMAScript)
}
