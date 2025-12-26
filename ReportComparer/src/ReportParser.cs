// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace ReportComparer;

internal static partial class ReportParser
{
    private static readonly XmlReaderSettings s_xmlReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Ignore,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
        Async = false, // Explicit for clarity
    };

    public static ParsedReport ParseReport(string reportPath, ReportComparison reportComparison)
    {
        using var reader = XmlReader.Create(reportPath, s_xmlReaderSettings);
        var root = XElement.Load(reader);
        ParsedReport report;
        if (root.Name == s_coverage)
        {
            report = new ParsedReport(reportComparison, reportPath, ReportType.Cobertura);
            ParseCobertura(root, report);
        }
        else if (root.Name == s_results)
        {
            report = new ParsedReport(reportComparison, reportPath, ReportType.DynamicCoverage);
            ParseDynamicCoverage(root, report);
        }
        else
        {
            throw new XmlException("Not a known coverage report.");
        }
        return report;
    }

    private static int? ParseInt(XAttribute? attribute) => ParseInt(attribute?.Value);
    private static double? ParseDouble(XAttribute? attribute) => ParseDouble(attribute?.Value);

    private static int? ParseInt(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty || value is "NaN") return null;
        // NumberStyles taken from XmlConvert.ToInt32()
        return int.Parse(
            value,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
            NumberFormatInfo.InvariantInfo);
    }

    private static double? ParseDouble(scoped ReadOnlySpan<char> value)
    {
        if (value.IsEmpty || value is "NaN") return null;
        int pos = value.IndexOf(',');
        if (pos >= 0)
        {
            Span<char> span = stackalloc char[value.Length];
            value.CopyTo(span);
            span[pos] = '.';
            value = span;
        }
        // MSCC doesn't use InvariantCulture when writing multiple formats.
        // NumberStyles taken from XmlConvert.ToDouble()
        return Math.Round(double.Parse(
            value,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent
            | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
            NumberFormatInfo.InvariantInfo), 2);
    }
}
