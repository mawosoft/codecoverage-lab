// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;

namespace ReportComparer;

internal sealed class RangeCoverage
{
    public required CoverageStatus Status { get; set; }
    public required int StartLine { get; set; }
    public int? StartColumn { get; set; }
    public int? EndLine { get; set; }
    public int? EndColumn { get; set; }
    public int? Hits { get; set; }
    public int? CoveredBranches { get; set; }
    public int? TotalBranches { get; set; }
    public List<double>? Conditions { get; set; }

    internal sealed record Builder
    {
        public required CoverageStatus Status { get; set; }
        public required int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get => field == 0 ? StartLine : field; set; }
        public int EndColumn { get; set; }
        public int Hits { get; set; } = -1;
        public int CoveredBranches { get; set; } = -1;
        public int TotalBranches { get; set; } = -1;
        public EquatableSequence<double> Conditions { get; set; }

        public bool IsInRange(int line) => line >= StartLine && line <= EndLine;

        public Builder? WithLines(int startLine, int endLine)
        {
            if (startLine == endLine)
            {
                if (StartLine != EndLine) return null;
            }
            else if (endLine != EndLine)
            {
                return null;
            }
            return this with { StartLine = startLine };
        }

        public RangeCoverage ToRangeCoverage() => new()
        {
            Status = Status,
            StartLine = StartLine,
            StartColumn = StartColumn == 0 ? null : StartColumn,
            EndLine = EndLine == StartLine ? null : EndLine,
            EndColumn = EndColumn == 0 ? null : EndColumn,
            Hits = Hits < 0 ? null : Hits,
            CoveredBranches = CoveredBranches < 0 ? null : CoveredBranches,
            TotalBranches = TotalBranches < 0 ? null : TotalBranches,
            Conditions = Conditions.Value?.Count > 0 ? [.. Conditions.Value] : null,
        };
    }
}
