// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Linq;

namespace ReportComparer;

internal sealed class FileInfo
{
    public required string Name { get; set; }
    public required List<LineInfo> Lines { get; set; }

    internal sealed class Builder
    {
        public required string Name { get; set; }
        public Dictionary<RangeCoverage.Builder, RangeCoverageInfo.Builder> CoverageInfos { get; set; } = [];

        public void AddRangeCoverage(RangeCoverage.Builder coverage, string container, string reportSource)
        {
            if (!CoverageInfos.TryGetValue(coverage, out var info))
            {
                info = new RangeCoverageInfo.Builder(coverage);
                CoverageInfos.Add(coverage, info);
            }
            info.Containers.Add(container);
            info.ReportSources.Add(reportSource);
        }

        public FileInfo ToFileInfo()
        {
            var lineInfos = new Dictionary<int, LineInfo.Builder>();
            foreach (var info in CoverageInfos.Values)
            {
                for (int line = info.Coverage.StartLine; line <= info.Coverage.EndLine; line++)
                {
                    if (!lineInfos.TryGetValue(line, out var lineInfo))
                    {
                        lineInfo = new LineInfo.Builder { StartLine = line };
                        lineInfos.Add(line, lineInfo);
                    }
                    lineInfo.CoverageInfos.Add(info);
                }
            }
            var sortedLineInfos = lineInfos.Values.OrderBy(li => li.StartLine).ToList();
            var mergedLineInfos = new List<LineInfo.Builder>(sortedLineInfos.Count);
            LineInfo.Builder? previousLineInfo = null;
            foreach (var lineInfo in sortedLineInfos)
            {
                if (previousLineInfo is null || !previousLineInfo.TryMerge(lineInfo))
                {
                    mergedLineInfos.Add(lineInfo);
                    previousLineInfo = lineInfo;
                }
            }
            return new FileInfo
            {
                Name = Name,
                Lines = [.. mergedLineInfos.Select(li => li.ToLineInfo())],
            };
        }
    }
}
