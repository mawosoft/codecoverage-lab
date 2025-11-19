// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportComparer;

internal sealed class LineInfo
{
    public required int StartLine { get; set; }
    public int? EndLine { get; set; }
    public required List<RangeCoverageInfo> CoverageInfos { get; set; }

    internal sealed class Builder
    {
        public required int StartLine { get; set; }
        public int EndLine { get => field == 0 ? StartLine : field; set; }
        public HashSet<RangeCoverageInfo.Builder> CoverageInfos { get; set; } = [];

        public LineInfo ToLineInfo() => new()
        {
            StartLine = StartLine,
            EndLine = EndLine == StartLine ? null : EndLine,
            CoverageInfos = [.. CoverageInfos.Select(b => b.ToRangeCoverageInfo())], // TODO Order?
        };

        public bool TryMerge(Builder other)
        {
            if (other.StartLine <= EndLine) throw new InvalidOperationException();
            if (other.StartLine > EndLine + 1) return false;
            int minStartLine = other.CoverageInfos.Min(ci => ci.Coverage.StartLine);
            int maxEndLine = other.CoverageInfos.Max(ci => ci.Coverage.EndLine);
            var coverageInfos = new HashSet<RangeCoverageInfo.Builder>(CoverageInfos.Count);
            foreach (var coverageInfo in CoverageInfos)
            {
                var clone = coverageInfo.WithLines(minStartLine, maxEndLine);
                if (clone is null) return false;
                coverageInfos.Add(clone);
            }
            if (!coverageInfos.SetEquals(other.CoverageInfos)) return false;
            EndLine = other.StartLine;
            return true;
        }
    }
}
