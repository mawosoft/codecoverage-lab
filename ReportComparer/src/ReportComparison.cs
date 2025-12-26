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
    private Dictionary<string, RealAssembly> _realAssemblies = new(StringComparer.OrdinalIgnoreCase);
    private List<ParsedReport> _reports = [];
    private bool _frozen;

    public IReadOnlyCollection<ParsedReport> Reports => _reports;
    public IReadOnlyCollection<RealAssembly> RealAssemblies => _realAssemblies.Values;

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        int commonReportPathLength = PathHelper.GetCommonPathLength(_reports.Select(r => r.FullName));
        foreach (var report in _reports) report.Name = report.FullName[commonReportPathLength..];
        _reports = _reports.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
        for (int reportOrder = 0; reportOrder < _reports.Count; reportOrder++)
        {
            _reports[reportOrder].Order = reportOrder + 1;
        }

        _realAssemblies = _realAssemblies.OrderBy(kvp => kvp.Value.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(_realAssemblies.Comparer);
        int commonSourcePathLength = PathHelper.GetCommonPathLength(_realAssemblies.Values.SelectMany(a => a.RealSources).Select(s => s.FullName));
        foreach (var source in _realAssemblies.Values.SelectMany(a => a.RealSources))
        {
            source.Name = source.FullName[commonSourcePathLength..];
        }
        int assemblyOrder = 1;
        int sourceOrder = 1;
        foreach (var assembly in _realAssemblies.Values)
        {
            assembly.Order = assemblyOrder++;
            assembly.FreezeParsing(_reports);
            foreach (var source in assembly.RealSources) source.Order = sourceOrder++;
        }

        foreach (var report in _reports) report.Freeze();
        MapParsedToRealMethods();
        foreach (var realAssembly in _realAssemblies.Values) realAssembly.Freeze();
        _frozen = true;
    }

    public void ParseReport(string reportPath)
    {
        Debug.Assert(!_frozen);
        reportPath = Path.GetFullPath(reportPath);
        var report = ReportParser.ParseReport(reportPath, this);
        _reports.Add(report);
    }

    public RealAssembly GetOrAddRealAssembly(string name)
    {
        Debug.Assert(!_frozen);
        if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) name = name[..^4];
        if (_realAssemblies.TryGetValue(name, out var assembly)) return assembly;
        assembly = new RealAssembly(name);
        _realAssemblies.Add(name, assembly);
        return assembly;
    }

    private void MapParsedToRealMethods()
    {
        foreach (var realSourceGroup in _reports.SelectMany(r => r.Assemblies)
            .SelectMany(a => a.Types)
            .SelectMany(t => t.Methods)
            .Select(m => m.BoundingRange)
            .OrderBy(br => br.RealSource.Order).ThenBy(br => br.Range).ThenBy(br => br.ParentMethod.Name)
            .GroupBy(br => br.RealSource))
        {
            ParsedRange[][] boundingRangesPerReport = realSourceGroup
                .GroupBy(boundingRange => boundingRange.ParentReport,
                         (report, boundingRanges) => boundingRanges.ToArray())
                .ToArray();
            for (int outerReportIndex = 0; outerReportIndex < boundingRangesPerReport.Length; outerReportIndex++)
            {
                ParsedRange[] outerBoundingRanges = boundingRangesPerReport[outerReportIndex];
                for (int outerRangeIndex = 0; outerRangeIndex < outerBoundingRanges.Length; outerRangeIndex++)
                {
                    ParsedRange outerRange = outerBoundingRanges[outerRangeIndex];
                    if (outerRange.ParentMethod.RealMethod is not null) continue;
                    RealMethod realMethod = outerRange.ParentAssembly.RealAssembly.CreateRealMethod(outerRange.ParentMethod);
                    for (int innerReportIndex = outerReportIndex + 1;
                        innerReportIndex < boundingRangesPerReport.Length;
                        innerReportIndex++)
                    {
                        ParsedRange[] innerBoundingRanges = boundingRangesPerReport[innerReportIndex];
                        ParsedRange? candidate = null;
                        long candidateMatchSize = 0;
                        int candidateTypeDistance = 0;
                        int candidateMethodDistance = 0;
                        for (int innerRangeIndex = 0; innerRangeIndex < innerBoundingRanges.Length; innerRangeIndex++)
                        {
                            ParsedRange innerRange = innerBoundingRanges[innerRangeIndex];
                            if (innerRange.ParentMethod.RealMethod is not null) continue;
                            Range intersection = outerRange.Range.Intersect(innerRange.Range);
                            if (intersection.IsValid())
                            {
                                long matchSize = intersection.VirtualSize;
                                int typeDistance = LevenshteinDistance(outerRange.ParentType.Name, innerRange.ParentType.Name);
                                int methodDistance = LevenshteinDistance(outerRange.ParentMethod.Name, innerRange.ParentMethod.Name);
                                if (candidate is null
                                    || matchSize > candidateMatchSize
                                    || (matchSize == candidateMatchSize
                                        && (typeDistance < candidateTypeDistance
                                            || (typeDistance == candidateTypeDistance && methodDistance < candidateMethodDistance))))
                                {
                                    candidate = innerRange;
                                    candidateMatchSize = matchSize;
                                    candidateTypeDistance = typeDistance;
                                    candidateMethodDistance = methodDistance;
                                }
                            }
                            else if (innerRange.Range.StartLine > outerRange.Range.EndLine)
                            {
                                break;
                            }
                        }
                        if (candidate is not null)
                        {
                            realMethod.AddParsedMethod(candidate.ParentMethod);
                        }
                    }
                }
            }
        }
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
        int[,] m = new int[s1.Length + 1, s2.Length + 1];
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional
        for (int i = 0; i <= s1.Length; i++) m[i, 0] = i;
        for (int i = 0; i <= s2.Length; i++) m[0, i] = i;
        for (int i = 1; i <= s1.Length; i++)
        {
            for (int k = 1; k <= s2.Length; k++)
            {
                int diff = (s1[i - 1] == s2[k - 1]) ? 0 : 1;
                m[i, k] = Math.Min(
                            Math.Min(
                                m[i - 1, k] + 1,
                                m[i, k - 1] + 1),
                            m[i - 1, k - 1] + diff);
            }
        }
        return m[s1.Length, s2.Length];
    }
}
