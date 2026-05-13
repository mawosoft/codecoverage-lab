// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Runtime.CompilerServices;

namespace CoverageDemo.Naming;

public class Closures
{
    public int Method1(MyInt value)
    {
        return LocalStatic(value);

        static int LocalStatic(MyInt v) => v;
    }

    public int Method2(MyInt value)
    {
        return CallLambda(v => v, value);
    }

    public int Method3(MyInt value)
    {
        return LocalInstance(value);

        int LocalInstance(MyInt v) => this is null ? 0 : v;
    }

    public int Method4(MyInt value)
    {
        return CallLambda(v => this is null ? 0 : v, value);
    }

    public int Method5(MyInt value)
    {
        int foo = value - 1;
        return LocalClosure(value);

        int LocalClosure(MyInt v) => foo < 0 ? 0 : v;
    }

    public int Method6(MyInt value)
    {
        int foo = value - 1;
        return CallLambda(v => foo < 0 ? 0 : v, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int CallLambda(Func<MyInt, MyInt> lambda, MyInt arg)
    {
        return lambda(arg);
    }
}
