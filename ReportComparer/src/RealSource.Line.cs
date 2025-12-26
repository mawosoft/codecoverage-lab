// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReportComparer;

internal sealed partial class RealSource
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    internal readonly struct Line
#pragma warning restore CA1815 // Override equals and operator equals on value types
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

        public Line(int number, IEnumerable<(IReadOnlyCollection<ParsedRange>? parsedRanges, IEnumerable<ParsedReport> reports)> rangeGroups)
        {
            Number = number;
            using var e = rangeGroups.GetEnumerator();
            bool hasNext = e.MoveNext();
            Debug.Assert(hasNext);
            if (!hasNext) return;
            var (parsedRanges, reports) = e.Current;
            hasNext = e.MoveNext();
            if (!hasNext && (parsedRanges is null || parsedRanges.Count == 0))
            {
                if (reports.First().ReportComparison.Reports.Count == reports.Count()) return;
            }
            _rangeGroups = rangeGroups.Select(vt => (vt.parsedRanges?.ToArray() ?? [], vt.reports.ToArray())).ToArray();
        }
    }
}
