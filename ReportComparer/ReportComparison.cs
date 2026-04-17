// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

internal sealed class ReportComparison
{
    private Dictionary<string, ParsedReport> _reports = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, RealAssembly> _realAssemblies = new(StringComparer.OrdinalIgnoreCase);
    private bool _frozen;

    public IReadOnlyCollection<ParsedReport> Reports => _reports.Values;
    public IReadOnlyCollection<RealAssembly> RealAssemblies => _realAssemblies.Values;
    public IReadOnlyCollection<IEnumerable<ParsedReport>> ContentGroups { get; private set; } = null!;
    public IReadOnlyCollection<IEnumerable<ParsedReport>> CoreContentGroups { get; private set; } = null!;
    public IReadOnlyCollection<IEnumerable<ParsedReport>> NameGroups { get; private set; } = null!;
    public IReadOnlyCollection<IEnumerable<ParsedReport>> CoverageGroups { get; private set; } = null!;
    public IReadOnlyCollection<IEnumerable<ParsedReport>> LineStatusGroups { get; private set; } = null!;
    public bool Frozen => _frozen;

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        int commonReportPathLength = PathHelper.GetCommonPathLength(_reports.Values.Select(r => r.FullName));
        int reportOrder = 1;
        foreach (var report in _reports.Values.OrderBy(r => r.Name, PathComparer.IgnoreCase))
        {
            report.Order = reportOrder++;
            report.Name = report.FullName[commonReportPathLength..];
        }
        _reports = _reports.OrderBy(kvp => kvp.Value.Order).ToDictionary(_reports.Comparer);

        int commonSourcePathLength = PathHelper.GetCommonPathLength(_realAssemblies.Values.SelectMany(a => a.RealSources).Select(s => s.FullName));
        int assemblyOrder = 1;
        int sourceOrder = 1;
        foreach (var assembly in _realAssemblies.Values.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
        {
            assembly.Order = assemblyOrder++;
            foreach (var source in assembly.RealSources.OrderBy(s => s.FullName, PathComparer.IgnoreCase))
            {
                source.Order = sourceOrder++;
                source.SetCommonPathLength(commonSourcePathLength);
            }
        }
        _realAssemblies = _realAssemblies.OrderBy(kvp => kvp.Value.Order).ToDictionary(_realAssemblies.Comparer);

        foreach (var report in _reports.Values) report.Freeze();
        foreach (var assembly in _realAssemblies.Values) assembly.Freeze();
        ContentGroups = _reports.Values.GroupBy(r => r, ParsedReport.FullEqualityComparer.Instance).ToArray();
        CoreContentGroups = _reports.Values.GroupBy(r => r, ParsedReport.CoreEqualityComparer.Instance).ToArray();
        NameGroups = _reports.Values.GroupBy(r => r.Assemblies.SelectMany(a => a.Types)
                .SelectMany(t => t.Methods)
                .Select(m => (m.RealMethod, m.ParentType.Name, m.Name))
                .ToEquatableSet())
            .ToArray();
        CoverageGroups = _reports.Values.GroupBy(r => r.Assemblies.SelectMany(a => a.Sources)
                .Select(s => (s.RealSource, s.LinesCoverage.ToEquatableSequence()))
                .ToEquatableSet())
            .ToArray();
        LineStatusGroups = _reports.Values.GroupBy(r => r.Assemblies.SelectMany(a => a.Sources)
                .Select(s => (
                    s.RealSource,
                    s.LinesCoverage.Select(line => ParsedRange.AggregateStatus(line.Values)).ToEquatableSequence()))
                .ToEquatableSet())
            .ToArray();
        _frozen = true;
    }

    public void ParseReport(string reportPath)
    {
        Debug.Assert(!_frozen);
        reportPath = Path.GetFullPath(reportPath);
        var report = ReportParser.ParseReport(reportPath, this);
        _reports.Add(reportPath, report);
    }

    public RealAssembly GetOrAddRealAssembly(string name)
    {
        Debug.Assert(!_frozen);
        if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) name = name[..^4];
        if (_realAssemblies.TryGetValue(name, out var assembly)) return assembly;
        assembly = new RealAssembly(this, name);
        _realAssemblies.Add(name, assembly);
        return assembly;
    }
}
