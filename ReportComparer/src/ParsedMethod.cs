// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{Name}")]
internal sealed class ParsedMethod : IEquatable<ParsedMethod>
{
    private List<ParsedRange> _ranges = [];
    private bool _frozen;

    public ParsedReport ParentReport => ParentAssembly.ParentReport;
    public ParsedAssembly ParentAssembly => ParentType.ParentAssembly;
    public ParsedType ParentType { get; }
    public RealMethod? RealMethod { get; internal set; }
    public string Name { get; }
    public ParsedRange BoundingRange { get; private set; } = null!;
    public IReadOnlyCollection<ParsedRange> Ranges => _ranges;
    public ReportedMetrics? ReportedMetrics { get; }

    public ParsedMethod(ParsedType parentType, string name, ReportedMetrics? reportedMetrics)
    {
        ParentType = parentType;
        Name = name;
        ReportedMetrics = reportedMetrics;
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_ranges.All(r => r.RealSource.Order != 0));
        _ranges = _ranges.OrderBy(r => r.RealSource.Order).ThenBy(r => r.Range).ToList();
        var source = _ranges[0].ParentSource;
        BoundingRange = new ParsedRange(
            this,
            source,
            Range.GetBoundingRange(_ranges.TakeWhile(r => r.ParentSource == source).Select(r => r.Range)),
            CoverageStatus.None,
            null);
        _frozen = true;
    }

    public ParsedRange AddRange(
        ParsedSource source,
        Range range,
        CoverageStatus status,
        CoberturaCoverage? coberturaCoverage)
    {
        Debug.Assert(!_frozen);
        var parsedRange = new ParsedRange(this, source, range, status, coberturaCoverage);
        source.AddRange(parsedRange);
        _ranges.Add(parsedRange);
        return parsedRange;
    }

    public bool Equals([NotNullWhen(true)] ParsedMethod? other)
    {
        Debug.Assert(_frozen);
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        if (Name != other.Name) return false;
        if (EquatableSequence.Create(_ranges) != EquatableSequence.Create(other._ranges)) return false;
        if (ReportedMetrics != other.ReportedMetrics) return false;
        return true;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ParsedMethod);

    public override int GetHashCode()
    {
        Debug.Assert(_frozen);
        return HashCode.Combine(
            Name,
            EquatableSequence.Create(_ranges),
            ReportedMetrics);
    }
}
