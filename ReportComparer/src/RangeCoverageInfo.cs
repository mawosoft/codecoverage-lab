// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ReportComparer;

internal sealed class RangeCoverageInfo
{
    public required RangeCoverage Coverage { get; set; }
    public required List<string> Containers { get; set; }
    public required List<string> ReportSources { get; set; }

    internal sealed class Builder : IEquatable<Builder>
    {
        public RangeCoverage.Builder Coverage { get; set; }
        public HashSet<string> Containers { get; set; } = [];
        public HashSet<string> ReportSources { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Builder(RangeCoverage.Builder coverage) => Coverage = coverage;

        public Builder? WithLines(int startLine, int endLine)
        {
            var coverage = Coverage.WithLines(startLine, endLine);
            if (coverage is null) return null;
            return new Builder(coverage)
            {
                Containers = Containers,
                ReportSources = ReportSources,
            };
        }

        public RangeCoverageInfo ToRangeCoverageInfo() => new()
        {
            Coverage = Coverage.ToRangeCoverage(),
            Containers = [.. Containers.Order()],
            ReportSources = [.. ReportSources.Order()],
        };

        public bool Equals(Builder? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!Coverage.Equals(other.Coverage)) return false;
            if (!Containers.SetEquals(other.Containers)) return false;
            if (!ReportSources.SetEquals(other.ReportSources)) return false;
            return true;
        }

        public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as Builder);
        public override int GetHashCode() => HashCode.Combine(Coverage, Containers.Count, ReportSources.Count);
    }
}
