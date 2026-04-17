// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{Name} in {ParentType}")]
internal sealed class ParsedMethod : IEquatable<ParsedMethod>
{
    private List<ParsedRange> _ranges = [];
    private bool _frozen;

    public ParsedType ParentType { get; }
    public string Name { get; }
    public ReportedMetrics? ReportedMetrics { get; }
    public ParsedSource ParentSource { get; private set; } = null!;
    public Range BoundingRange { get; private set; }
    public bool HasMultipleSources { get; private set; }
    public RealMethod RealMethod { get; internal set; } = null!;

    public string OriginalName { get; }
    public string MethodName { get; }
    public string ParameterSignature { get; }
    public int ParameterCount { get; }
    public int GenericParameterCount { get; }

    public RealSource RealSource => ParentSource.RealSource;
    public RealType RealType => ParentType.RealType;
    public ParsedReport ParentReport => ParentAssembly.ParentReport;
    public ParsedAssembly ParentAssembly => ParentType.ParentAssembly;

    public ParsedMethod(ParsedType parentType, string name, ReportedMetrics? reportedMetrics)
    {
        ParentType = parentType;
        OriginalName = name;
        int pos = name.IndexOf('(');
        if (pos > 0 && name[pos - 1] == '>')
        {
            pos = SymbolNameHelper.LastIndexOfBalancedBraces(name.AsSpan(0, pos));
        }
        if (pos > 0)
        {
            MethodName = name[..pos];
            name = string.Concat(MethodName, SymbolNameHelper.NormalizeCommaSpacing(name[pos..]));
            if (name[pos] == '<')
            {
                int end = name.IndexOf('(', pos + 1);
                GenericParameterCount = SymbolNameHelper.CountParameters(name.AsSpan((pos + 1)..(end - 1)));
                pos = end;
            }
            ParameterSignature = name[pos..];
            ParameterCount = SymbolNameHelper.CountParameters(ParameterSignature.AsSpan(1..^1));
        }
        else
        {
            MethodName = name;
            ParameterSignature = "";
        }
        Name = name;
        ReportedMetrics = reportedMetrics;
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_ranges.Count != 0);
        Debug.Assert(_ranges.All(r => r.RealSource.Order != 0));
        _ranges = _ranges.OrderBy(r => r.RealSource.Order).ThenBy(r => r.Range).ThenBy(r => r.Status).ToList();
        if (_ranges.Count != 0)
        {
            ParentSource = _ranges[0].ParentSource;
            BoundingRange = _ranges[0].Range;
            for (int i = 1; i < _ranges.Count; i++)
            {
                if (!ReferenceEquals(_ranges[i].ParentSource, ParentSource))
                {
                    HasMultipleSources = true;
                    break;
                }
                BoundingRange = BoundingRange.GetBoundingRange(_ranges[i].Range);
            }
            ParentSource.AddMethod(this);
        }
        _frozen = true;
    }

    public ParsedRange AddRange(
        ParsedSource source,
        Range range,
        CoverageStatus status,
        CoberturaCoverage? coberturaCoverage)
    {
        Debug.Assert(!_frozen);
        var parsedRange = new ParsedRange(source, range, status, coberturaCoverage);
        _ranges.Add(parsedRange);
        source.AddRange(parsedRange);
        return parsedRange;
    }

    public bool Equals([NotNullWhen(true)] ParsedMethod? other)
    {
        Debug.Assert(_frozen);
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        if (Name != other.Name) return false;
        if (ReportedMetrics != other.ReportedMetrics) return false;
        return _ranges.SequenceEqual(other._ranges);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ParsedMethod);

    public override int GetHashCode()
    {
        Debug.Assert(_frozen);
        return HashCode.Combine(
            Name,
            EquatableSequence.Create(_ranges),
            ReportedMetrics);
    }

    public Match MatchTo(ParsedMethod other)
    {
        Debug.Assert(_frozen);
        return new(this, other);
    }

    internal readonly record struct Match : IComparable<Match>
    {
        public long MatchSize { get; }
        public int ParameterCountDistance { get; }
        public int NameDistance { get; }
        public int SignatureDistance { get; }
        public int RealTypeDistance { get; }

        public Match(ParsedMethod left, ParsedMethod right)
        {
            Debug.Assert(left.RealSource == right.RealSource);
            if (left.RealSource != right.RealSource) return;
            MatchSize = left.BoundingRange.Intersect(right.BoundingRange).VirtualSize;
            if (MatchSize == 0) return;
            // Store these as negative values so bigger is better.
            ParameterCountDistance = -Math.Abs(left.ParameterCount - right.ParameterCount);
            NameDistance = -SymbolNameHelper.LevenshteinDistance(left.MethodName, right.MethodName);
            SignatureDistance = -SymbolNameHelper.LevenshteinDistance(left.ParameterSignature, right.ParameterSignature);
            RealTypeDistance = -SymbolNameHelper.LevenshteinDistance(left.RealType.Name, right.RealType.Name);
        }

        public int CompareTo(Match other)
        {
            int r = MatchSize.CompareTo(other.MatchSize);
            if (r != 0) return r;
            r = ParameterCountDistance.CompareTo(other.ParameterCountDistance);
            if (r != 0) return r;
            r = NameDistance.CompareTo(other.NameDistance);
            if (r != 0) return r;
            r = SignatureDistance.CompareTo(other.SignatureDistance);
            if (r != 0) return r;
            return RealTypeDistance.CompareTo(other.RealTypeDistance);
        }

        public static bool operator <(Match left, Match right) => left.CompareTo(right) < 0;
        public static bool operator <=(Match left, Match right) => left.CompareTo(right) <= 0;
        public static bool operator >(Match left, Match right) => left.CompareTo(right) > 0;
        public static bool operator >=(Match left, Match right) => left.CompareTo(right) >= 0;
    }
}
