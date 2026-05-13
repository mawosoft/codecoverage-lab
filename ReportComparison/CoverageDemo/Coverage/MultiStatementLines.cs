// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CoverageDemo.Coverage;

// Block vs. line coverage with multiple statements on same line.
// See also Properties.cs, Lambdas.cs, and *Conditions.cs.

public class MultiStatementLines
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new MultiStatementLines();
            c.FirstOnly1();
            c.SecondOnly2();
            try { c.SeparateStatementsPartial(); } catch { }
            try { c.ChainedCallsPartial(); } catch { }
            try { c.ChainedCallsPartialMultiline(); } catch { }
        }
    }

    public void SeparateStatementsPartial()
    {
        SimpleMethod(); ThrowIf(true); SimpleMethod();
    }

    public void ChainedCallsPartial()
    {
        ChainedMethod().ChainedThrowIf(true).ChainedMethod();
    }

    public void ChainedCallsPartialMultiline()
    {
        ChainedMethod()
            .ChainedThrowIf(true)
            .ChainedMethod();
    }

    public void SimpleMethod() { }
    public MultiStatementLines ChainedMethod() => this;

#pragma warning disable format
    // This matches Property { get; set; }
    public void FirstOnly1() { } public void FirstOnly2() { }
    public void SecondOnly1() { } public void SecondOnly2() { }
#pragma warning restore format

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ThrowIf(MyBool condition)
    {
        if (condition) throw new InvalidOperationException();
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public MultiStatementLines ChainedThrowIf(MyBool condition)
    {
        if (condition) throw new InvalidOperationException();
        return this;
    }
}
