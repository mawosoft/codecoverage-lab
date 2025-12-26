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
    private HashSet<string> _skippedFunctions = [];
    private bool _frozen;

    public ParsedReport ParentReport { get; }
    public RealAssembly RealAssembly { get; }
    public string FullName { get; }
    public string Name => RealAssembly.Name;
    public IReadOnlyCollection<ParsedSource> Sources => _sources.Values;
    public IReadOnlyCollection<ParsedType> Types => _types.Values;
    public ReportedMetrics? ReportedMetrics { get; set; }
    public IReadOnlyCollection<string> SkippedFunctions => _skippedFunctions; // DynamicCoverage only

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
        foreach (var type in _types.Values) type.Freeze();
        _types = _types.OrderBy(kvp => kvp.Key).ToDictionary(_types.Comparer);
        _skippedFunctions = _skippedFunctions.Order().ToHashSet();
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
        if (_types.TryGetValue(name, out var type)) return type;
        type = new ParsedType(this, name);
        _types.Add(name, type);
        return type;
    }

    public void AddSkippedFunction(string name)
    {
        Debug.Assert(!_frozen);
        _skippedFunctions.Add(name);
    }

    public bool Equals([NotNullWhen(true)] ParsedAssembly? other)
    {
        Debug.Assert(_frozen);
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        if (!ReferenceEquals(RealAssembly, other.RealAssembly)) return false;
        if (EquatableSequence.Create(_types) != EquatableSequence.Create(other._types)) return false;
        if (ReportedMetrics != other.ReportedMetrics) return false;
        if (EquatableSequence.Create(_skippedFunctions) != EquatableSequence.Create(other._skippedFunctions))
            return false;
        return true;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ParsedAssembly);

    public override int GetHashCode()
    {
        Debug.Assert(_frozen);
        return HashCode.Combine(
            RealAssembly,
            EquatableSequence.Create(_types),
            ReportedMetrics,
            EquatableSequence.Create(_skippedFunctions));
    }
}
