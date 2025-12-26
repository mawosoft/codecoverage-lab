// Copyright (c) Matthias Wolf, Mawosoft.

using System.Diagnostics;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{Hits}, {CoveredBranches}/{TotalBranches}")]
internal sealed record CoberturaCoverage
{
    public required int Hits { get; init; }
    public int CoveredBranches { get; init; }
    public int TotalBranches { get; init; }
    public EquatableSequence<double> Conditions { get; init; }
}
