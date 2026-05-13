// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CoverageDemo.Coverage;

// Loop coverage.

public class LoopConditions
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new LoopConditions();
            var empty = new MyIntEnumerable([]);
            var five = new MyIntEnumerable([0, 1, 2, 3, 4]);
            try { c.ForLoop(0, 5); } catch { }
            try { c.ForeachLoop(empty, five); } catch { }
            try { c.AsyncForeachLoop(empty, five).GetAwaiter().GetResult(); } catch { }
            try { c.WhileLoop(0, 5); } catch { }
            try { c.DoWhileLoop(5); } catch { }
        }
    }

    public void ForLoop(MyInt zero, MyInt value)
    {
        // Skip loop
        for (int i = 0; i < zero; i++)
        {
            ThrowIf(i >= 10);
        }

        // Complete loop
        for (int i = 0; i < value; i++)
        {
            ThrowIf(i >= 10);
        }

        // Break loop before completion
        for (int i = 0; i < value; i++)
        {
            ThrowIf(i >= value - 2);
        }
    }

    public void ForeachLoop(MyIntEnumerable empty, MyIntEnumerable values)
    {
        // Skip loop
        foreach (var v in empty)
        {
            ThrowIf(v >= 10);
        }

        // Complete loop
        foreach (var v in values)
        {
            ThrowIf(v >= 10);
        }

        // Break loop before completion
        foreach (var v in values)
        {
            ThrowIf(v >= 3);
        }
    }

    public async Task AsyncForeachLoop(MyIntEnumerable empty, MyIntEnumerable values)
    {
        // Skip loop
        await foreach (var v in empty.ConfigureAwait(false))
        {
            ThrowIf(v >= 10);
        }

        // Complete loop
        await foreach (var v in values.ConfigureAwait(false))
        {
            ThrowIf(v >= 10);
        }

        // Break loop before completion
        await foreach (var v in values.ConfigureAwait(false))
        {
            ThrowIf(v >= 3);
        }
    }

    public void WhileLoop(MyInt zero, MyInt value)
    {
        // Skip loop
        int i = 0;
        while (i < zero)
        {
            ThrowIf(i >= 10);
            i++;
        }

        // Complete loop
        i = 0;
        while (i < value)
        {
            ThrowIf(i >= 10);
            i++;
        }

        // Break loop before completion
        i = 0;
        while (i < value)
        {
            ThrowIf(i >= value - 2);
            i++;
        }
    }

    public void DoWhileLoop(MyInt value)
    {
        // Complete loop
        int i = -1;
        do
        {
            i++;
            ThrowIf(i >= 10);
        } while (i < value);

        // Break loop before completion
        i = -1;
        do
        {
            i++;
            ThrowIf(i >= value - 2);
        } while (i < value);
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ThrowIf(MyBool condition)
    {
        if (condition) throw new InvalidOperationException();
    }
}
