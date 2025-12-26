// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using ReportComparer.Helpers;

namespace ReportComparer;

internal sealed partial class HtmlResultWriter
{
    private void WriteIndexPage()
    {
        WriteHtmlStart("Report Comparison");
        _writer.WriteLine("<h1>Report Comparison</h1>");
        WriteReportTypes();

        _writer.WriteLine("<details open><summary><h2>Source Files per Assembly</h2></summary>");
        WriteAssembliesAndSourceFiles();
        _writer.WriteLine("</details>");

        _writer.WriteLine("<h2>Report Groups</h2>");
        _writer.WriteLine("""
            <details open><summary><h3 class="help">By Content</h3> <button class="help" data-for="help1">?</button></summary>
            <p id="help1" class="hidden">
            Assembly names, source file names, nested type separators, and the order of data elements have been normalized
            before comparing the reports. Apart from these normalizations, reports in the same group are identical.
            Consider removing the duplicates for better readability of the comparison results.
            </p>
            """);
        var group = _reportComparison.Reports.GroupBy(
            r => r,
            (_, g) => g.ToArray(), ParsedReport.EqualityComparer.Instance);
        WriteReportGroupTable(group);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details open><summary><h3>By Line Status</h3></summary>");
        group = _reportComparison.Reports.Select(report => (
                report,
                sequence: EquatableSequence.Create(report.Assemblies.SelectMany(a => a.Sources)
                    .Select(s => (
                        s.RealSource,
                        EquatableSequence.Create(s.LinesCoverage.Select(line =>
                                ParsedRange.AggregateStatus(line.ValuesOrDefault))
                            .ToArray())))
                    .ToArray())))
            .GroupBy(vt => vt.sequence, (_, g) => g.Select(vt => vt.report).ToArray());
        WriteReportGroupTable(group);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>By Type Names</h3></summary>");
        group = _reportComparison.Reports.Select(report => (
                report,
                sequence: EquatableSequence.Create(report.Assemblies.SelectMany(a => a.Types)
                    .Select(t => (t.RealType, t.Name))
                    .ToArray())))
            .GroupBy(vt => vt.sequence, (_, g) => g.Select(vt => vt.report).ToArray());
        WriteReportGroupTable(group);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>By Method Names</h3></summary>");
        group = _reportComparison.Reports.Select(report => (
                report,
                sequence: EquatableSequence.Create(report.Assemblies.SelectMany(a => a.Types)
                    .SelectMany(t => t.Methods)
                    .Select(m => (m.RealMethod, m.Name))
                    .ToArray())))
            .GroupBy(vt => vt.sequence, (_, g) => g.Select(vt => vt.report).ToArray());
        WriteReportGroupTable(group);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<h2>Reported Metrics</h2>");
        _writer.WriteLine("<details><summary><h3>Per Report</h3></summary>");
        WriteReportMetrics();
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>Per Assembly</h3></summary>");
        WriteAssemblyMetrics();
        _writer.WriteLine("</details>");

        _writer.WriteLine("<h2>Miscellaneous</h2>");
        _writer.WriteLine("<details><summary><h3>Sources</h3></summary>");
        var sequenceGroups = _reportComparison.Reports.Select(report => (report, sequence: EquatableSequence.Create(report.SourceRoots)))
            .GroupBy(vt => vt.sequence, (sequence, g) => (sequence, reports: g.Select(vt => vt.report).ToArray()));
        WriteSequenceGroupTable("Sources", sequenceGroups);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>Skipped Modules</h3></summary>");
        sequenceGroups = _reportComparison.Reports.Select(report => (report, sequence: EquatableSequence.Create(report.SkippedModules)))
            .GroupBy(vt => vt.sequence, (sequence, g) => (sequence, reports: g.Select(vt => vt.report).ToArray()));
        WriteSequenceGroupTable("Skipped Modules", sequenceGroups);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>Skipped Functions per Assembly</h3></summary>");
        WriteAssembliesAndSkippedFunctions();
        _writer.WriteLine("</details>");
        WriteHtmlEnd();
    }

    private void WriteReportTypes()
    {
        _writer.WriteLine("""<table class="datagrid"><tbody>""");
        foreach ((ReportType reportType, int count) in _reportComparison.Reports.GroupBy(
                r => r.ReportType, (reportType, g) => (reportType, count: g.Count()))
            .OrderBy(vt => vt.reportType))
        {
            _writer.WriteLine($"""<tr><td>{reportType} reports</td><td class right>{count}</td></tr>""");
        }
        _writer.WriteLine($"""<tr><td>Total</td><td class right>{_reportComparison.Reports.Count}</td></tr>""");
        _writer.WriteLine("</tbody></table>");
    }

    private void WriteAssembliesAndSourceFiles()
    {
        _writer.WriteLine("""
            <table class="datagrid zebra">
            <thead><tr><th colspan="2">Source Files</th><th>Range<br>Overlaps</th><th>Reports</th></tr></thead>
            """);
        int index = 0;
        foreach (var realAssembly in _reportComparison.RealAssemblies)
        {
            _writer.WriteLine("<tbody>");
            foreach ((string? name, ParsedReport[] reports) in realAssembly.ParsedAssemblies.GroupBy(
                kvp => kvp.Value?.FullName,
                (name, g) => (name, reports: g.Select(g => g.Key).ToArray())))
            {
                _writer.Write($"""<tr><td colspan="3">{HtmlEncodeName(name)}</td><td>""");
                WriteReportGroup(reports);
                _writer.WriteLine("</td></tr>");
            }
            _writer.WriteLine("</tbody>");
            foreach (var realSource in realAssembly.RealSources)
            {
                var parsed = realSource.ParsedSources.GroupBy(kvp => kvp.Value is null)
                    .OrderBy(g => g.Key)
                    .Select(g => (missing: g.Key, reports: g.Select(kvp => kvp.Key).ToArray()))
                    .ToArray();
                int? overlaps = realSource.LinesCoverage.Count(line => line.HasOverlaps);
                if (overlaps == 0) overlaps = null;
                _writer.Write($"""
                    <tbody>
                    <tr><td class="right"{Rowspan(parsed.Length)}>{++index}</td>
                    <td><a href="{WebUtility.HtmlEncode(_sourceFileTargetNames[realSource])}"
                    >{WebUtility.HtmlEncode(realSource.Name)}</a></td>
                    <td class=right{Rowspan(parsed.Length)}>{overlaps}</td><td>
                    """);
                WriteReportGroup(parsed[0].reports);
                _writer.WriteLine("</td></tr>");
                if (parsed.Length > 1)
                {
                    _writer.WriteLine($"<tr><td>{Missing}</td><td>");
                    WriteReportGroup(parsed[1].reports);
                    _writer.WriteLine("</td></tr>");
                }
                _writer.WriteLine("</tbody>");
            }
        }
        _writer.WriteLine("</table>");
    }

    private void WriteReportMetrics()
    {
        _writer.Write("""
            <table class="datagrid">
            <thead class="sticky"><tr>
            """);
        WriteRootMetricsHeaderCells();
        _writer.WriteLine("""
            <th>Reports</th></tr></thead>
            <tbody class="right">
            """);
        foreach ((ReportedRootMetrics? metrics, ParsedReport[] reports) in _reportComparison.Reports.GroupBy(
            r => r.ReportedMetrics,
            (metrics, g) => (metrics, reports: g.ToArray())))
        {
            _writer.Write("<tr>");
            WriteRootMetricsDataCells(metrics);
            _writer.Write("""<td class="left">""");
            WriteReportGroup(reports);
            _writer.WriteLine("</td></tr>");

        }
        _writer.WriteLine("</tbody></table>");
    }

    private void WriteAssemblyMetrics()
    {
        _writer.Write("""
            <table class="datagrid zebra">
            <thead class="sticky"><tr>
            <th>Assembly</th>
            """);
        WriteMetricsHeaderCells();
        _writer.WriteLine("<th>Reports</th></tr></thead>");
        foreach (var realAssembly in _reportComparison.RealAssemblies)
        {
            var metricsGroups = realAssembly.ParsedAssemblies.GroupBy(
                    kvp => kvp.Value?.ReportedMetrics,
                    (metrics, g) => (metrics, reports: g.Select(g => g.Key).ToArray()))
                .ToArray();
            _writer.Write($"""<tbody class="right"><tr><td{Rowspan(metricsGroups.Length)}>{WebUtility.HtmlEncode(realAssembly.Name)}</td>""");
            bool tr = false;
            foreach ((ReportedMetrics? metrics, ParsedReport[] reports) in metricsGroups)
            {
                if (tr) _writer.Write("<tr>");
                tr = true;
                WriteMetricsDataCells(metrics);
                _writer.Write("""<td class="left">""");
                WriteReportGroup(reports);
                _writer.WriteLine("</td></tr>");

            }
            _writer.WriteLine("</tbody>");
        }
        _writer.WriteLine("</table>");
    }

    private void WriteAssembliesAndSkippedFunctions()
    {
        _writer.WriteLine("""
            <table class="datagrid zebra">
            <thead><tr><th>Skipped Functions</th><th>Reports</th></tr></thead>
            """);
        foreach (var realAssembly in _reportComparison.RealAssemblies)
        {
            _writer.WriteLine($"""
                <tbody><tr><td colspan="2">{WebUtility.HtmlEncode(realAssembly.Name)}</td></tr></tbody>
                <tbody>
                """);
            foreach ((EquatableSequence<string> sequence, ParsedReport[] reports) in realAssembly.ParsedAssemblies.GroupBy(
                kvp => EquatableSequence.Create(kvp.Value?.SkippedFunctions),
                (sequence, g) => (sequence, reports: g.Select(kvp => kvp.Key).ToArray())))
            {
                _writer.WriteLine("<tr><td>");
                WriteExpandableSequence(sequence.ValuesOrDefault);
                _writer.WriteLine("</td><td>");
                WriteReportGroup(reports);
                _writer.WriteLine("</td></tr>");
            }
            _writer.WriteLine("</tbody>");
        }
        _writer.WriteLine("</table>");
    }

    private void WriteSequenceGroupTable(string? sequenceHeader, IEnumerable<(EquatableSequence<string> sequence, ParsedReport[] reports)> sequenceGroups)
    {
        _writer.WriteLine("""<table class="datagrid">""");
        if (sequenceHeader is not null)
        {
            _writer.WriteLine($"<thead><tr><th>{WebUtility.HtmlEncode(sequenceHeader)}</th><th>Reports</th></tr></thead>");
        }
        _writer.WriteLine("<tbody>");
        foreach ((EquatableSequence<string> sequence, ParsedReport[]? reports) in sequenceGroups)
        {
            _writer.WriteLine("<tr><td>");
            WriteExpandableSequence(sequence.ValuesOrDefault);
            _writer.WriteLine("</td><td>");
            WriteReportGroup(reports);
            _writer.WriteLine("</td></tr>");
        }
        _writer.WriteLine("</tbody></table>");
    }

    private void WriteReportGroupTable(IEnumerable<ParsedReport[]> reportGroups)
    {
        _writer.WriteLine("""<table class="datagrid"><tbody>""");
        int index = 0;
        foreach (var reportGroup in reportGroups)
        {
            _writer.WriteLine($"""<tr><td class="right">{++index}</td><td>""");
            WriteReportGroup(reportGroup);
            _writer.WriteLine("</td></tr>");
        }
        _writer.WriteLine("</tbody></table>");
    }
}
