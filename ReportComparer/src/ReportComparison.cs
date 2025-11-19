// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ReportComparer;

internal sealed class ReportComparison
{
    public required List<FileInfo> FileInfos { get; set; }

    internal sealed class Builder
    {
        private static readonly XmlReaderSettings s_xmlReaderSettings = new()
        {
            DtdProcessing = DtdProcessing.Ignore,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
            Async = false, // Explicit for clarity
        };

        private readonly Dictionary<string, FileInfo.Builder> _fileInfos = new(StringComparer.OrdinalIgnoreCase);

        public required int CommonReportPathLength { get; init; }

        public void ParseReport(string reportPath)
        {
            using var reader = XmlReader.Create(reportPath, s_xmlReaderSettings);
            reportPath = reportPath[CommonReportPathLength..].Replace('\\', '/');
            var root = XElement.Load(reader);
            if (root.Name == "coverage")
            {
                ParseCobertura(root, reportPath);
            }
            else if (root.Name == "results")
            {
                ParseDynamicCoverage(root, reportPath);
            }
            else
            {
                throw new XmlException("Not a known coverage report.");
            }
        }

        public ReportComparison ToReportComparison()
        {
            int commonSourcePathLength = PathHelper.GetCommonPathLength(_fileInfos.Keys);
            List<FileInfo> fileInfos = new(_fileInfos.Count);
            foreach (var fileInfo in _fileInfos.Values)
            {
                fileInfo.Name = fileInfo.Name[commonSourcePathLength..].Replace('\\', '/');
                fileInfos.Add(fileInfo.ToFileInfo());
            }
            fileInfos.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
            return new ReportComparison
            {
                FileInfos = fileInfos,
            };
        }

        private void ParseCobertura(XElement root, string report)
        {
            var sources = root.Elements("sources").Elements("source").Nodes().OfType<XText>().Select(n => n.Value).ToArray();
            string? sourceRoot = sources.Length == 1 ? sources[0] : null;
            if (string.IsNullOrEmpty(sourceRoot)) sourceRoot = null;
            foreach (var package in root.Elements("packages").Elements("package"))
            {
                string packageName = (string)package.Attribute("name")!;
                foreach (var @class in package.Elements("classes").Elements("class"))
                {
                    string fileName = (string)@class.Attribute("filename")!;
                    if (sourceRoot is not null) fileName = Path.Combine(sourceRoot, fileName);
                    if (!_fileInfos.TryGetValue(fileName, out var fileInfo))
                    {
                        fileInfo = new FileInfo.Builder { Name = fileName };
                        _fileInfos.Add(fileName, fileInfo);
                    }
                    string className = $"{packageName}:{(string)@class.Attribute("name")!}";
                    foreach (var method in @class.Elements("methods").Elements("method"))
                    {
                        string methodName = $"{className}.{(string)method.Attribute("name")!}{(string)method.Attribute("signature")!}";
                        ParseCoberturaLines(method, fileInfo, methodName, report);
                    }
                    ParseCoberturaLines(@class, fileInfo, className, report);
                }
            }
        }

        private static void ParseCoberturaLines(XElement parent, FileInfo.Builder fileInfo, string container, string report)
        {
            foreach (var line in parent.Elements("lines").Elements("line"))
            {
                var rangeCoverage = new RangeCoverage.Builder
                {
                    Status = CoverageStatus.NotCovered,
                    StartLine = (int)line.Attribute("number")!,
                    Hits = (int)line.Attribute("hits")!,
                };
                if (rangeCoverage.Hits != 0) rangeCoverage.Status = CoverageStatus.Covered;
                ReadOnlySpan<char> conditionCoverage = (string?)line.Attribute("condition-coverage");
                int pos = conditionCoverage.IsEmpty ? -1 : conditionCoverage.IndexOf('(');
                if (pos >= 0)
                {
                    var span = conditionCoverage[(pos + 1)..];
                    pos = span.IndexOf("/");
                    rangeCoverage.CoveredBranches = int.Parse(span[..pos], CultureInfo.InvariantCulture);
                    rangeCoverage.TotalBranches = int.Parse(span[(pos + 1)..span.IndexOf(')')], CultureInfo.InvariantCulture);
                    if (rangeCoverage.CoveredBranches != rangeCoverage.TotalBranches) rangeCoverage.Status = CoverageStatus.Partial;
                }
                if (!conditionCoverage.IsEmpty)
                {
                    List<double>? conditions = null;
                    foreach (var condition in line.Elements("conditions").Elements("condition"))
                    {
                        var coverage = ((string?)condition.Attribute("coverage")).AsSpan().TrimEnd('%');
                        if (!coverage.IsEmpty)
                        {
                            conditions ??= [];
                            conditions.Add(double.Parse(coverage, CultureInfo.InvariantCulture));
                        }
                    }
                    if (conditions is not null)
                    {
                        rangeCoverage.Conditions = new EquatableSequence<double>(conditions);
                    }
                }
                fileInfo.AddRangeCoverage(rangeCoverage, container, report);
            }
        }

        private void ParseDynamicCoverage(XElement root, string report)
        {
            foreach (var module in root.Elements("modules").Elements("module"))
            {
                string moduleName = (string)module.Attribute("name")!;
                var sourcesById = new Dictionary<int, string>();
                foreach (var source in module.Elements("source_files").Elements("source_file"))
                {
                    // Cannot use Add() here because dupes are possible.
                    sourcesById[(int)source.Attribute("id")!] = (string)source.Attribute("path")!;
                }
                foreach (var function in module.Elements("functions").Elements("function"))
                {
                    string? @namespace = (string?)function.Attribute("namespace");
                    if (!string.IsNullOrEmpty(@namespace)) @namespace += '.';
                    string? typeName = (string?)function.Attribute("type_name");
                    if (!string.IsNullOrEmpty(typeName)) typeName += '.';
                    string container = $"{moduleName}:{@namespace}{typeName}{(string)function.Attribute("name")!}";
                    ParseDynamicCoverageRanges(function, sourcesById, container, report);
                }
            }
        }

        private void ParseDynamicCoverageRanges(XElement function, Dictionary<int, string> sourcesById, string container, string report)
        {
            int lastSourceId = -1;
            FileInfo.Builder? fileInfo = null;
            string fileName = "";
            foreach (var range in function.Elements("ranges").Elements("range"))
            {
                int sourceId = (int)range.Attribute("source_id")!;
                if (sourceId != lastSourceId)
                {
                    lastSourceId = sourceId;
                    fileName = sourcesById[sourceId];
                    _fileInfos.TryGetValue(fileName, out fileInfo);
                }
                if (fileInfo is null)
                {
                    fileInfo = new FileInfo.Builder { Name = fileName };
                    _fileInfos.Add(fileName, fileInfo);
                }
                var rangeCoverage = new RangeCoverage.Builder
                {
                    Status = range.Attribute("covered")?.Value switch
                    {
                        "yes" => CoverageStatus.Covered,
                        "partial" => CoverageStatus.Partial,
                        _ => CoverageStatus.NotCovered,
                    },
                    StartLine = (int)range.Attribute("start_line")!,
                    StartColumn = (int)range.Attribute("start_column")!,
                    EndLine = (int)range.Attribute("end_line")!,
                    EndColumn = (int)range.Attribute("end_column")!,
                };
                fileInfo.AddRangeCoverage(rangeCoverage, container, report);
            }
        }
    }
}
