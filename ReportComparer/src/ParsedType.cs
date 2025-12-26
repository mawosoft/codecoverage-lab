// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{Name}")]
internal sealed class ParsedType : IEquatable<ParsedType>
{
    private List<ParsedMethod> _methods = [];
    private Dictionary<RealSource, ReportedMetrics> _reportedMetrics = [];
    private bool _frozen;

    public ParsedReport ParentReport => ParentAssembly.ParentReport;
    public ParsedAssembly ParentAssembly { get; }
    public RealType? RealType { get; internal set; }
    public string Name { get; }
    public string ShortName => field is not null ? field : RealType is null || !RealType.Frozen ? Name : field = CalculateShortName();
    public IReadOnlyCollection<ParsedMethod> Methods => _methods;
    public IReadOnlyDictionary<RealSource, ReportedMetrics> ReportedMetrics => _reportedMetrics; // Cobertura only

    public ReportedMetrics? ReportedMetricsForSource(RealSource realSource)
        => _reportedMetrics.TryGetValue(realSource, out var metrics) ? metrics : null;

    public ParsedType(ParsedAssembly parentAssembly, string name)
    {
        ParentAssembly = parentAssembly;
        Name = name;
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_reportedMetrics.All(kvp => kvp.Key.Order != 0));
        foreach (var method in _methods) method.Freeze();
        Debug.Assert(_methods.All(m => m.BoundingRange.RealSource.Order != 0));
        _methods = _methods.OrderBy(m => m.BoundingRange.RealSource.Order)
            .ThenBy(m => m.BoundingRange.Range)
            .ThenBy(m => m.Name)
            .ToList();
        _reportedMetrics = _reportedMetrics.OrderBy(kvp => kvp.Key.Order).ToDictionary();
        _frozen = true;
    }

    public ParsedMethod AddMethod(string name, ReportedMetrics? reportedMetrics)
    {
        Debug.Assert(!_frozen);
        var method = new ParsedMethod(this, name, reportedMetrics);
        _methods.Add(method);
        return method;
    }

    public void AddReportedMetrics(ParsedSource source, ReportedMetrics reportedMetrics)
    {
        Debug.Assert(!_frozen);
        _reportedMetrics.Add(source.RealSource, reportedMetrics);
    }

    public bool Equals([NotNullWhen(true)] ParsedType? other)
    {
        Debug.Assert(_frozen);
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        if (Name != other.Name) return false;
        if (EquatableSequence.Create(_methods) != EquatableSequence.Create(other._methods)) return false;
        if (EquatableSequence.Create(_reportedMetrics) != EquatableSequence.Create(other._reportedMetrics))
            return false;
        return true;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ParsedType);

    public override int GetHashCode()
    {
        Debug.Assert(_frozen);
        return HashCode.Combine(
            Name,
            EquatableSequence.Create(_methods),
            EquatableSequence.Create(_reportedMetrics));
    }

    private string CalculateShortName()
    {
        Debug.Assert(RealType is not null && RealType.Frozen);
        string @namespace = RealType.Namespace;
        return @namespace.Length != 0
               && Name.Length > @namespace.Length
               && Name[@namespace.Length] == '.'
               && Name.StartsWith(@namespace, StringComparison.Ordinal)
            ? Name[(@namespace.Length + 1)..]
            : Name;
    }
}
