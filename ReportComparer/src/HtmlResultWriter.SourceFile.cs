// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace ReportComparer;

internal sealed partial class HtmlResultWriter
{
    private void WriteSourceFile()
    {
        WriteHtmlStart(_realSource.Name);
        _writer.WriteLine($"<h1>{WebUtility.HtmlEncode(_realSource.Name)}</h1>");
        _writer.WriteLine($"<p>Assembly: {WebUtility.HtmlEncode(_realSource.ParentAssembly.Name)}</p>");

        _writer.WriteLine("<details><summary><h2>Type and Method Names</h2></summary>");
        WriteParsedNames();
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h2>Reported Metrics</h2></summary>");
        WriteReportedMetrics();
        _writer.WriteLine("</details>");

        _writer.WriteLine("""
            <h2 class="help">Coverage</h2> <button class="help" data-for="help1">?</button>
            <div id="help1" class="hidden"><table class="legend"><tbody>
            <tr>
              <td><table class="lineMarginGlyphs"><tbody>
                  <tr><td class=blank>&nbsp;</td><td class=green>&nbsp;</td><td class=red>&nbsp;</td><td class=yellow>&nbsp;</td></tr>
                  </tbody></table></td>
              <td>Aggregated line status per report group: <em>not coverable, covered, not covered, partial</em>.<br />
                  Click the <button class="small" inert>+</button> button to show the details.</td>
            </tr><tr>
              <td class="redtext">37</td>
              <td>Lines containing overlapping ranges.</td>
            </tr><tr>
              <td>65,17 - 65,42</td>
              <td>Starting line and column (inclusive) to ending line and column (exclusive).<br />
                  Column numbers of 0 indicate that the range encompasses the entire line.</td>
            </tr>
            </tbody></table></div>
            """);
        _writer.WriteLine("""<table class="sourceLines"><tbody>""");
        int lineNumber = 1;
        string? lineText;
        while ((lineText = _reader.ReadLine()) is not null || lineNumber < _realSource.LinesCoverage.Count)
        {
            lineText ??= "";
            RealSource.Line line = lineNumber < _realSource.LinesCoverage.Count
                ? _realSource.LinesCoverage[lineNumber]
                : default;
            string? detailId = null;
            if (line.IsCoverable)
            {
                detailId = NextId();
                _writer.Write($"""<tr><td><button class="small" data-for="{detailId}">+</button></td><td>""");
                WriteLineMarginGlyphs(line);
                _writer.Write(line.HasOverlaps ? """</td><td class="redtext">""" : "</td><td>");
            }
            else
            {
                _writer.Write("<tr><td></td><td></td><td>");
            }
            _writer.WriteLine($"{lineNumber}</td><td><pre>{WebUtility.HtmlEncode(lineText)}</pre></td></tr>");
            if (line.IsCoverable)
            {
                _writer.WriteLine($"""
                        <tr id="{detailId}" class="hidden"><td colspan="3"></td><td>
                        <table class="datagrid zebra"><thead><tr>
                        <th>Status</th><th>Range</th><th>Hits</th><th>Branches</th><th>Conditions</th><th>Reports</th>
                        </tr></thead>
                        """);
                var rangeGroups = line.RangeGroups.OrderBy(rg => ParsedRange.AggregateStatus(rg.parsedRanges));
                foreach (var (parsedRanges, reports) in rangeGroups)
                {
                    WriteLineCoverageDetails(parsedRanges, reports);
                }
                _writer.WriteLine("</table></td></tr>");
            }
            lineNumber++;
        }
        _writer.WriteLine("</tbody></table>");
        WriteHtmlEnd();
    }

    private void WriteParsedNames()
    {
        _writer.WriteLine("""
            <table class="datagrid zebra">
            <thead><tr><th colspan=2>Names</th><th>Reports</th></tr></thead>
            """);
        foreach (var @namespace in _realSource.RealTree)
        {
            _writer.WriteLine($"""<tbody><tr><td colspan="3">namespace {WebUtility.HtmlEncode(@namespace.Key)}</td></tr></tbody>""");
            foreach (var type in @namespace)
            {
                _writer.WriteLine("<tbody>");
                foreach ((string? name, ParsedReport[]? reports) in type.Key.ParsedTypes.GroupBy(
                    kvp => kvp.Value?.Name,
                    (k, g) => (name: g.First().Value?.ShortName, reports: g.Select(g => g.Key).ToArray())))
                {
                    _writer.Write($"""<tr><td colspan="2">{HtmlEncodeName(name)}</td><td>""");
                    WriteReportGroup(reports);
                    _writer.WriteLine("</td></tr>");
                }
                _writer.WriteLine("</tbody>");
                foreach (RealMethod realMethod in type)
                {
                    var nameGroup = realMethod.ParsedMethods.GroupBy(
                            kvp => kvp.Value?.Name,
                            (name, g) => (name, reports: g.Select(g => g.Key).ToArray()))
                        .ToArray();
                    _writer.Write($"""<tbody><tr><td class="right"{Rowspan(nameGroup.Length)}>{realMethod.BoundingRange.range.StartLine}</td>""");
                    bool tr = false;
                    foreach ((string? name, ParsedReport[] reports) in nameGroup)
                    {
                        if (tr) _writer.Write("<tr>");
                        tr = true;
                        _writer.Write($"<td>{HtmlEncodeName(name)}</td><td>");
                        WriteReportGroup(reports);
                        _writer.WriteLine("</td></tr>");
                    }
                    _writer.WriteLine("</tbody>");
                }
            }
        }
        _writer.WriteLine("</table>");
    }

    private void WriteReportedMetrics()
    {
        _writer.Write("""<table class="datagrid zebra"><thead class="sticky"><tr><th colspan=2>Entity</th>""");
        WriteMetricsHeaderCells();
        _writer.WriteLine("""
            <th>Reports</th></tr></thead>
            """);
        foreach (var @namespace in _realSource.RealTree)
        {
            _writer.WriteLine($"""<tbody><tr><td colspan="12">namespace {WebUtility.HtmlEncode(@namespace.Key)}</td></tr></tbody>""");
            foreach (var type in @namespace)
            {
                RealType realType = type.Key;
                var typeMetrics = realType.ParsedTypes.GroupBy(
                        kvp => kvp.Value?.ReportedMetricsForSource(_realSource),
                        (metrics, g) => (metrics, reports: g.Select(g => g.Key).ToArray()))
                    .ToArray();
                var typeNames = realType.ParsedTypes.Select(kvp => kvp.Value?.Name).Distinct();
                if (typeNames.Any(s => s is null)) typeNames = typeNames.Where(s => s is not null).Append(null);
                _writer.Write($"""<tbody class="right"><tr><td class="left" colspan="2"{Rowspan(typeMetrics.Length)}>""");
                WriteExpandableSequence(typeNames);
                _writer.WriteLine("</td>");
                bool trType = false;
                foreach ((ReportedMetrics? metrics, ParsedReport[] reports) in typeMetrics)
                {
                    if (trType) _writer.Write("<tr>");
                    trType = true;
                    WriteMetricsDataCells(metrics);
                    _writer.Write("""<td class="left">""");
                    WriteReportGroup(reports);
                    _writer.WriteLine("</td></tr>");

                }
                _writer.WriteLine("</tbody>");
                foreach (RealMethod realMethod in type)
                {
                    var methodMetrics = realMethod.ParsedMethods.GroupBy(
                            kvp => kvp.Value?.ReportedMetrics,
                            (metrics, g) => (metrics, reports: g.Select(g => g.Key).ToArray()))
                        .ToArray();
                    var methodNames = realMethod.ParsedMethods.Select(kvp => kvp.Value?.Name).Distinct();
                    if (methodNames.Any(s => s is null)) methodNames = methodNames.Where(s => s is not null).Append(null);
                    _writer.Write($"""
                        <tbody class="right">
                        <tr><td{Rowspan(methodMetrics.Length)}>{realMethod.BoundingRange.range.StartLine}</td>
                        <td class="left"{Rowspan(methodMetrics.Length)}>
                        """);
                    WriteExpandableSequence(methodNames);
                    _writer.WriteLine("</td>");
                    bool trMethod = false;
                    foreach ((ReportedMetrics? metrics, ParsedReport[] reports) in methodMetrics)
                    {
                        if (trMethod) _writer.Write("<tr>");
                        trMethod = true;
                        WriteMetricsDataCells(metrics);
                        _writer.Write("""<td class="left">""");
                        WriteReportGroup(reports);
                        _writer.WriteLine("</td></tr>");

                    }
                    _writer.WriteLine("</tbody>");
                }
            }
        }
        _writer.WriteLine("</table>");
    }

    private void WriteLineMarginGlyphs(RealSource.Line line)
    {
        if (!line.IsCoverable) return;
        var statuses = line.RangeGroups.Select(rg => ParsedRange.AggregateStatus(rg.parsedRanges)).Order();
        _writer.Write("""<table class="lineMarginGlyphs"><tbody><tr>""");
        foreach (var status in statuses)
        {
            _writer.Write($"""<td class={CoverageStatusToClassName(status)}>&nbsp;</td>""");
        }
        _writer.WriteLine("</tr></tbody></table>");
    }

    private void WriteLineCoverageDetails(ParsedRange[] parsedRanges, ParsedReport[] reports)
    {
        if (parsedRanges.Length == 0)
        {
            _writer.Write($"""
                <tbody>
                <tr><td class="center">{CoverageStatusToEncodedText(CoverageStatus.None)}</td>
                <td colspan="4"></td><td>
                """);
            WriteReportGroup(reports);
            _writer.WriteLine("</td></tr></tbody>");
            return;
        }

        _writer.WriteLine("<tbody>");
        bool first = true;
        foreach (var parsedRange in parsedRanges)
        {
            var r = parsedRange.Range;
            _writer.Write($"""
                <tr><td class="center">{CoverageStatusToEncodedText(parsedRange.Status)}</td>
                <td>{r.StartLine},{r.StartColumn} - {r.EndLine},{r.EndColumn}</td>
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
                foreach (double condition in c.Conditions.ValuesOrDefault)
                {
                    _writer.Write($" {condition}%");
                }
                _writer.Write("</td>");
            }
            if (first)
            {
                first = false;
                _writer.Write($"<td{Rowspan(parsedRanges.Length)}>");
                WriteReportGroup(reports);
                _writer.Write("</td>");
            }
            _writer.WriteLine("</tr>");
        }
        _writer.WriteLine("</tbody>");
    }

    private static string CoverageStatusToClassName(CoverageStatus status)
    {
        Debug.Assert(Enum.IsDefined(status));
        return status switch
        {
            CoverageStatus.NotCovered => "red",
            CoverageStatus.Covered => "green",
            CoverageStatus.Partial => "yellow",
            _ => "blank",
        };
    }

    private static string CoverageStatusToEncodedText(CoverageStatus status)
    {
        Debug.Assert(Enum.IsDefined(status));
        return status switch
        {
            CoverageStatus.NotCovered => "not covered",
            CoverageStatus.Covered => "covered",
            CoverageStatus.Partial => "partial",
            _ => "not coverable",
        };
    }
}
