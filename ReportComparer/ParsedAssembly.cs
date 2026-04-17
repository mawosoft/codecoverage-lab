// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{FullName}")]
internal sealed class ParsedAssembly : IEquatable<ParsedAssembly>
{
    private Dictionary<RealSource, ParsedSource> _sources = new(ReferenceEqualityComparer.Instance);
    private Dictionary<string, ParsedType> _types = [];
    private bool _frozen;

    public ParsedReport ParentReport { get; }
    public RealAssembly RealAssembly { get; }
    public string FullName { get; }
    public IReadOnlyCollection<ParsedSource> Sources => _sources.Values;
    public IReadOnlyCollection<ParsedType> Types => _types.Values;
    public ReportedMetrics? ReportedMetrics { get; set; }

    public ParsedAssembly(ParsedReport parentReport, RealAssembly realAssembly, string name)
    {
        ParentReport = parentReport;
        RealAssembly = realAssembly;
        FullName = name;
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_sources.All(kvp => kvp.Key.Order != 0));
        _sources = _sources.OrderBy(kvp => kvp.Key.Order).ToDictionary(_sources.Comparer);
        _types = _types.OrderBy(kvp => kvp.Key).ToDictionary(_types.Comparer);
        foreach (var type in _types.Values) type.Freeze();
        foreach (var source in _sources.Values) source.Freeze();
        _frozen = true;
    }

    public ParsedSource GetOrAddSource(string fullName)
    {
        Debug.Assert(!_frozen);
        var realSource = RealAssembly.GetOrAddRealSource(fullName);
        if (_sources.TryGetValue(realSource, out var parsedSource)) return parsedSource;
        parsedSource = new ParsedSource(this, realSource);
        realSource.AddParsedSource(parsedSource);
        _sources.Add(realSource, parsedSource);
        return parsedSource;
    }

    public ParsedType GetOrAddType(string name)
    {
        Debug.Assert(!_frozen);
        if (_types.TryGetValue(name, out var parsedType)) return parsedType;
        string normalizedName = SymbolNameHelper.NormalizeTypeName(name);
        var realType = RealAssembly.GetOrAddRealType(normalizedName);
        parsedType = new ParsedType(this, realType, name);
        realType.AddParsedType(parsedType);
        _types.Add(name, parsedType);
        return parsedType;
    }

    public bool Equals([NotNullWhen(true)] ParsedAssembly? other)
    {
        Debug.Assert(_frozen);
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        if (!ReferenceEquals(RealAssembly, other.RealAssembly)) return false;
        if (ReportedMetrics != other.ReportedMetrics) return false;
        return _types.SequenceEqual(other._types);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ParsedAssembly);

    public override int GetHashCode()
    {
        Debug.Assert(_frozen);
        return HashCode.Combine(
            RealAssembly,
            ReportedMetrics,
            EquatableSequence.Create(_types));
    }
}
