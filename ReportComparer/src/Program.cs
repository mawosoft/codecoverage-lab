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
        if (arguments.Count < 2)
        {
            Console.WriteLine($"Usage: {nameof(ReportComparer)} <targetdir> <reportfiles>");
            return 1;
        }
        var targetPath = Path.GetFullPath(arguments[0]);
        Directory.CreateDirectory(targetPath);
        var comparison = new ReportComparison();
        for (int i = 1; i < arguments.Count; i++)
        {
            comparison.ParseReport(arguments[i]);
        }
        comparison.Freeze();
        var writer = new HtmlResultWriter(comparison, targetPath);
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
}
