// Copyright (c) Matthias Wolf, Mawosoft.

using System;

namespace ReportComparer;

internal sealed record ReportedMetrics
{
    public const int Count = 12;
    public double? BlockCoverage { get; set; } // DynamicCoverage
    public double? LineCoverage { get; set; } // DynamicCoverage, Cobertura. For Cobertura, this is line_rate.
    public double? BranchRate { get; set; } // Cobertura
    public int? Complexity { get; set; } // Cobertura
    public int? BlocksCovered { get; set; } // DynamicCoverage
    public int? BlocksNotCovered { get; set; } // DynamicCoverage
    public int? LinesCovered { get; set; } // DynamicCoverage. For Cobertura, only on root element.
    public int? LinesPartiallyCovered { get; set; } // DynamicCoverage
    public int? LinesNotCovered { get; set; } // DynamicCoverage
    public int? LinesValid { get; set; } // Cobertura, only on root.
    public int? BranchesCovered { get; set; } // Cobertura, only on root.
    public int? BranchesValid { get; set; } // Cobertura, only on root.

    public double? this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 11);
            if (index < 3)
            {
                return index switch
                {
                    0 => BlockCoverage,
                    1 => LineCoverage,
                    _ => BranchRate,
                };
            }
            else
            {
                int? v = index switch
                {
                    3 => Complexity,
                    4 => BlocksCovered,
                    5 => BlocksNotCovered,
                    6 => LinesCovered,
                    7 => LinesPartiallyCovered,
                    8 => LinesNotCovered,
                    9 => LinesValid,
                    10 => BranchesCovered,
                    _ => BranchesValid,
                };
                return v;
            }
        }
    }
}
