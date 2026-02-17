// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

internal sealed partial class HtmlResultWriter
{
    private void WriteIndexPage()
    {
        WriteHtmlStart("Report Comparison");
        _writer.WriteLine($"<h1>{_homeNavig}Report Comparison</h1>");
        WriteReportTypes();

        _writer.WriteLine("""
            <details open><summary><h2>Source Files per Assembly <button class="help" data-for="help1">i</button></h2></summary>
            <div id="help1" class="hidden"><table class="help"><tbody>
            <tr>
              <td>Range Overlaps</td>
              <td>Whether at least one of the reports contains overlapping coverage ranges.</td>
            </tr>
            <tr>
              <td>Ambiguous Types</td>
              <td>Whether at least one of the reports contains type names that do not match the actual type.</td>
            </tr>
            <tr>
              <td>Name Groups</td>
              <td>Number of report groups with the same type and method names.</td>
            </tr>
            <tr>
              <td>Coverage Groups</td>
              <td>Number of report groups with the same coverage data.</td>
            </tr>
            <tr>
              <td>Line Status Groups</td>
              <td>Number of report groups with the same aggregated line status.</td>
            </tr>
            </tbody></table></div>
            """);
        WriteAssembliesAndSourceFiles();
        _writer.WriteLine("</details>");

        _writer.WriteLine("""
            <h2>Report Groups</h2>
            <details open><summary><h3>By Content <button class="help" data-for="help2">i</button></h3></summary>
            <p id="help2" class="help hidden">
            Assembly names, source file names, nested type separators, parameter spacing, reported metrics, and the order
            of data elements have been normalized before comparing the reports. Aside from these normalizations, reports in
            the same group are identical. Consider removing the duplicates for better readability of the comparison results.
            </p>
            """);
        IEnumerable<IEnumerable<ParsedReport>> group = _reportComparison.Reports.GroupBy(r => r, ParsedReport.EqualityComparer.Instance);
        WriteReportGroupTable(group);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details open><summary><h3>By Line Status</h3></summary>");
        group = _reportComparison.Reports.Select(report => (
                report,
                sequence: report.Assemblies.SelectMany(a => a.Sources)
                    .Select(s => (
                        s.RealSource,
                        s.LinesCoverage.Select(line => ParsedRange.AggregateStatus(line.Values)).ToEquatableSequence()))
                    .ToEquatableSequence()))
            .GroupBy(vt => vt.sequence, (_, g) => g.Select(vt => vt.report));
        WriteReportGroupTable(group);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>By Type Names</h3></summary>");
        group = _reportComparison.Reports.Select(report => (
                report,
                set: report.Assemblies.SelectMany(a => a.Types)
                    .Select(t => (t.RealType, t.Name))
                    .ToEquatableSet()))
            .GroupBy(vt => vt.set, (_, g) => g.Select(vt => vt.report));
        WriteReportGroupTable(group);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>By Method Names</h3></summary>");
        group = _reportComparison.Reports.Select(report => (
                report,
                set: report.Assemblies.SelectMany(a => a.Types)
                    .SelectMany(t => t.Methods)
                    .Select(m => (m.RealMethod, m.Name))
                    .ToEquatableSet()))
            .GroupBy(vt => vt.set, (_, g) => g.Select(vt => vt.report).ToArray());
        WriteReportGroupTable(group);
        _writer.WriteLine("</details>");

        _writer.WriteLine("""
            <h2>Reported Metrics</h2>
            <details><summary><h3>Per Report</h3></summary>
            """);
        WriteReportMetrics();
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>Per Assembly</h3></summary>");
        WriteAssemblyMetrics();
        _writer.WriteLine("</details>");

        _writer.WriteLine("""
            <h2>Miscellaneous</h2>
            <details><summary><h3>Sources</h3></summary>
            """);
        var sequenceGroups = _reportComparison.Reports.Select(report => (report, sequence: report.SourceRoots.ToEquatableSet(StringComparer.OrdinalIgnoreCase)))
            .GroupBy(vt => vt.sequence, (sequence, g) => (sequence: sequence.Values.AsEnumerable(), reports: g.Select(vt => vt.report).ToArray()));
        WriteSequenceGroupTable("Sources", sequenceGroups);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>Skipped Modules</h3></summary>");
        sequenceGroups = _reportComparison.Reports.Select(report => (report, sequence: EquatableSequence.Create(report.SkippedModules)))
            .GroupBy(vt => vt.sequence, (sequence, g) => (sequence.Values.AsEnumerable(), reports: g.Select(vt => vt.report).ToArray()));
        WriteSequenceGroupTable("Skipped Modules", sequenceGroups);
        _writer.WriteLine("</details>");

        _writer.WriteLine("<details><summary><h3>Skipped Functions per Assembly</h3></summary>");
        WriteAssembliesAndSkippedFunctions();
        _writer.WriteLine("</details>");
        WriteHtmlEnd();
    }

    private void WriteReportTypes()
    {
        _writer.WriteLine("""<table class="datagrid"><tbody class="right">""");
        foreach (var kvp in _reportComparison.Reports.CountBy(r => r.ReportType).OrderBy(kvp => kvp.Key))
        {
            _writer.WriteLine($"""<tr><td>{kvp.Key} reports</td><td>{kvp.Value}</td></tr>""");
        }
        _writer.WriteLine($"""
            <tr><td>Total</td><td>{_reportComparison.Reports.Count}</td></tr>
            </tbody></table>
            """);
    }

    private void WriteAssembliesAndSourceFiles()
    {
        _writer.WriteLine("""
            <table class="datagrid">
            <colgroup>
            <col span="2" />
            <col span="5" class="number" />
            <col />
            </colgroup>
            <thead class="sticky"><tr>
            <th colspan="2">Source Files</th>
            <th>Range Overlaps</th>
            <th>Ambiguous Types</th>
            <th>Name Groups</th>
            <th>Coverage Groups</th>
            <th>Line Status Groups</th>
            <th>Reports</th>
            </tr></thead>
            <tbody class="right">
            """);
        int index = 0;
        foreach (var realAssembly in _reportComparison.RealAssemblies)
        {
            foreach ((string? name, IEnumerable<ParsedReport> reports) in realAssembly.ParsedAssemblies.GroupBy(
                a => a.FullName,
                (name, g) => (name, reports: g.Select(g => g.ParentReport))))
            {
                _writer.WriteLine($"""
                    <tr class="blue left"><td colspan="7">{Encode(name)}</td>
                    <td>{ReportGroup(reports)}</td></tr>
                    """);
            }
            var assemblyReports = _reportComparison.Reports;
            if (realAssembly.Reports.Count != _reportComparison.Reports.Count)
            {
                assemblyReports = realAssembly.Reports;
                _writer.WriteLine($"""
                    <tr class="blue left"><td colspan="7">(missing)</td>
                    <td>{ReportGroup(_reportComparison.Reports.Except(assemblyReports))}</td></tr>
                    """);
            }
            string lastDirectory = "";
            bool altBack = false;
            foreach (var realSource in realAssembly.RealSources)
            {
                if (!string.Equals(lastDirectory, realSource.DirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    lastDirectory = realSource.DirectoryName;
                    altBack = false;
                    _writer.WriteLine($"""
                        <tr class="altblue"><td></td>
                        <td class="left" colspan="7">{Encode(lastDirectory)}</td></tr>
                        """);
                }
                string rowSpan = RowSpan(realSource.ParsedSources.Count == assemblyReports.Count ? 1 : 2);
                string tr = altBack ? "<tr class=\"altback\">" : "<tr>";
                altBack = !altBack;
                _writer.WriteLine($"""
                    {tr}
                    <td{rowSpan}>{++index}</td>
                    <td class="left"><a href="{Encode(_sourceFileTargetNames[realSource])}"
                        >{Encode(realSource.FileName)}</a></td>
                    <td{rowSpan} class="center">{(realSource.HasRangeOverlaps ? "yes" : "")}</td>
                    <td{rowSpan} class="center">{(realSource.TypesAreAmbiguous ? "yes" : "")}</td>
                    <td{rowSpan}>{realSource.NameGroupCount}</td>
                    <td{rowSpan}>{realSource.CoverageGroupCount}</td>
                    <td{rowSpan}>{realSource.StatusGroupCount}</td>
                    <td class="left">{ReportGroup(realSource.Reports)}</td>
                    </tr>
                    """);
                if (realSource.ParsedSources.Count != assemblyReports.Count)
                {
                    _writer.WriteLine($"""
                        {tr}<td class="left">(missing)</td>
                        <td class="left">{ReportGroup(assemblyReports.Except(realSource.Reports))}</td></tr>
                        """);
                }
            }
        }
        _writer.WriteLine("</tbody></table>");
    }

    private void WriteReportMetrics()
    {
        var reportGroups = _reportComparison.Reports.GroupBy(r => r.ReportedMetrics, (metrics, g) => (metrics, g.AsEnumerable()));
        (int columnMask, int fractionMask) = GetMetricsMask(reportGroups.Select(vt => vt.metrics));
        _writer.WriteLine("""<table class="datagrid">""");
        WriteMetricsHeaders(columnMask);
        _writer.WriteLine("""<tbody class="right">""");
        WriteMetricsData(columnMask, fractionMask, startRow: true, reportGroups);
        _writer.WriteLine("</tbody></table>");
    }

    private void WriteAssemblyMetrics()
    {
        var assemblyGroups = _reportComparison.RealAssemblies.Select(realAssembly => (
            realAssembly.Name,
            data: realAssembly.ParsedAssemblies.GroupBy(
                    pa => pa.ReportedMetrics,
                    (metrics, g) => (metrics, reports: g.Select(pa => pa.ParentReport)))
                .ToArray()));
        (int columnMask, int fractionMask) = GetMetricsMask(assemblyGroups.SelectMany(vt => vt.data).Select(d => d.metrics));
        _writer.WriteLine("""<table class="datagrid zebra">""");
        WriteMetricsHeaders(columnMask, "Assembly");
        foreach ((string name, (ReportedMetrics? metrics, IEnumerable<ParsedReport> reports)[] data) in assemblyGroups)
        {
            _writer.Write($"""
                <tbody class="right"><tr>
                <td{RowSpan(data.Length)}>{Encode(name)}</td>
                """);
            WriteMetricsData(columnMask, fractionMask, startRow: false, data);
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
                <tbody><tr><td colspan="2">{Encode(realAssembly.Name)}</td></tr></tbody>
                <tbody>
                """);
            foreach ((EquatableSequence<string> sequence, IEnumerable<ParsedReport> reports) in realAssembly.ParsedAssemblies.GroupBy(
                pa => EquatableSequence.Create(pa.SkippedFunctions),
                (sequence, g) => (sequence, reports: g.Select(pa => pa.ParentReport))))
            {
                _writer.WriteLine($"""
                    <tr><td>{Expandable(sequence.Values)}</td>
                    <td>{ReportGroup(reports)}</td></tr>
                    """);
            }
            _writer.WriteLine("</tbody>");
        }
        _writer.WriteLine("</table>");
    }

    private void WriteSequenceGroupTable(
        string? sequenceHeader,
        IEnumerable<(IEnumerable<string> sequence, ParsedReport[] reports)> sequenceGroups)
    {
        _writer.WriteLine("""<table class="datagrid">""");
        if (sequenceHeader is not null)
        {
            _writer.WriteLine($"""
                <thead><tr><th>{Encode(sequenceHeader)}</th>
                <th>Reports</th></tr></thead>
                """);
        }
        _writer.WriteLine("<tbody>");
        foreach ((IEnumerable<string> sequence, ParsedReport[] reports) in sequenceGroups)
        {
            _writer.WriteLine($"""
                    <tr><td>{Expandable(sequence)}</td>
                    <td>{ReportGroup(reports)}</td></tr>
                    """);
        }
        _writer.WriteLine("</tbody></table>");
    }

    private void WriteReportGroupTable(IEnumerable<IEnumerable<ParsedReport>> reportGroups)
    {
        _writer.WriteLine("""<table class="datagrid"><tbody>""");
        int index = 0;
        foreach (var reportGroup in reportGroups)
        {
            _writer.WriteLine($"""
                    <tr><td class="right">{++index}</td>
                    <td>{ReportGroup(reportGroup)}</td></tr>
                    """);
        }
        _writer.WriteLine("</tbody></table>");
    }
}
