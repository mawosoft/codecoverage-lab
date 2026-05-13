// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CoverageDemo.Coverage;

// Lambda functions coverage.

public class Lambdas
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new Lambdas();
            _ = c.LambdaFull(0);
            _ = c.LambdaFull(-1);
            _ = c.LambdaPartial(0);
        }
    }

    public int LambdaFull(MyInt value)
    {
        int result1 = CallLambda(static (v) => 2 * v, value);
        int result2 = CallLambda(static (v) =>
            {
                return 3 * v;
            },
            value);
        int result3 = CallLambda(static (v) => v >= 0 ? 2 * v : 3 * v, value);
        int result4 = CallLambda(static (v) =>
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
        return result1 + result2 + result3 + result4;
    }

    public int LambdaPartial(MyInt value)
    {
        // Expected: no hit on else branch.
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
    private static int CallLambda(Func<MyInt, MyInt> lambda, MyInt arg)
    {
        return lambda(arg);
    }
}
