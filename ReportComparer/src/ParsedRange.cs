// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ReportComparer;

[DebuggerDisplay("{Status}, {Range} in {RealSource}")]
internal sealed class ParsedRange : IEquatable<ParsedRange>
{
    public ParsedReport ParentReport => ParentAssembly.ParentReport;
    public ParsedAssembly ParentAssembly => ParentType.ParentAssembly;
    public ParsedType ParentType => ParentMethod.ParentType;
    public ParsedMethod ParentMethod { get; }
    public ParsedSource ParentSource { get; }
    public RealSource RealSource => ParentSource.RealSource;
    public Range Range { get; }
    public CoverageStatus Status { get; }
    public CoberturaCoverage? Cobertura { get; }

    public ParsedRange(
        ParsedMethod parentMethod,
        ParsedSource parentSource,
        Range range,
        CoverageStatus status,
        CoberturaCoverage? coberturaCoverage)
    {
        ParentMethod = parentMethod;
        ParentSource = parentSource;
        Range = range;
        Status = status;
        Cobertura = coberturaCoverage;
    }

    public bool Equals([NotNullWhen(true)] ParsedRange? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        if (!ReferenceEquals(RealSource, other.RealSource)) return false;
        if (Range != other.Range) return false;
        if (Status != other.Status) return false;
        if (Cobertura != other.Cobertura) return false;
        return true;
    }

    public static CoverageStatus AggregateStatus(IEnumerable<ParsedRange> parsedRanges)
    {
        CoverageStatus status = default;
        foreach (var parsedRange in parsedRanges)
        {
            if (parsedRange.Status != status)
            {
                Debug.Assert(status == default || parsedRange.Status != default);
                status = status == default ? parsedRange.Status : CoverageStatus.Partial;
#if !DEBUG
                if (status == CoverageStatus.Partial) break;
#endif
            }
        }
        return status;
    }

    public static bool OverlapsAt(int lineNumber, ParsedRange[] parsedRanges)
    {
        for (int i = 0; i < parsedRanges.Length; i++)
        {
            var pr = parsedRanges[i];
            if (!pr.Range.Overlaps(lineNumber)) continue;
            for (int k = i + 1; k < parsedRanges.Length; k++)
            {
                var r = pr.Range.Intersect(parsedRanges[k].Range);
                if (r.IsValid() && r.Overlaps(lineNumber)) return true;
            }
        }
        return false;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ParsedRange);

    public override int GetHashCode() => HashCode.Combine(RealSource, Range, Status, Cobertura);
}
