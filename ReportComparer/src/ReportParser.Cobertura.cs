// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

internal static partial class ReportParser
{
    private static void ParseCobertura(XElement root, ParsedReport report)
    {
        var sources = root.Elements(s_sources).Elements(s_source).Nodes().OfType<XText>()
            .Select(n => n.Value)
            .ToArray();
        report.AddSourceRoots(sources);
        string? sourceRoot = sources.Length == 1 ? sources[0] : null;
        if (string.IsNullOrEmpty(sourceRoot)) sourceRoot = null;
        foreach (var package in root.Elements(s_packages).Elements(s_package))
        {
            var assembly = report.AddAssembly((string)package.Attribute(s_name)!);
            assembly.ReportedMetrics = ParseCoberturaMetrics(package);
            foreach (var @class in package.Elements(s_classes).Elements(s_class))
            {
                string sourceFile = (string)@class.Attribute(s_filename)!;
                if (sourceRoot is not null) sourceFile = Path.Combine(sourceRoot, sourceFile);
                var source = assembly.GetOrAddSource(sourceFile);
                var type = assembly.GetOrAddType(((string)@class.Attribute(s_name)!).Replace('/', '+'));
                type.AddReportedMetrics(source, ParseCoberturaMetrics(@class));
                foreach (var methodElement in @class.Elements(s_methods).Elements(s_method))
                {
                    var method = type.AddMethod(
                        (string)methodElement.Attribute(s_name)! + (string)methodElement.Attribute(s_signature)!,
                        ParseCoberturaMetrics(methodElement));
                    ParseCoberturaLines(methodElement, method, source);
                }
            }
        }
        report.ReportedMetrics = new ReportedRootMetrics
        {
            LineRate = ParseDouble(root.Attribute(s_line_rate)),
            BranchRate = ParseDouble(root.Attribute(s_branch_rate)),
            Complexity = ParseInt(root.Attribute(s_complexity)),
            LinesCovered = ParseInt(root.Attribute(s_lines_coveredCobertura)),
            LinesValid = ParseInt(root.Attribute(s_lines_valid)),
            BranchesCovered = ParseInt(root.Attribute(s_branches_covered)),
            BranchesValid = ParseInt(root.Attribute(s_branches_valid))
        };
    }

    private static void ParseCoberturaLines(XElement methodElement, ParsedMethod method, ParsedSource source)
    {
        foreach (var line in methodElement.Elements(s_lines).Elements(s_line))
        {
            int hits = (int)line.Attribute(s_hits)!;
            int coveredBranches = 0;
            int totalBranches = 0;
            ReadOnlySpan<char> conditionCoverage = (string?)line.Attribute(s_condition_coverage);
            int pos = conditionCoverage.IsEmpty ? -1 : conditionCoverage.IndexOf('(');
            if (pos >= 0)
            {
                var span = conditionCoverage[(pos + 1)..];
                pos = span.IndexOf("/");
                coveredBranches = int.Parse(span[..pos], CultureInfo.InvariantCulture);
                totalBranches = int.Parse(span[(pos + 1)..span.IndexOf(')')], CultureInfo.InvariantCulture);
            }
            List<double>? conditions = null;
            if (!conditionCoverage.IsEmpty)
            {
                foreach (var condition in line.Elements(s_conditions).Elements(s_condition))
                {
                    var coverage = ((string?)condition.Attribute(s_coverage)).AsSpan().TrimEnd('%');
                    if (!coverage.IsEmpty)
                    {
                        conditions ??= [];
                        conditions.Add(ParseDouble(coverage).GetValueOrDefault());
                    }
                }
            }
            method.AddRange(
                source,
                new Range((int)line.Attribute(s_number)!),
                hits == 0
                    ? CoverageStatus.NotCovered
                    : coveredBranches != totalBranches
                        ? CoverageStatus.Partial
                        : CoverageStatus.Covered,
                new CoberturaCoverage
                {
                    Hits = hits,
                    CoveredBranches = coveredBranches,
                    TotalBranches = totalBranches,
                    Conditions = EquatableSequence.Create(conditions),
                });
        }
    }

    private static ReportedMetrics ParseCoberturaMetrics(XElement element)
    {
        return new ReportedMetrics
        {
            LineCoverage = ParseDouble(element.Attribute(s_line_rate)),
            BranchRate = ParseDouble(element.Attribute(s_branch_rate)),
            Complexity = ParseInt(element.Attribute(s_complexity)),
        };
    }
}
