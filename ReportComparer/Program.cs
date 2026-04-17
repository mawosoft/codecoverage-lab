// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ReportComparer;

internal static class Program
{
    public static int Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var arguments = ExpandArguments(args);
        string? targetDir = null;
        string? homeLink = null;
        string? homeText = null;
        int i = 0;
        while (i < arguments.Count && arguments[i].StartsWith('-'))
        {
            switch (arguments[i++].ToUpperInvariant())
            {
                case "--TARGETDIR":
                    targetDir = Path.GetFullPath(arguments[i++]);
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
        if (targetDir is null || (homeText is not null && homeLink is null) || i >= arguments.Count)
        {
            return Usage();
        }
        Directory.CreateDirectory(targetDir);
        var comparison = new ReportComparison();
        while (i < arguments.Count)
        {
            comparison.ParseReport(arguments[i++]);
        }
        comparison.Freeze();
        var writer = new HtmlResultWriter(comparison, targetDir, homeLink, homeText);
        writer.WriteResult();
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
              reportcomparer --targetdir <targetdir> [--homelink <homelink> [--hometext <hometext>]] <reportfiles>
              reportcomparer @<responsefile>

            Arguments:
              <targetdir>     Directory where the HTML result files should be saved. Will be created if it doesn't exist.
              <homelink>      Optional custom link to include in navigation.
              <hometext>      Text for custom link. Defaults to 'Home'.
              <reportfiles>   Any number of XML report files in Cobertura or DynamicCoverage format.
              <responsefile>  A response file containing one option or argument per line.
                              Empty lines and lines starting with '#' or ';' are ignored.
            """);
        return 1;
    }
}
