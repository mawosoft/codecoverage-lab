// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{Name,nq}")]
internal sealed class ParsedReport
{
    private List<ParsedAssembly> _assemblies = [];
    private Dictionary<RealAssembly, HashSet<string>> _skippedFunctions = new(ReferenceEqualityComparer.Instance); // DynamicCoverage only.
    private List<(RealAssembly, string)> _skippedFunctionsEquatable = null!;
    private HashSet<string> _skippedModules = [];
    private HashSet<string> _sourceRoots = new(StringComparer.OrdinalIgnoreCase);
    private bool _frozen;

    public ReportComparison ReportComparison { get; }
    public string FullName { get; }
    public string Name { get => field is not null ? field : FullName; set; }
    public int Order { get; set; }
    public ReportType ReportType { get; }
    public IReadOnlyCollection<ParsedAssembly> Assemblies => _assemblies;
    public ReportedMetrics? ReportedMetrics { get; set; } // Cobertura only
    public IReadOnlyCollection<string> SourceRoots => _sourceRoots; // Cobertura only
    public IReadOnlyCollection<string> SkippedModules => _skippedModules; // DynamicCoverage only

    public IReadOnlyCollection<string> SkippedFunctionsForAssembly(RealAssembly realAssembly)
        => _skippedFunctions.TryGetValue(realAssembly, out var result) ? result : [];

    public ParsedReport(ReportComparison reportComparison, string fullName, ReportType reportType)
    {
        ReportComparison = reportComparison;
        FullName = fullName;
        ReportType = reportType;
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_assemblies.All(a => a.RealAssembly.Order != 0));
        _assemblies = _assemblies.OrderBy(a => a.RealAssembly.Order).ToList();
        _skippedFunctions = _skippedFunctions.OrderBy(kvp => kvp.Key.Order)
            .Select(kvp => (kvp.Key, kvp.Value.Order().ToHashSet()))
            .ToDictionary(_skippedFunctions.Comparer);
        _skippedFunctionsEquatable = _skippedFunctions.SelectMany(kvp => kvp.Value.Select(s => (kvp.Key, s))).ToList();
        _skippedModules = _skippedModules.Order().ToHashSet();
        _sourceRoots = _sourceRoots.Order(StringComparer.OrdinalIgnoreCase).ToHashSet(_sourceRoots.Comparer);
        foreach (var assembly in _assemblies) assembly.Freeze();
        _frozen = true;
    }

    public ParsedAssembly AddAssembly(string name)
    {
        Debug.Assert(!_frozen);
        var realAssembly = ReportComparison.GetOrAddRealAssembly(name);
        var parsedAssembly = new ParsedAssembly(this, realAssembly, name);
        realAssembly.AddParsedAssembly(parsedAssembly);
        _assemblies.Add(parsedAssembly);
        return parsedAssembly;
    }

    public void AddSourceRoots(IEnumerable<string> roots)
    {
        Debug.Assert(!_frozen);
        _sourceRoots.UnionWith(roots);
    }

    public void AddSkippedFunctions(ParsedAssembly assembly, IEnumerable<string> names)
    {
        Debug.Assert(!_frozen);
        if (!_skippedFunctions.TryGetValue(assembly.RealAssembly, out var result))
        {
            result = [];
            _skippedFunctions.Add(assembly.RealAssembly, result);
        }
        result.UnionWith(names);
    }

    public void AddSkippedModules(IEnumerable<string> names)
    {
        Debug.Assert(!_frozen);
        _skippedModules.UnionWith(names);
    }

    internal sealed class CoreEqualityComparer : IEqualityComparer<ParsedReport?>
    {
        public static CoreEqualityComparer Instance { get; } = new CoreEqualityComparer();

        private CoreEqualityComparer() { }

        public bool Equals(ParsedReport? x, ParsedReport? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            Debug.Assert(x._frozen);
            Debug.Assert(y._frozen);
            if (x.ReportType != y.ReportType) return false;
            if (x.ReportedMetrics != y.ReportedMetrics) return false;
            return x._assemblies.SequenceEqual(y._assemblies);
        }

        public int GetHashCode([DisallowNull] ParsedReport? obj)
        {
            if (obj is null) return 0;
            Debug.Assert(obj._frozen);
            return HashCode.Combine(
                obj.ReportType,
                obj.ReportedMetrics,
                EquatableSequence.Create(obj._assemblies));
        }
    }

    internal sealed class FullEqualityComparer : IEqualityComparer<ParsedReport?>
    {
        public static FullEqualityComparer Instance { get; } = new FullEqualityComparer();

        private FullEqualityComparer() { }

        public bool Equals(ParsedReport? x, ParsedReport? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            if (!CoreEqualityComparer.Instance.Equals(x, y)) return false;
            if (!x._sourceRoots.SetEquals(y._sourceRoots)) return false;
            if (!x._skippedFunctionsEquatable.SequenceEqual(y._skippedFunctionsEquatable)) return false;
            return x._skippedModules.SequenceEqual(y._skippedModules);
        }

        public int GetHashCode([DisallowNull] ParsedReport? obj)
        {
            if (obj is null) return 0;
            return HashCode.Combine(
                CoreEqualityComparer.Instance.GetHashCode(obj),
                EquatableSequence.Create(obj._skippedFunctionsEquatable),
                EquatableSequence.Create(obj._skippedModules),
                EquatableSet.Create(obj._sourceRoots));
        }
    }
}
