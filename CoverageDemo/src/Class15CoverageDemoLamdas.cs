// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Runtime.CompilerServices;

namespace CoverageDemo;

#pragma warning disable CA1822 // Mark members as static
public class Class15CoverageDemoLamdas
{
    public int Method01LambdaFullCoverage(int value)
    {
        int result1 = CallLambda(static (v) => 2 * v, value);
        int result2 = CallLambda(static (v) =>
        {
            return 3 * v;
        },
        value);
        return result1 + result2;
    }

    public int Method02LambdaPartialCoverage(int value)
    {
        // Expected coverage: nohit on else branch.
        int result1 = CallLambda(static (v) => v >= 0 ? 2 * v : 3 * v, value);
        int result2 = CallLambda(static (v) =>
        {
            if (v >= 0)
            {
                return 3 * v;
            }
            else
            {
                return 4 * v;
            }
        },
        value);
        return result1 + result2;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int CallLambda(Func<int, int> lambda, int arg)
    {
        return lambda(arg);
    }
}
