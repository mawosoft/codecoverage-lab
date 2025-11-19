// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReportComparer;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine($"Usage: {nameof(ReportComparer)} <outputfile> <inputfile_patterns>");
            return 1;
        }
        string outputPath = args[0];
        HashSet<string> reportPaths = new(StringComparer.OrdinalIgnoreCase);
        for (int i = 1; i < args.Length; i++)
        {
            string path = args[i];
            if (File.Exists(path))
            {
                reportPaths.Add(Path.GetFullPath(path));
            }
            else
            {
                string pattern = "*";
                if (!Directory.Exists(path))
                {
                    pattern = Path.GetFileName(path);
                    path = Path.GetDirectoryName(path) ?? "";
                }
                reportPaths.UnionWith(Directory.EnumerateFiles(Path.GetFullPath(path), pattern));
            }
        }
        if (reportPaths.Count == 0)
        {
            Console.WriteLine("No reports matched the specified pattern(s).");
            return 1;
        }
        int commonReportPathLength = PathHelper.GetCommonPathLength(reportPaths);
        var reportComparisonBuilder = new ReportComparison.Builder { CommonReportPathLength = commonReportPathLength };
        foreach (var path in reportPaths)
        {
            try
            {
                reportComparisonBuilder.ParseReport(path);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Console.WriteLine($"Failed to parse {path}\n{ex.Message}");
            }
        }
        var reportComparison = reportComparisonBuilder.ToReportComparison();
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances
        using var fs = File.Create(outputPath);
        JsonSerializer.Serialize(fs, reportComparison.FileInfos, options);
        return 0;
    }
}
