// Copyright (c) Matthias Wolf, Mawosoft.

using System.Diagnostics.CodeAnalysis;

namespace CoverageDemo.Coverage;

// Generic instantiation coverage.

public class Generics
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new Generics();
            _ = c.TypeOptimized(0);
            _ = c.MergedCoverageFromDifferentInstantiations(0, 0, true);
            _ = c.MergedCoverageFromDifferentInstantiations("", "", false);
        }
    }

    public int TypeOptimized<TItem>(TItem _)
    {
        // Only the <int> version of this method is used and its optimized body just
        // contains the "return 1" statement.
        // However, with coverage instrumentation, the hit-logging code remains,
        // even if the statement it is supposed track isn't present.
        // The instrumented and optimized code therefore looks something like this:
        //   LogHit_For_IntComparison();
        //   LogHit_For_Return1();
        //   return 1;

        if (typeof(TItem) == typeof(int))
        {
            return 1;
        }
        if (typeof(TItem) == typeof(bool))
        {
            return 2;
        }
        return -1;
    }

    public TItem MergedCoverageFromDifferentInstantiations<TItem>(TItem value1, TItem value2, MyBool condition)
    {
        if (condition)
        {
            // Only hit in <int> version of this method.
            return value1;
        }
        else
        {
            // Only hit in <string> version of this method.
            return value2;
        }
    }
}
