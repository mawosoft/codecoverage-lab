// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ReportComparer;

#pragma warning disable CA1036 // Override methods on comparable types
[DebuggerDisplay("{StartLine},{StartColumn} - {EndLine},{EndColumn}")]
internal readonly record struct Range : IComparable<Range>
{
    public int StartLine { get; } // 1..n
    public int StartColumn { get; } // 1..n; 0: entire line
    public int EndLine { get; } // 1..n inclusive
    public int EndColumn { get; } // 1..n exclusive; 0: entire line

    public long Start => ((long)StartLine << 32) + StartColumn;
    public long End => ((long)EndLine << 32) + (EndColumn != 0 ? EndColumn : int.MaxValue);
    public long VirtualSize => IsValid() ? End - Start + (StartColumn == 0 ? 0 : 1) : 0;

    public Range(int startLine) : this(startLine, 0, startLine, 0) { }

    public Range(int startLine, int startColumn, int endLine, int endColumn)
    {
        StartLine = startLine;
        StartColumn = startColumn;
        EndLine = endLine;
        EndColumn = endColumn;
    }

    public Range(long start, long end)
    {
        StartLine = (int)(start >>> 32);
        int sc = (int)start;
        StartColumn = sc != int.MaxValue ? sc : 0;
        EndLine = (int)(end >>> 32);
        int ec = (int)end;
        EndColumn = ec != int.MaxValue ? ec : 0;
    }

    public bool IsValid()
    {
        if (StartLine <= 0 || EndLine < StartLine) return false;
        if (StartColumn < 0 || EndColumn < 0) return false;
        if (StartLine != EndLine) return true;
        return EndColumn > StartColumn || EndColumn == 0;
    }

    public bool Overlaps(int lineNumber) => lineNumber >= StartLine && lineNumber <= EndLine;

    public bool Overlaps(Range other) => Start < other.End && End > other.Start;

    //public bool Overlaps(Range other)
    //{
    //    if (StartLine > other.EndLine || other.StartLine > EndLine) return false;
    //    return (StartLine < other.EndLine
    //            || StartColumn < other.EndColumn
    //            || StartColumn == 0
    //            || other.EndColumn == 0)
    //        && (other.StartLine < EndLine
    //            || other.StartColumn < EndColumn
    //            || other.StartColumn == 0
    //            || EndColumn == 0);
    //}

    public Range Intersect(Range other)
    {
        long start = Math.Max(Start, other.Start);
        long end = Math.Min(End, other.End);
        if (start >= end) return default;
        return new Range(start, end);
    }

    public int CompareTo(Range other)
    {
        if (StartLine < other.StartLine) return -1;
        if (StartLine > other.StartLine) return 1;
        if (StartColumn < other.StartColumn) return -1;
        if (StartColumn > other.StartColumn) return 1;
        // We want smaller ranges sorted after enclosing larger range.
        if (EndLine < other.EndLine) return 1;
        if (EndLine > other.EndLine) return -1;
        if (EndColumn < other.EndColumn) return 1;
        return 0;
    }

    public static Range GetBoundingRange(IEnumerable<Range> ranges)
    {
        long start = 0, end = 0;
        using var e = ranges.GetEnumerator();
        if (e.MoveNext())
        {
            var range = e.Current;
            start = range.Start;
            end = range.End;
            while (e.MoveNext())
            {
                range = e.Current;
                start = Math.Min(start, range.Start);
                end = Math.Max(end, range.End);
            }
        }
        return new Range(start, end);
    }

    //public static Range GetBoundingRange(IEnumerable<Range> ranges)
    //{
    //    int startLine = 0, startColumn = 0, endLine = 0, endColumn = 0;
    //    using var e = ranges.GetEnumerator();
    //    if (e.MoveNext())
    //    {
    //        var range = e.Current;
    //        startLine = range.StartLine;
    //        startColumn = range.StartColumn;
    //        endLine = range.EndLine;
    //        endColumn = range.EndColumn;
    //        while (e.MoveNext())
    //        {
    //            range = e.Current;
    //            if (range.StartLine < startLine)
    //            {
    //                startLine = range.StartLine;
    //                startColumn = range.StartColumn;
    //            }
    //            else if (startColumn != 0 && range.StartLine == startLine && range.StartColumn < startColumn)
    //            {
    //                startColumn = range.StartColumn;
    //            }
    //            if (range.EndLine > endLine)
    //            {
    //                endLine = range.EndLine;
    //                endColumn = range.EndColumn;
    //            }
    //            else if (endColumn != 0
    //                     && range.EndLine == endLine
    //                     && (range.EndColumn > endColumn || range.EndColumn == 0))
    //            {
    //                endColumn = range.EndColumn;
    //            }
    //        }
    //    }
    //    return new Range(startLine, startColumn, endLine, endColumn);
    //}
}
