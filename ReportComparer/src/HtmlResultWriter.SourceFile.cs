// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

internal sealed partial class HtmlResultWriter
{
    private void WriteSourceFile()
    {
        WriteHtmlStart("Report Comparison - " + _realSource.Name);
        _writer.WriteLine($"""
            <h1>{_homeNavig}<a href="index.html">Report Comparison</a> &nbsp;&gt;&nbsp; {Encode(_realSource.Name)}</h1>
            """);
        _writer.WriteLine($"""
            <table class="datagrid"><tbody>
            <tr><td class="right">Source</td><td>{Encode(_realSource.Name)}</td></tr>
            <tr><td class="right">Assembly</td><td>{Encode(_realSource.ParentAssembly.Name)}</td></tr>
            """);
        if (_realSource.Reports.Count != _reportComparison.Reports.Count)
        {
            _writer.WriteLine($"""
                <tr><td class="right">Available reports</td><td>{ReportGroup(_realSource.Reports)}</td></tr>
                <tr><td class="right">Missing reports</td><td>{ReportGroup(_reportComparison.Reports.Except(_realSource.Reports))}</td></tr>
                """);
        }
        _writer.WriteLine("</tbody></table>");

        _writer.WriteLine("<details><summary><h2>Report Groups</h2></summary>");
        _writer.WriteLine("<h3>By Names</h3>");
        WriteReportGroupTable(_realSource.NameGroups);
        _writer.WriteLine("<h3>By Coverage Details</h3>");
        WriteReportGroupTable(_realSource.CoverageGroups);
        _writer.WriteLine("<h3>By Line Status</h3>");
        WriteReportGroupTable(_realSource.LineStatusGroups);
        _writer.WriteLine("</details>");

        _writer.WriteLine("""
            <details><summary><h2>Type and Method Names <button class="help" data-for="help1">i</button></h2></summary>
            <p id="help1" class="help hidden">
            The plus sign (+) is used as unified separator for nested type names if the associated reports
            provide a way to distinguish between type and namespace nesting.
            </p>
            """);
        WriteParsedNames();
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h2>Reported Type Metrics</h2></summary>");
        WriteTypeMetrics();
        _writer.WriteLine("</details>");

        _writer.WriteLine("""
            <details><summary><h2>Reported Method Metrics</h2>
            <input type="checkbox" id="chkInlineMetrics" />
            <label for="chkInlineMetrics">Show inline</label>
            </summary>
            """);
        WriteMethodMetrics();
        _writer.WriteLine("</details>");

        _writer.WriteLine("""
            <h2>Coverage <button class="help" data-for="help2">i</button></h2>
            <div id="help2" class="hidden"><table class="help"><tbody>
            <tr>
              <td><button class="plus" inert>+</button> <button class="plus" inert>M</button></td>
              <td>Click to display the line coverage details. If you have chosen to show reported method metrics
                  inline, the <button class="plus" inert>M</button> button marks lines where those metrics
                  are available.</td>
            </tr><tr>
              <td><table class="lineMarginGlyphs"><tbody>
                  <tr><td class="blank">&nbsp;</td><td class="green">&nbsp;</td><td class="red">&nbsp;</td><td class="yellow">&nbsp;</td></tr>
                  </tbody></table></td>
              <td>Aggregated line status per report group: <em>not coverable, covered, not covered, partial</em>.</td>
            </tr><tr>
              <td class="redtext">37</td>
              <td>Lines containing overlapping ranges.</td>
            </tr><tr>
              <td>65,17 - 65,42</td>
              <td>Starting line and column (inclusive) to ending line and column (exclusive) of a reported range.
                  Hover the mouse over it to highlight the range in the source code line above.
                  For line-based reports, column numbers are always 0.</td>
            </tr>
            </tbody></table></div>
            """);
        WriteCoverage();
        WriteHtmlEnd();
    }

    private void WriteParsedNames()
    {
        _writer.WriteLine("""
            <table class="datagrid zebra">
            <thead><tr><th>Line</th><th>Type</th><th>Method</th><th>Reports</th></tr></thead>
            """);
        string? lastNamespace = " ";
        bool canUseTypeName = false;
        foreach (var realMethod in _realSource.Methods)
        {
            var parsedMethodGroups = realMethod.ParsedMethods.GroupBy(
                m => (m.ParentType.Name, m.Name),
                (_, g) => (parsedMethod: g.First(), reports: g.Select(m => m.ParentReport))).ToArray();
            string? @namespace = parsedMethodGroups.Select(vt => vt.parsedMethod.RealType.Namespace)
                .Distinct()
                .FirstIfSingleOrDefault();
            if (@namespace != lastNamespace)
            {
                lastNamespace = @namespace;
                canUseTypeName = !string.IsNullOrEmpty(@namespace);
                _writer.WriteLine($"""<tbody><tr><td colspan="4">{EncodeNamespace(@namespace)}</td></tr></tbody>""");
            }
            int rowspan = parsedMethodGroups.Length;
            if (_realSource.Reports.Count != realMethod.Reports.Count) rowspan++;
            _writer.WriteLine($"""<tbody><tr><td{RowSpan(rowspan)} class="right">{realMethod.BoundingRange.StartLine}</td>""");
            bool tr = false;
            foreach ((ParsedMethod parsedMethod, IEnumerable<ParsedReport> reports) in parsedMethodGroups)
            {
                if (tr) _writer.Write("<tr>");
                tr = true;
                _writer.WriteLine($"""
                    <td>{Encode(canUseTypeName ? parsedMethod.ParentType.TypeName : parsedMethod.ParentType.Name)}</td>
                    <td>{Encode(parsedMethod.Name)}</td>
                    <td>{ReportGroup(reports)}</td>
                    </tr>
                    """);
            }
            if (_realSource.Reports.Count != realMethod.Reports.Count)
            {
                if (tr) _writer.Write("<tr>");
                _writer.WriteLine($"""
                    <td colspan="2">(missing)</td>
                    <td>{ReportGroup(_realSource.Reports.Except(realMethod.Reports))}</td>
                    </tr>
                    """);
            }
            _writer.WriteLine("</tbody>");
        }
        _writer.WriteLine("</table>");
    }

    private void WriteTypeMetrics()
    {
        var namespaceGroups = _realSource.Methods.SelectMany(rm => rm.ParsedTypes)
            .DistinctBy(pt => pt, ReferenceEqualityComparer.Instance)
            .GroupBy(
                pt => pt.RealType,
                (realType, g) => (
                    realType,
                    typeNames: g.Select(pt => pt.TypeName).Distinct(),
                    typeMetrics: g.GroupBy(
                            pt => pt.ReportedMetricsForSource(_realSource),
                            (metrics, g) => (metrics, reports: g.Select(pt => pt.ParentReport)))
                        .ToArray()))
            .GroupBy(vt => vt.realType.Namespace);
        (int columnMask, int fractionMask) = GetMetricsMask(
            namespaceGroups.SelectMany(g => g.SelectMany(vt => vt.typeMetrics.Select(vt => vt.metrics))));
        int columnCount = int.PopCount(columnMask) + 2;
        _writer.WriteLine("""<table class="datagrid zebra">""");
        WriteMetricsHeaders(columnMask, "Type");
        foreach (var namespaceGroup in namespaceGroups)
        {
            _writer.WriteLine($"""<tbody><tr><td colspan="{columnCount}">{EncodeNamespace(namespaceGroup.Key)}</td></tr></tbody>""");
            foreach (var (realType, typeNames, typeMetrics) in namespaceGroup)
            {
                _writer.WriteLine($"""
                    <tbody class="right"><tr>
                    <td class= "left"{RowSpan(typeMetrics.Length)}>{Expandable(typeNames)}</td>
                    """);
                WriteMetricsData(columnMask, fractionMask, startRow: false, typeMetrics);
                _writer.WriteLine("</tbody>");
            }
        }
        _writer.WriteLine("</table>");
    }

    private void WriteMethodMetrics()
    {
        (int columnMask, int fractionMask) = GetMetricsMask(_realSource.Methods.SelectMany(rm => rm.ParsedMethods).Select(pm => pm.ReportedMetrics));
        int columnCount = int.PopCount(columnMask) + 4;
        _writer.WriteLine("""<table class="datagrid zebra">""");
        WriteMetricsHeaders(columnMask, "Line", "Type", "Method");
        string? lastNamespace = " ";
        bool canUseTypeName = false;
        foreach (var realMethod in _realSource.Methods)
        {
            string? @namespace = realMethod.ParsedMethods.Select(pm => pm.RealType)
                .Distinct()
                .Select(rt => rt.Namespace)
                .Distinct()
                .FirstIfSingleOrDefault();
            if (@namespace != lastNamespace)
            {
                lastNamespace = @namespace;
                canUseTypeName = !string.IsNullOrEmpty(@namespace);
                _writer.WriteLine($"""<tbody><tr><td colspan="{columnCount}">{EncodeNamespace(@namespace)}</td></tr></tbody>""");
            }
            var methodMetrics = realMethod.ParsedMethods.GroupBy(
                pm => pm.ReportedMetrics,
                (metrics, g) => (metrics, reports: g.Select(pm => pm.ParentReport)))
                .ToArray();
            string rowSpan = RowSpan(methodMetrics.Length);
            // TODO sort type names and method names?
            var typeNames = canUseTypeName
                ? realMethod.ParsedMethods.Select(pm => pm.ParentType.TypeName).Distinct()
                : realMethod.ParsedMethods.Select(pm => pm.ParentType.Name).Distinct();
            _writer.WriteLine($"""
                <tbody class="right"><tr>
                <td{rowSpan}>{realMethod.BoundingRange.StartLine}</td>
                <td class="left"{rowSpan}>{Expandable(typeNames)}</td>
                <td class="left"{rowSpan}>{Expandable(realMethod.ParsedMethods.Select(pm => pm.Name).Distinct())}</td>
                """);
            WriteMetricsData(columnMask, fractionMask, startRow: false, methodMetrics);
            _writer.WriteLine("</tbody>");
        }
        _writer.WriteLine("</table>");
    }

    private void WriteCoverage()
    {
        _writer.WriteLine("""<table class="sourceLines"><tbody>""");
        var methodsPerLine = _realSource.Methods.ToLookup(rm => rm.BoundingRange.StartLine);
        int lineNumber = 1;
        string? lineText;
        while ((lineText = _reader.ReadLine()) is not null || lineNumber < _realSource.LinesCoverage.Count)
        {
            lineText ??= "";
            RealSource.Line line = lineNumber < _realSource.LinesCoverage.Count
                ? _realSource.LinesCoverage[lineNumber]
                : default;
            if (line.IsCoverable)
            {
                var methods = methodsPerLine[line.Number];
                bool metrics = methods.Any();
                string dataMetrics = metrics ? " data-metrics=\"true\"" : "";
                string detailId = NextUniqueId();
                _writer.Write($"""
                    <tr><td><button class="plus" data-for="{detailId}"{dataMetrics}">+</button></td>
                    <td>
                    """);
                WriteLineMarginGlyphs(line);
                string redText = line.HasOverlaps ? " class=\"redtext\"" : "";
                string? lineId = null;
                string preId = "";
                if (line.HasColumns)
                {
                    lineId = NextUniqueId();
                    preId = $" id=\"{lineId}\"";
                }
                _writer.WriteLine($"""
                    </td>
                    <td{redText}>{lineNumber}</td>
                    <td><pre{preId}>{Encode(lineText)}</pre></td></tr>
                    <tr id="{detailId}" class="hidden"><td colspan="3"></td>
                    <td>
                    """);
                WriteLineCoverageDetails(line, lineId, lineText.Length);
                if (metrics) WriteInlineMethodMetrics(methods);
                _writer.WriteLine("</td></tr>");
            }
            else
            {
                Debug.Assert(!methodsPerLine[line.Number].Any());
                _writer.WriteLine($"""
                    <tr>
                    <td></td><td></td>
                    <td>{lineNumber}</td><td><pre>{Encode(lineText)}</pre></td>
                    </tr>
                    """);
            }
            lineNumber++;
        }
        _writer.WriteLine("</tbody></table>");
    }

    private void WriteLineMarginGlyphs(RealSource.Line line)
    {
        if (!line.IsCoverable) return;
        var statuses = line.RangeGroups.Select(rg => ParsedRange.AggregateStatus(rg.parsedRanges));
        _writer.Write("""<table class="lineMarginGlyphs"><tbody><tr>""");
        foreach (var status in statuses)
        {
            _writer.Write($"""<td class="{StatusToClass(status)}">&nbsp;</td>""");
        }
        _writer.WriteLine("</tr></tbody></table>");
    }

    private void WriteLineCoverageDetails(RealSource.Line line, string? lineId, int lineLength)
    {
        if (!line.IsCoverable) return;
        _writer.WriteLine("""
            <table class="datagrid zebra">
            <colgroup><col class="detailGlyph" /></colgroup>
            <thead><tr>
            <th colspan="2">Status</th>
            <th>Range</th>
            <th>Hits</th>
            <th>Branches</th>
            <th>Conditions</th>
            <th>Reports</th>
            </tr></thead>
            """);
        foreach (var (parsedRanges, reports) in line.RangeGroups)
        {
            if (parsedRanges.Length == 0)
            {
                _writer.WriteLine($"""
                    <tbody><tr>
                    <td class="blank detailGlyph"></td>
                    <td class="center">{EncodeStatus(CoverageStatus.None)}</td>
                    <td colspan="4"></td>
                    <td>{ReportGroup(reports)}</td>
                    </tr></tbody>
                    """);
            }
            else
            {
                string rowSpan = RowSpan(parsedRanges.Length);
                string statusClass = StatusToClass(ParsedRange.AggregateStatus(parsedRanges));
                _writer.WriteLine($"""<tbody><tr><td{rowSpan} class="{statusClass} detailGlyph"></td>""");
                bool first = true;
                foreach (var parsedRange in parsedRanges)
                {
                    if (!first) _writer.Write("<tr>");
                    var r = parsedRange.Range;
                    string dataForRange = "";
                    if (r.StartColumn != 0 && !string.IsNullOrEmpty(lineId))
                    {
                        int start = 0;
                        int end = lineLength;
                        if (line.Number == r.StartLine) start = r.StartColumn - 1;
                        if (line.Number == r.EndLine) end = Math.Min(lineLength, r.EndColumn - 1);
                        dataForRange = $""" data-for="{lineId}" data-range="{start} {end}" """;
                    }
                    _writer.Write($"""
                        <td class="center">{EncodeStatus(parsedRange.Status)}</td>
                        <td{dataForRange}>{r.StartLine},{r.StartColumn} - {r.EndLine},{r.EndColumn}</td>
                        """);
                    if (parsedRange.Cobertura is null)
                    {
                        _writer.WriteLine("""<td colspan="3"></td>""");
                    }
                    else
                    {
                        var c = parsedRange.Cobertura;
                        _writer.Write($"<td class=right>{c.Hits}</td>");
                        if (c.TotalBranches == 0)
                        {
                            _writer.Write("<td></td>");
                        }
                        else
                        {
                            _writer.Write($"""<td class="center">{c.CoveredBranches}/{c.TotalBranches}</td>""");
                        }
                        _writer.Write("<td>");
                        foreach (double condition in c.Conditions.Values)
                        {
                            _writer.Write($" {condition}%");
                        }
                        _writer.Write("</td>");
                    }
                    if (first)
                    {
                        first = false;
                        _writer.Write($"<td{rowSpan}>{ReportGroup(reports)}</td>");
                    }
                    _writer.WriteLine("</tr>");
                }
                _writer.WriteLine("</tbody>");
            }
        }
        _writer.WriteLine("</table>");
    }

    private void WriteInlineMethodMetrics(IEnumerable<RealMethod> methods)
    {
        var realMethods = methods as ICollection<RealMethod> ?? methods.ToArray();
        if (realMethods.Count == 0) return;
        (int columnMask, int fractionMask) = GetMetricsMask(realMethods.SelectMany(rm => rm.ParsedMethods).Select(pm => pm.ReportedMetrics));
        bool includeTypeNames = realMethods.Any(rm => rm.TypesAreAmbiguous);
        _writer.WriteLine("""<table class="datagrid zebra hidden" data-metrics="true">""");
        WriteMetricsHeaders(columnMask, includeTypeNames ? ["Type", "Method"] : ["Method"]);
        foreach (var realMethod in realMethods)
        {
            var methodMetrics = realMethod.ParsedMethods.GroupBy(
                pm => pm.ReportedMetrics,
                (metrics, g) => (metrics, reports: g.Select(pm => pm.ParentReport)))
                .ToArray();
            string rowSpan = RowSpan(methodMetrics.Length);
            _writer.WriteLine("""<tbody class="right"><tr>""");
            if (includeTypeNames)
            {
                _writer.WriteLine($"""<td class="left"{rowSpan}>{Expandable(realMethod.ParsedMethods.Select(pm => pm.ParentType.TypeName).Distinct())}</td>""");
            }
            _writer.WriteLine($"""<td class="left"{rowSpan}>{Expandable(realMethod.ParsedMethods.Select(pm => pm.Name).Distinct())}</td>""");
            WriteMetricsData(columnMask, fractionMask, startRow: false, methodMetrics);
            _writer.WriteLine("</tbody>");
        }
        _writer.WriteLine("</table>");
    }

    private static string EncodeNamespace(string? value)
    {
        return value switch
        {
            null => "Mixed namespace",
            "" => "Global namespace",
            _ => "namespace " + Encode(value),
        };
    }

    private static string EncodeStatus(CoverageStatus value)
    {
        Debug.Assert(Enum.IsDefined(value));
        return value switch
        {
            CoverageStatus.NotCovered => "not covered",
            CoverageStatus.Covered => "covered",
            CoverageStatus.Partial => "partial",
            _ => "not coverable",
        };
    }

    private static string StatusToClass(CoverageStatus value)
    {
        Debug.Assert(Enum.IsDefined(value));
        return value switch
        {
            CoverageStatus.NotCovered => "red",
            CoverageStatus.Covered => "green",
            CoverageStatus.Partial => "yellow",
            _ => "blank",
        };
    }
}
