// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Runtime.CompilerServices;

namespace CoverageDemo;

#pragma warning disable CA1822 // Mark members as static
public class Class15CoverageDemoLamdas
{
    public int Method01LambdaFullCoverage(int value)
    {
        int result = CallLambda(static (v) =>
        {
            return v;
        },
        value);
        return result;
    }

    public int Method02LambdaPartialCoverage(int value)
    {
        int result = CallLambda(static (v) =>
        {
            if (v >= 0)
            {
                return v;
            }
            else
            {
                return 2 * v;
            }
        },
        value);
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int CallLambda(Func<int, int> lamda, int arg)
    {
        return lamda(arg);
    }
}
