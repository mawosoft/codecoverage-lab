// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ReportComparer;

internal sealed partial class HtmlResultWriter
{
    private static readonly string[] s_metricsColumnHeaders = GetMetricsColumnHeaders();

    private static string[] GetMetricsColumnHeaders()
    {
        var properties = typeof(ReportedMetrics).GetProperties();
        Debug.Assert(properties.Length == ReportedMetrics.Count + 1);
        string[] result = new string[ReportedMetrics.Count];
        for (int i = 0, k = 0; i < properties.Length; i++)
        {
            if (properties[i].Name == "Item") continue;
            result[k++] = Encode(Regex.Replace(properties[i].Name, "(?<=.)([A-Z])", " $1"));
        }
        return result;
    }

    private static (int columnMask, int fractionMask) GetMetricsMask(IEnumerable<ReportedMetrics?> metrics)
    {
        int columnMask = 0;
        int fractionMask = 0;
        foreach (ReportedMetrics? metric in metrics)
        {
            if (metric is null) continue;
            for (int i = 0; i < ReportedMetrics.Count; i++)
            {
                double? value = metric[i];
                if (!value.HasValue) continue;
                int mask = 1 << i;
                columnMask |= mask;
                if ((fractionMask & mask) == 0 && value.Value - Math.Floor(value.Value) != 0)
                {
                    fractionMask |= mask;
                }
            }
        }
        return (columnMask, fractionMask);
    }

    private void WriteMetricsHeaders(int columnMask, params ReadOnlySpan<string> headersBefore)
    {
        _writer.WriteLine("<colgroup>");
        for (int i = 0; i < headersBefore.Length; i++)
        {
            _writer.Write("<col />");
        }
        _writer.Write($"""
            <col span="{int.PopCount(columnMask)}" class="number" />
            <col />
            </colgroup>
            <thead class="sticky"><tr>
            """);
        for (int i = 0; i < headersBefore.Length; i++)
        {
            _writer.Write($"<th>{Encode(headersBefore[i])}</th>");
        }
        for (int i = 0; i < ReportedMetrics.Count; i++)
        {
            if ((columnMask & (1 << i)) != 0)
            {
                _writer.Write($"<th>{s_metricsColumnHeaders[i]}</th>");
            }
        }
        _writer.WriteLine("<th>Reports</th></tr></thead>");
    }

    private void WriteMetricsData(
        int columnMask,
        int fractionMask,
        bool startRow,
        IEnumerable<(ReportedMetrics? metrics, IEnumerable<ParsedReport> reports)> data)
    {
        foreach (var (metrics, reports) in data)
        {
            if (startRow) _writer.Write("<tr>");
            startRow = true;
            for (int i = 0; i < ReportedMetrics.Count; i++)
            {
                int mask = 1 << i;
                if ((columnMask & mask) != 0)
                {
                    _writer.Write("<td>");
                    double? v = metrics?[i];
                    if (v.HasValue)
                    {
                        _writer.Write(v.Value.ToString((fractionMask & mask) != 0 ? "0.00" : null, null));
                    }
                    _writer.Write("</td>");
                }
            }
            _writer.WriteLine($"""<td class="left">{ReportGroup(reports)}</td></tr>""");
        }
    }
}
