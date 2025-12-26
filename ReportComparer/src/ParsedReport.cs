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
    private HashSet<string> _skippedModules = [];
    private HashSet<string> _sourceRoots = new(StringComparer.OrdinalIgnoreCase);
    private bool _frozen;

    public ReportComparison ReportComparison { get; }
    public string FullName { get; }
    public string Name { get => field is not null ? field : FullName; set; }
    public int Order { get; set; }
    public ReportType ReportType { get; }
    public IReadOnlyCollection<ParsedAssembly> Assemblies => _assemblies;
    public ReportedRootMetrics? ReportedMetrics { get; set; } // Cobertura only
    public IReadOnlyCollection<string> SourceRoots => _sourceRoots; // Cobertura only
    public IReadOnlyCollection<string> SkippedModules => _skippedModules; // DynamicCoverage only

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
        foreach (var assembly in _assemblies) assembly.Freeze();
        _assemblies = _assemblies.OrderBy(a => a.RealAssembly.Order).ToList();
        _skippedModules = _skippedModules.Order().ToHashSet();
        _sourceRoots = _sourceRoots.Order().ToHashSet(_sourceRoots.Comparer);
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

    public void AddSkippedModule(string name)
    {
        Debug.Assert(!_frozen);
        _skippedModules.Add(name);
    }

    internal sealed class EqualityComparer : IEqualityComparer<ParsedReport?>
    {
        public static EqualityComparer Instance { get; } = new EqualityComparer();

        private EqualityComparer() { }

        public bool Equals(ParsedReport? x, ParsedReport? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (y is null) return false;
            Debug.Assert(x!._frozen);
            Debug.Assert(y._frozen);
            if (x!.ReportType != y.ReportType) return false;
            if (EquatableSequence.Create(x._assemblies) != EquatableSequence.Create(y._assemblies)) return false;
            if (x.ReportedMetrics != y.ReportedMetrics) return false;
            if (EquatableSequence.Create(x._skippedModules) != EquatableSequence.Create(y._skippedModules))
                return false;
            if (EquatableSequence.Create(x._sourceRoots) != EquatableSequence.Create(y._sourceRoots))
                return false;
            return true;
        }

        public int GetHashCode([DisallowNull] ParsedReport? obj)
        {
            if (obj is null) return 0;
            Debug.Assert(obj._frozen);
            return HashCode.Combine(
                obj.ReportType,
                EquatableSequence.Create(obj._assemblies),
                obj.ReportedMetrics,
                EquatableSequence.Create(obj._skippedModules));
        }
    }
}
