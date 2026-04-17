// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

#pragma warning disable CA1822 // Mark members as static
public class Class16CoverageDemoGenerics
{
    public TItem Method01GenericsFullCoverage<TItem>(TItem value1, TItem value2, bool condition)
    {
        if (condition)
        {
            return value1;
        }
        else
        {
            return value2;
        }
    }

    public TItem Method02GenericsFullCoverageViaDifferentTypeParam<TItem>(TItem value1, TItem value2, bool condition)
    {
        if (condition)
        {
            return value1;
        }
        else
        {
            return value2;
        }
    }

    public TItem Method03GenericsTypeOptimizationApplied<TItem>(TItem value1, TItem value2)
    {
        if (typeof(TItem) == typeof(int))
        {
            return value1;
        }
        return value2; // Expected: nohit
    }

    public TItem Method04GenericsTypeOptimizationNotApplied<TItem>(TItem value1, TItem value2)
    {
        if (typeof(TItem) == typeof(int))
        {
            return value1; // Expected: nohit
        }
        return value2;
    }

    public TItem Method05GenericsTypeOptimizationBoth<TItem>(TItem value1, TItem value2)
    {
        if (typeof(TItem) == typeof(int))
        {
            return value1;
        }
        return value2;
    }
}
