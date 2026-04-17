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
    private Dictionary<RealSource, ReportedMetrics> _reportedMetrics = new(ReferenceEqualityComparer.Instance); // Cobertura only
    private bool _frozen;

    public ParsedAssembly ParentAssembly { get; }
    public RealType RealType { get; }
    public string Name { get; }
    public string OriginalName { get; }
    public string TypeName { get; internal set; }
    public IReadOnlyCollection<ParsedMethod> Methods => _methods;

    public ParsedReport ParentReport => ParentAssembly.ParentReport;

    public ReportedMetrics? ReportedMetricsForSource(RealSource realSource)
        => _reportedMetrics.TryGetValue(realSource, out var metrics) ? metrics : null;

    public ParsedType(ParsedAssembly parentAssembly, RealType realType, string name)
    {
        ParentAssembly = parentAssembly;
        RealType = realType;
        OriginalName = name;
        if (name.EndsWith('>'))
        {
            int pos = SymbolNameHelper.LastIndexOfBalancedBraces(name);
            if (pos > 0)
            {
                name = string.Concat(name.AsSpan(0, pos), SymbolNameHelper.NormalizeCommaSpacing(name[pos..]));
            }
        }
        Name = name;
        TypeName = name;
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_reportedMetrics.All(kvp => kvp.Key.Order != 0));
        foreach (var method in _methods) method.Freeze();
        Debug.Assert(_methods.All(m => m.RealSource.Order != 0));
        _methods = _methods.OrderBy(m => m.RealSource.Order).ThenBy(m => m.BoundingRange).ThenBy(m => m.Name).ToList();
        _reportedMetrics = _reportedMetrics.OrderBy(kvp => kvp.Key.Order).ToDictionary(_reportedMetrics.Comparer);
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
        if (!_reportedMetrics.SequenceEqual(other._reportedMetrics)) return false;
        return _methods.SequenceEqual(other._methods);
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
}
