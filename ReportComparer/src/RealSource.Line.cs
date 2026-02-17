// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReportComparer;

internal sealed partial class RealSource
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    [DebuggerDisplay("{_rangeGroups}")]
    internal readonly struct Line
    {
        private readonly IReadOnlyCollection<(ParsedRange[] parsedRanges, ParsedReport[] reports)>? _rangeGroups;

        public int Number { get; }
        public IReadOnlyCollection<(ParsedRange[] parsedRanges, ParsedReport[] reports)> RangeGroups => _rangeGroups ?? [];
        public bool IsCoverable => _rangeGroups is not null;

        public bool HasOverlaps
        {
            get
            {
                if (_rangeGroups is null) return false;
                int n = Number;
                return _rangeGroups.Any(rg => ParsedRange.OverlapsAt(n, rg.parsedRanges));
            }
        }

        public bool HasColumns
        {
            get
            {
                if (_rangeGroups is null) return false;
                int n = Number;
                return _rangeGroups.Any(rg => rg.parsedRanges.Any(pr => pr.Range.StartColumn != 0));
            }
        }

        public Line(int number, IEnumerable<(ParsedRange[] parsedRanges, ParsedReport[] reports)> rangeGroups, int maxReportCount)
        {
            Number = number;
            using var e = rangeGroups.GetEnumerator();
            bool hasNext = e.MoveNext();
            Debug.Assert(hasNext);
            if (!hasNext) return;
            var (parsedRanges, reports) = e.Current;
            hasNext = e.MoveNext();
            if (!hasNext && parsedRanges.Length == 0)
            {
                if (reports.Length == maxReportCount) return;
            }
            _rangeGroups = rangeGroups.ToArray();
        }
    }
}
