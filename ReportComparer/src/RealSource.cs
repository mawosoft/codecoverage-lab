// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{Name,nq}")]
internal sealed partial class RealSource
{
    private List<ParsedSource> _parsedSources = [];
    private Line[] _linesCoverage = [];
    private List<RealMethod> _methods = [];
    private bool _frozen;

    public RealAssembly ParentAssembly { get; }
    public string FullName { get; }
    public string Name { get; private set; }
    public string DirectoryName { get; private set; }
    public string FileName { get; private set; }
    public int Order { get; set; }
    public IReadOnlyCollection<ParsedSource> ParsedSources => _parsedSources;

    public IReadOnlyList<Line> LinesCoverage => _linesCoverage;
    public IReadOnlyCollection<RealMethod> Methods => _methods;
    public bool HasRangeOverlaps { get; private set; }
    public bool TypesAreAmbiguous { get; private set; }
    public int NameGroupCount { get; private set; }
    public int CoverageGroupCount { get; private set; }
    public int StatusGroupCount { get; private set; }

    public IReadOnlyCollection<ParsedReport> Reports { get; private set; } = null!;

    public RealSource(RealAssembly realAssembly, string fullName)
    {
        ParentAssembly = realAssembly;
        FullName = fullName;
        SetCommonPathLength(0);
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_parsedSources.All(s => s.ParentReport.Order != 0));
        _parsedSources = _parsedSources.OrderBy(s => s.ParentReport.Order).ToList();
        MapParsedToRealLines();
        MapParsedToRealMethods();
        foreach (var method in _methods) method.Freeze();
        _methods = _methods.OrderBy(m => m.BoundingRange).ToList();
        HasRangeOverlaps = _linesCoverage.Any(line => line.HasOverlaps);
        TypesAreAmbiguous = _methods.Any(m => m.TypesAreAmbiguous);
        NameGroupCount = _parsedSources.DistinctBy(
            s => s.Methods.Select(m => (m.RealMethod, m.ParentType.Name, m.Name)).ToEquatableSequence())
            .Count();
        CoverageGroupCount = _parsedSources.DistinctBy(s => EquatableSequence.Create(s.LinesCoverage)).Count();
        StatusGroupCount = _parsedSources.DistinctBy(
            s => s.LinesCoverage.Select(line => ParsedRange.AggregateStatus(line.Values)).ToEquatableSequence())
            .Count();
        Reports = _parsedSources.Select(s => s.ParentReport).ToArray();
        Debug.Assert(Reports.Count == Reports.Distinct().Count());
        _frozen = true;
    }

    public void AddParsedSource(ParsedSource parsedSource)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedSource.RealSource == this);
        _parsedSources.Add(parsedSource);
    }

    [MemberNotNull(nameof(Name), nameof(DirectoryName), nameof(FileName))]
    public void SetCommonPathLength(int value)
    {
        Name = FullName[value..];
        int pos = Name.AsSpan().LastIndexOfAny(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        pos = pos < 0 ? 0 : pos + 1;
        DirectoryName = Name[..pos];
        FileName = Name[pos..];
    }

    private void MapParsedToRealLines()
    {
        int lineCount = _parsedSources.Max(s => s.LinesCoverage.Count);
        _linesCoverage = new Line[lineCount];
        for (int lineNumber = 0; lineNumber < _linesCoverage.Length; lineNumber++)
        {
            _linesCoverage[lineNumber] = new Line(
                lineNumber,
                _parsedSources.Select(s => (
                        parsedRanges: s.LinesCoverage.Count > lineNumber
                            ? s.LinesCoverage[lineNumber]
                            : default,
                        report: s.ParentReport))
                    .GroupBy(
                        vt => vt.parsedRanges,
                        (parsedRanges, g) => (parsedRanges.Values.ToArray(), g.Select(vt => vt.report).ToArray())),
                _parsedSources.Count);
        }
    }

    private void MapParsedToRealMethods()
    {
        var parsedMethods = _parsedSources.Select(s => s.Methods.ToArray()).ToArray();
        for (int outerReportIndex = 0; outerReportIndex < parsedMethods.Length; outerReportIndex++)
        {
            var outerMethods = parsedMethods[outerReportIndex];
            for (int outerRangeIndex = 0; outerRangeIndex < outerMethods.Length; outerRangeIndex++)
            {
                ParsedMethod outerMethod = outerMethods[outerRangeIndex];
                if (outerMethod.RealMethod is not null) continue;
                RealMethod realMethod = new(outerMethod);
                for (int innerReportIndex = outerReportIndex + 1; innerReportIndex < parsedMethods.Length; innerReportIndex++)
                {
                    var innerMethods = parsedMethods[innerReportIndex];
                    ParsedMethod? candidate = null;
                    ParsedMethod.Match candidateMatch = default;
                    for (int innerRangeIndex = 0; innerRangeIndex < innerMethods.Length; innerRangeIndex++)
                    {
                        var innerMethod = innerMethods[innerRangeIndex];
                        if (innerMethod.RealMethod is not null) continue;
                        var match = outerMethod.MatchTo(innerMethod);
                        if (match.MatchSize != 0)
                        {
                            if (candidate is null || match > candidateMatch)
                            {
                                candidate = innerMethod;
                                candidateMatch = match;
                            }
                        }
                        else if (innerMethod.BoundingRange.StartLine > outerMethod.BoundingRange.EndLine)
                        {
                            break;
                        }
                    }
                    if (candidate is not null)
                    {
                        realMethod.AddParsedMethod(candidate);
                    }
                }
                _methods.Add(realMethod);
            }
        }
    }
}
