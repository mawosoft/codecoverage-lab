// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Xml.Linq;

namespace ReportComparer;

internal static partial class ReportParser
{
    private static void ParseDynamicCoverage(XElement root, ParsedReport report)
    {
        foreach (var module in root.Elements(s_modules).Elements(s_module))
        {
            var assembly = report.AddAssembly((string)module.Attribute(s_name)!);
            assembly.ReportedMetrics = ParseDynamicCoverageMetrics(module);
            var sources = ParseDynamicCoverageSources(module, assembly);
            foreach (var function in module.Elements(s_functions).Elements(s_function))
            {
                string typeName = ((string?)function.Attribute(s_type_name) ?? "").Replace('.', '+');
                string? @namespace = (string?)function.Attribute(s_namespace);
                if (!string.IsNullOrEmpty(@namespace)) typeName = @namespace + '.' + typeName;
                var type = assembly.GetOrAddType(typeName);
                var method = type.AddMethod(
                    (string)function.Attribute(s_name)!,
                    ParseDynamicCoverageMetrics(function));
                ParseDynamicCoverageRanges(function, method, sources);
            }
            foreach (var function in module.Elements(s_skipped_functions).Elements(s_skipped_function))
            {
                string name = (string)function.Attribute(s_name)!;
                string? typeName = (string?)function.Attribute(s_type_name);
                if (!string.IsNullOrEmpty(typeName)) name = typeName + '.' + name;
                assembly.AddSkippedFunction(name);
            }
        }
        foreach (var module in root.Elements(s_skipped_modules).Elements(s_skipped_module))
        {
            report.AddSkippedModule((string)module.Attribute(s_name)!);
        }
    }

    private static Dictionary<int, ParsedSource> ParseDynamicCoverageSources(XElement module, ParsedAssembly assembly)
    {
        var results = new Dictionary<int, ParsedSource>();
        foreach (var sourceElement in module.Elements(s_source_files).Elements(s_source_file))
        {
            int id = (int)sourceElement.Attribute(s_id)!;
            if (!results.ContainsKey(id))
            {
                var source = assembly.GetOrAddSource((string)sourceElement.Attribute(s_path)!);
                results.Add(id, source);

            }
        }
        return results;
    }

    private static void ParseDynamicCoverageRanges(
        XElement functionElement,
        ParsedMethod method,
        Dictionary<int, ParsedSource> sources)
    {
        int lastSourceId = -1;
        ParsedSource source = null!;
        foreach (var rangeElement in functionElement.Elements(s_ranges).Elements(s_range))
        {
            int sourceId = (int)rangeElement.Attribute(s_source_id)!;
            if (sourceId != lastSourceId)
            {
                lastSourceId = sourceId;
                source = sources[sourceId];
            }
            method.AddRange(
                source,
                new Range(
                    (int)rangeElement.Attribute(s_start_line)!,
                    (int)rangeElement.Attribute(s_start_column)!,
                    (int)rangeElement.Attribute(s_end_line)!,
                    (int)rangeElement.Attribute(s_end_column)!),
                (string)rangeElement.Attribute(s_covered)! switch
                {
                    "yes" => CoverageStatus.Covered,
                    "partial" => CoverageStatus.Partial,
                    _ => CoverageStatus.NotCovered,
                },
                null);
        }
    }

    private static ReportedMetrics ParseDynamicCoverageMetrics(XElement element)
    {
        return new ReportedMetrics
        {
            BlockCoverage = ParseDouble(element.Attribute(s_block_coverage)),
            LineCoverage = ParseDouble(element.Attribute(s_line_coverage)),
            BlocksCovered = ParseInt(element.Attribute(s_blocks_covered)),
            BlocksNotCovered = ParseInt(element.Attribute(s_blocks_not_covered)),
            LinesCovered = ParseInt(element.Attribute(s_lines_coveredDynamicCoverage)),
            LinesPartiallyCovered = ParseInt(element.Attribute(s_lines_partially_covered)),
            LinesNotCovered = ParseInt(element.Attribute(s_lines_not_covered)),
        };
    }
}
