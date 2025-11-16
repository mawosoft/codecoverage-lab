// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics.CodeAnalysis;

namespace CoverageDemo;

public class Class11CoverageDemoBasics
{
    private readonly int _someField;

    public Class11CoverageDemoBasics(int value = 0) => _someField = value;

    public int Method01NoBranch(int value)
    {
        return value - _someField;
    }

    public int Method02UnreachableCode(int value)
    {
        if (value < 0)
        {
            ThrowSomething();
            return value + _someField;
        }
        else
        {
            return value - _someField;
        }
    }

    public int Method03UnreachableCodeIf(int value)
    {
        ThrowSomethingIf(value < 0);
        return value + _someField;
    }

    [DoesNotReturn]
    private static void ThrowSomething()
    {
        throw new InvalidOperationException();
    }

    private static void ThrowSomethingIf([DoesNotReturnIf(true)] bool condition)
    {
        if (condition)
        {
            throw new InvalidOperationException();
        }
    }
}
