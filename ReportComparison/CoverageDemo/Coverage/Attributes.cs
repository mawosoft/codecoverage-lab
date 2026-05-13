// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CoverageDemo.Coverage;

// Coverage exclusion via attributes.

// - MSCC
//   Some attributes trigger exclusions by default.
//   The DoesNotReturnAttribute is hardcoded and cannot be configured. Only its presence
//   in the same assembly is checked by default, but this is configurable.
// - coverlet (non-MTP)
//   Only attributes explicitly specified in the configuration trigger exclusions.
//   The DoesNotReturnAttribute must be explicitly specified.
// - coverlet (MTP)
//   Some attributes trigger exclusions by default.
//   The DoesNotReturnAttribute must be explicitly specified.

public class Attributes
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new Attributes();
            try { c.CallNonReturningMethod(); } catch { }
            try { c.CallExternalNonReturningMethod(); } catch { }
            try { c.CallMaybeNonReturningMethod(); } catch { }
            c.DebuggerHiddenMethod();
            c.DebuggerNonUserCodeMethod();
            c.DebuggerStepperBoundaryMethod();
            c.CompilerGeneratedMethod();
            c.GeneratedCodeMethod();
            c.ExcludeFromCodeCoverageMethod();
        }
    }

    public int CallNonReturningMethod()
    {
        DoesNotReturnMethod();
        return 0;
    }

    public int CallExternalNonReturningMethod()
    {
        Trace.Fail("");
        return 0;
    }

    public int CallMaybeNonReturningMethod()
    {
        DoesNotReturnIfMethod(false);
        DoesNotReturnIfMethod(true);
        return 0;
    }

    [DoesNotReturn]
    public void DoesNotReturnMethod()
    {
        throw new InvalidOperationException();
    }

    public void DoesNotReturnIfMethod([DoesNotReturnIf(true)] MyBool condition)
    {
        if (condition)
        {
            throw new InvalidOperationException();
        }
    }

    [DebuggerHidden] public void DebuggerHiddenMethod() { }
    [DebuggerNonUserCode] public void DebuggerNonUserCodeMethod() { }
    [DebuggerStepperBoundary] public void DebuggerStepperBoundaryMethod() { }
    [CompilerGenerated] public void CompilerGeneratedMethod() { }
    [GeneratedCode(null, null)] public void GeneratedCodeMethod() { }
    [ExcludeFromCodeCoverage] public void ExcludeFromCodeCoverageMethod() { }
}
