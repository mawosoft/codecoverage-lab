// Copyright (c) Matthias Wolf, Mawosoft.

namespace ReportComparer;

internal sealed record ReportedMetrics
{
    public double? BlockCoverage { get; set; } // DynamicCoverage
    public double? LineCoverage { get; set; } // DynamicCoverage, Cobertura. For Cobertura, this is line_rate.
    public double? BranchRate { get; set; } // Cobertura
    public int? Complexity { get; set; } // Cobertura
    public int? BlocksCovered { get; set; } // DynamicCoverage
    public int? BlocksNotCovered { get; set; } // DynamicCoverage
    public int? LinesCovered { get; set; } // DynamicCoverage, Cobertura. For Cobertura, only on <coverage>
    public int? LinesPartiallyCovered { get; set; } // DynamicCoverage
    public int? LinesNotCovered { get; set; } // DynamicCoverage
}
