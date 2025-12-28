// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using ReportComparer.Helpers;

namespace ReportComparer;

internal sealed partial class HtmlResultWriter
{
    private const string Missing = "(missing)";

    private readonly ReportComparison _reportComparison;
    private readonly string _targetDirectoryPath;
    private readonly Dictionary<RealSource, string> _sourceFileTargetNames;
    private readonly Dictionary<ParsedReport, string> _reportFileLinks;
    private readonly Dictionary<string, string> _hiddenReportGroups = [];
    private int _uniqueId;
    private RealSource _realSource = null!;
    private StreamWriter _writer = null!;
    private StreamReader _reader = null!;

    public HtmlResultWriter(ReportComparison reportComparison, string targetDirectoryPath)
    {
        Debug.Assert(reportComparison.Frozen);
        _reportComparison = reportComparison;
        _targetDirectoryPath = Path.GetFullPath(targetDirectoryPath);
        _sourceFileTargetNames = reportComparison.RealAssemblies.SelectMany(ra => ra.RealSources)
            .ToDictionary(
                rs => rs,
                rs => PathHelper.SanitizeFileName(rs.ParentAssembly.Name + '_' + rs.Name + ".html"));
        _reportFileLinks = _reportComparison.Reports.ToDictionary(r => r, r =>
        {
            string relative = Path.GetRelativePath(targetDirectoryPath, r.FullName);
            return Path.IsPathRooted(relative)
                ? WebUtility.HtmlEncode(r.Name)
                : $"""<a href="{WebUtility.HtmlEncode(relative.Replace('\\', '/'))}">{WebUtility.HtmlEncode(r.Name)}</a>""";
        });
    }

    public void WriteResult()
    {
        Directory.CreateDirectory(_targetDirectoryPath);
        WriteResource("main.css");
        WriteResource("main.js");
        _hiddenReportGroups.Clear();
        _uniqueId = 0;
        try
        {
            _writer = File.CreateText(Path.Combine(_targetDirectoryPath, "index.html"));
            WriteIndexPage();
            _writer.Close();
            foreach (var source in _sourceFileTargetNames.Keys)
            {
                _hiddenReportGroups.Clear();
                _uniqueId = 0;
                _realSource = source;
                _writer = File.CreateText(Path.Combine(_targetDirectoryPath, _sourceFileTargetNames[source]));
                _reader = File.OpenText(source.FullName);
                WriteSourceFile();
                _reader.Close();
                _writer.Close();
            }
        }
        finally
        {
            _hiddenReportGroups.Clear();
            _writer?.Dispose();
            _reader?.Dispose();
            _writer = null!;
            _reader = null!;
            _realSource = null!;
        }
    }

    private void WriteResource(string fileName)
    {
        using var rs = typeof(HtmlResultWriter).Assembly.GetManifestResourceStream(nameof(ReportComparer) + ".resources." + fileName)!;
        using var fs = File.Create(Path.Combine(_targetDirectoryPath, fileName));
        rs.CopyTo(fs);
    }

    private void WriteHtmlStart(string title)
    {
        title = WebUtility.HtmlEncode(title);
        _writer.WriteLine($"""
            <!DOCTYPE html>
            <html lang="en"><head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{title}</title>
            <link rel="stylesheet" type="text/css" href="main.css" />
            </head>
            <body>
            """);
    }

    private void WriteHtmlEnd()
    {
        _writer.WriteLine("""
            <footer><br /><hr />
            <a href="https://github.com/mawosoft/codecoverage-lab">Copyright (c) 2025 Matthias Wolf, Mawosoft</a>
            </footer>
            """);
        _writer.WriteLine("""<div style="display: none;">""");
        foreach (var kvp in _hiddenReportGroups)
        {
            _writer.WriteLine($"""<span id="{kvp.Value}">{kvp.Key}</span>""");
        }
        _writer.WriteLine("""
            </div>
            <script src="main.js"></script>
            </body></html>
            """);
    }

    private void WriteMetricsHeaderCells()
    {
        _writer.Write("""
            <th>Block Coverage</th>
            <th>Line Coverage</th>
            <th>Branch Rate</th>
            <th>Complexity</th>
            <th>Blocks Covered</th>
            <th>Blocks Not Covered</th>
            <th>Lines Covered</th>
            <th>Lines Partially Covered</th>
            <th>Lines Not Covered</th>
            """);
    }

    private void WriteRootMetricsHeaderCells()
    {
        _writer.Write("""
            <th>Line Rate</th>
            <th>Branch Rate</th>
            <th>Complexity</th>
            <th>Lines Covered</th>
            <th>Lines Valid</th>
            <th>Branches Covered</th>
            <th>Branches Valid</th>
            """);
    }

    private void WriteMetricsDataCells(ReportedMetrics? metrics)
    {
        if (metrics is null)
        {
            _writer.Write("<td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td>");
        }
        else
        {
            _writer.Write($"""
                <td>{metrics.BlockCoverage}</td>
                <td>{metrics.LineCoverage}</td>
                <td>{metrics.BranchRate}</td>
                <td>{metrics.Complexity}</td>
                <td>{metrics.BlocksCovered}</td>
                <td>{metrics.BlocksNotCovered}</td>
                <td>{metrics.LinesCovered}</td>
                <td>{metrics.LinesPartiallyCovered}</td>
                <td>{metrics.LinesNotCovered}</td>
                """);
        }
    }

    private void WriteRootMetricsDataCells(ReportedRootMetrics? metrics)
    {
        if (metrics is null)
        {
            _writer.Write("<td></td><td></td><td></td><td></td><td></td><td></td><td></td>");
        }
        else
        {
            _writer.Write($"""
                <td>{metrics.LineRate}</td>
                <td>{metrics.BranchRate}</td>
                <td>{metrics.Complexity}</td>
                <td>{metrics.LinesCovered}</td>
                <td>{metrics.LinesValid}</td>
                <td>{metrics.BranchesCovered}</td>
                <td>{metrics.BranchesValid}</td>
                """);
        }
    }

    private void WriteReportGroup(ParsedReport[] reports)
    {
        if (reports.Length == 0) return;
        int skip = 0;
        if (reports.Length == _reportComparison.Reports.Count)
        {
            _writer.Write("(all)");
        }
        else
        {
            _writer.Write(_reportFileLinks[reports[0]]);
            if (reports.Length == 1) return;
            skip = 1;
        }
        string html = string.Join("", reports.Skip(skip).Select(r => "<br />" + _reportFileLinks[r]));
        if (!_hiddenReportGroups.TryGetValue(html, out string? idSource))
        {
            idSource = NextId();
            _hiddenReportGroups.Add(html, idSource);
        }
        string idFor = NextId();
        _writer.WriteLine($"""
             <button class ="small top" data-for="{idFor}">… {reports.Length}</button>
            <span id="{idFor}" class="hidden" data-source="{idSource}"></span>
            """);
    }

    private void WriteExpandableSequence(IEnumerable<string?> lines)
    {
        using var e = lines.GetEnumerator();
        if (!e.MoveNext()) return;
        _writer.Write(HtmlEncodeName(e.Current));
        if (!e.MoveNext()) return;
        string idFor = NextId();
        _writer.WriteLine($"""
             <button class ="small top" data-for="{idFor}">… {lines.Count()}</button>
            <span id="{idFor}" class="hidden">
            """);
        do
        {
            _writer.Write($"<br />{HtmlEncodeName(e.Current)}");
        } while (e.MoveNext());
        _writer.WriteLine("</span>");
    }

    private string NextId()
    {
        return $"id{++_uniqueId}";
    }

    private static string HtmlEncodeName(string? name)
    {
        return name is null ? Missing : WebUtility.HtmlEncode(name);
    }

    private static string Rowspan(int value)
    {
        return value <= 1 ? "" : $" rowspan=\"{value}\"";
    }
}
