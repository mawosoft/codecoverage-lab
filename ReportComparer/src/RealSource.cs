// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReportComparer;

[DebuggerDisplay("{Name,nq}")]
internal sealed partial class RealSource
{
    private Dictionary<ParsedReport, ParsedSource?> _parsedSources = new(ReferenceEqualityComparer.Instance);
    private Line[] _linesCoverage = [];
    private bool _frozen;

    public RealAssembly ParentAssembly { get; }
    public string FullName { get; }
    public string Name { get => field is not null ? field : FullName; set; }
    public int Order { get; set; }
    public IReadOnlyDictionary<ParsedReport, ParsedSource?> ParsedSources => _parsedSources;

    public IReadOnlyList<Line> LinesCoverage => _linesCoverage;

    public IEnumerable<IGrouping<string, IGrouping<RealType, RealMethod>>> RealTree
        => ParentAssembly.RealTypes.SelectMany(rt => rt.RealMethods)
            .Where(rm => rm.BoundingRange.source == this)
            .GroupBy(rm => rm.ParentType)
            .GroupBy(g => g.Key.Namespace);

    public RealSource(RealAssembly realAssembly, string fullName)
    {
        ParentAssembly = realAssembly;
        FullName = fullName;
    }

    public void Freeze(IEnumerable<ParsedReport> allReports)
    {
        Debug.Assert(!_frozen);
        foreach (var report in allReports) _parsedSources.TryAdd(report, null);
        Debug.Assert(_parsedSources.All(kvp => kvp.Key.Order != 0));
        _parsedSources = _parsedSources.OrderBy(kvp => kvp.Key.Order).ToDictionary(_parsedSources.Comparer);
        int lineCount = _parsedSources.Max(kvp => kvp.Value?.LinesCoverage.Count ?? 0);
        _linesCoverage = new Line[lineCount];
        for (int lineNumber = 0; lineNumber < _linesCoverage.Length; lineNumber++)
        {
            _linesCoverage[lineNumber] = new Line(lineNumber, _parsedSources.Select(kvp => (
                    parsedRanges: kvp.Value?.LinesCoverage?.Count > lineNumber
                        ? kvp.Value.LinesCoverage[lineNumber]
                        : default,
                    report: kvp.Key))
                .GroupBy(
                    vt => vt.parsedRanges,
                    (parsedRanges, g) => (parsedRanges.Values, g.Select(vt => vt.report))));
        }
        _frozen = true;
    }

    public void AddParsedSource(ParsedSource parsedSource)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedSource.RealSource == this);
        _parsedSources.Add(parsedSource.ParentReport, parsedSource);
    }
}
