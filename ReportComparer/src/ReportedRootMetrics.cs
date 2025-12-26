// Copyright (c) Matthias Wolf, Mawosoft.

namespace ReportComparer;

// Cobertura only
internal sealed record ReportedRootMetrics
{
    public double? LineRate { get; set; }
    public double? BranchRate { get; set; }
    public int? Complexity { get; set; }
    public int? LinesCovered { get; set; }
    public int? LinesValid { get; set; }
    public int? BranchesCovered { get; set; }
    public int? BranchesValid { get; set; }
}
