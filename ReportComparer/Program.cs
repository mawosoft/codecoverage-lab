// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ReportComparer;

internal static class Program
{
    public static int Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var arguments = ExpandArguments(args);
        string? jsonFile = null;
        string? htmlDir = null;
        string? homeLink = null;
        string? homeText = null;
        int i = 0;
        while (i < arguments.Count && arguments[i].StartsWith('-'))
        {
            switch (arguments[i++].ToUpperInvariant())
            {
                case "--JSON":
                    jsonFile = Path.GetFullPath(arguments[i++]);
                    break;
                case "--HTML":
                    htmlDir = Path.GetFullPath(arguments[i++]);
                    break;
                case "--HOMELINK":
                    homeLink = arguments[i++];
                    break;
                case "--HOMETEXT":
                    homeText = arguments[i++];
                    break;
                default:
                    return Usage();
            }
        }
        if (i >= arguments.Count) return Usage();
        if (htmlDir is null)
        {
            if (jsonFile is null) return Usage();
            if (homeLink is not null || homeText is not null) return Usage();
        }
        else
        {
            if (homeText is not null && homeLink is null) return Usage();
        }

        var comparison = new ReportComparison();
        while (i < arguments.Count)
        {
            comparison.ParseReport(arguments[i++]);
        }
        comparison.Freeze();

        if (htmlDir is not null)
        {
            var writer = new HtmlResultWriter(comparison, htmlDir, homeLink, homeText);
            writer.WriteResult();
        }

        if (jsonFile is not null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(jsonFile)!);
            using var fs = File.Create(jsonFile);
            var fn = new Func<IEnumerable<ParsedReport>, IEnumerable<int>>(g => g.Select(r => r.Order - 1));
            var groups = new
            {
                Version = 1,
                Reports = comparison.Reports.Select(r => r.FullName),
                ContentGroups = comparison.ContentGroups.Select(fn),
                CoreContentGroups = comparison.CoreContentGroups.Select(fn),
                NameGroups = comparison.NameGroups.Select(fn),
                CoverageGroups = comparison.CoverageGroups.Select(fn),
                LineStatusGroups = comparison.LineStatusGroups.Select(fn),
            };
            JsonSerializer.Serialize(fs, groups);
        }

        return 0;
    }

    private static List<string> ExpandArguments(string[] args)
    {
        List<string> results = new(args.Length);
        foreach (var arg in args)
        {
            if (arg.StartsWith('@'))
            {
                using var reader = File.OpenText(arg[1..]);
                string? line;
                while ((line = reader.ReadLine()) is not null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line[0] is '#' or ';') continue;
                    results.Add(line);
                }
            }
            else
            {
                results.Add(arg);
            }
        }
        return results;
    }

    private static int Usage()
    {
        Console.WriteLine("""
            Usage:
              reportcomparer <jsonoptions> <htmloptions> <reportfiles>
              reportcomparer @<responsefile>

            JSON Options:    --json <jsonfile>
            HTML Options:    --html <htmldir> [--homelink <homelink> [--hometext <hometext>]]

            Arguments:
              <jsonfile>      Path of a file to write the JSON summary to.
              <htmldir>       Directory where the HTML result files should be saved. Will be created if it doesn't exist.
              <homelink>      Optional custom link to include in navigation.
              <hometext>      Text for custom link. Defaults to 'Home'.
              <reportfiles>   Any number of XML report files in Cobertura or DynamicCoverage format.
              <responsefile>  A response file containing one option or argument per line.
                              Empty lines and lines starting with '#' or ';' are ignored.
            """);
        return 1;
    }
}
