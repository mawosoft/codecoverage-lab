// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ReportComparer.Helpers;

namespace ReportComparer;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "Created and disposed in WriteResult().")]
internal sealed partial class HtmlResultWriter
{
    private readonly ReportComparison _reportComparison;
    private readonly string _targetDirectoryPath;
    private readonly Dictionary<RealSource, string> _sourceFileTargetNames;
    private readonly Dictionary<ParsedReport, string> _reportFileLinks;
    private readonly string? _homeNavig;
    private readonly Dictionary<EquatableSequence<ParsedReport>, (string id, string html)> _hiddenReportGroups = [];
    private readonly StringBuilder _scratchBuilder = new(512);
    private int _uniqueId;
    private RealSource _realSource = null!;
    private StreamWriter _writer = null!;
    private TextReader _reader = null!;

    public HtmlResultWriter(ReportComparison reportComparison, string targetDirectoryPath, string? homeLink, string? homeText)
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
                ? Encode(r.Name)
                : $"""<a href="{Encode(relative.Replace('\\', '/'))}">{Encode(r.Name)}</a>""";
        });
        if (homeLink is not null)
        {
            _homeNavig = $"""<a href="{Encode(homeLink)}">{Encode(homeText ?? "Home")}</a> &nbsp;&gt;&nbsp; """;
        }
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
                try
                {
                    _reader = File.OpenText(source.FullName);
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    _reader = new StringReader(e.Message);
                }
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
        title = Encode(title);
        _writer.WriteLine($"""
            <!DOCTYPE html>
            <html lang="en"><head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{Encode(title)}</title>
            <link rel="stylesheet" type="text/css" href="main.css" />
            </head>
            <body>
            """);
    }

    private void WriteHtmlEnd()
    {
        _writer.WriteLine("""
            <footer>
            <br /><hr />
            <a href="https://github.com/mawosoft/codecoverage-lab">Copyright (c) 2026 Matthias Wolf, Mawosoft</a>
            </footer>
            <div style="display: none;">
            """);
        foreach ((string id, string html) in _hiddenReportGroups.Values)
        {
            _writer.WriteLine($"""<span id="{id}">{html}</span>""");
        }
        _writer.WriteLine("""
            </div>
            <script src="main.js"></script>
            </body>
            </html>
            """);
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

    private string ReportGroup(IEnumerable<ParsedReport> reports)
    {
        bool owned = false;
        if (reports is not IReadOnlyCollection<ParsedReport> reportCollection)
        {
            reportCollection = reports.ToArray();
            owned = true;
        }
        if (reportCollection.Count == 0) return "";
        string first = "(all)";
        int skip = 0;
        if (reportCollection.Count != _reportComparison.Reports.Count)
        {
            first = _reportFileLinks[reportCollection.First()];
            if (reportCollection.Count == 1) return first;
            skip = 1;
        }
        var key = EquatableSequence.Create(reportCollection);
        if (!_hiddenReportGroups.TryGetValue(key, out var group))
        {
            group = (NextUniqueId(), string.Join("", reportCollection.Skip(skip).Select(r => "<br />" + _reportFileLinks[r])));
            if (!owned) key = EquatableSequence.Create(reportCollection.ToArray());
            _hiddenReportGroups.Add(key, group);
        }
        string idFor = NextUniqueId();
        return $"""
            {first}
            <button class ="small top" data-for="{idFor}">… {reportCollection.Count}</button>
            <span id="{idFor}" class="hidden" data-source="{group.id}"></span>
            """;
    }

    private string Expandable(IEnumerable<string?> lines)
    {
        using var e = lines.GetEnumerator();
        if (!e.MoveNext()) return "";
        string first = Encode(e.Current ?? "(missing)");
        if (!e.MoveNext()) return first;
        string idFor = NextUniqueId();
        _scratchBuilder.Clear().AppendLine(first).AppendLine(null, $"""
            <button class ="small top" data-for="{idFor}">… {lines.Count()}</button>
            <span id="{idFor}" class="hidden">
            """);
        do
        {
            _scratchBuilder.Append("<br />").Append(Encode(e.Current ?? "(missing)"));
        } while (e.MoveNext());
        return _scratchBuilder.AppendLine("</span>").ToString();
    }

    private static string Encode(string? value)
    {
        return value is null ? "" : WebUtility.HtmlEncode(value);
    }

    private static string RowSpan(int value)
    {
        return value <= 1 ? "" : $" rowspan=\"{value}\"";
    }

    private string NextUniqueId()
    {
        return $"id{++_uniqueId}";
    }
}
