// Copyright (c) Matthias Wolf, Mawosoft.

using Xunit;

namespace FooLib;

public class CoverageDemoTests
{
    [Fact]
    public void Test1()
    {
        for (int hits = 0; hits < 5; hits++)
        {
            var demo = new CoverageDemo();
            demo.NoBranch(0);
            demo.SingleBranchFullCoverage(0);
            demo.SingleBranchFullCoverage(-1);
            demo.SingleBranchPartialCoverage(0);
            demo.SingleBranchSingleLineFullCoverage(0);
            demo.SingleBranchSingleLineFullCoverage(-1);
            demo.SingleBranchSingleinePartialCoverage(0);
        }
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 0)]
    public void Test2(int value, int autoProp1)
    {
        for (int hits = 0; hits < 5; hits++)
        {
            var demo = new CoverageDemo { AutoProp1 = autoProp1 };
            demo.MultiBranchFullCoverage(value);
            demo.MultiBranchSingleLineFullCoverage(value);
            if (value != 0)
            {
                demo.MultiBranchPartialCoverage(value);
                demo.MultiBranchSingleLinePartialCoverage(value);
            }
        }
    }

    [Fact]
    public void Test3()
    {
        for (int hits = 0; hits < 5; hits++)
        {
            var demo = new CoverageDemo();
            for (int i = 0; i <= 10; i++)
            {
                demo.SwitchStatementFullCoverage(i);
                demo.SwitchExpressionFullCoverage(i);
                if ((i & 1) == 0)
                {
                    demo.SwitchStatementPartialCoverage(i);
                    demo.SwitchExpressionPartialCoverage(i);
                }
            }
        }
    }

#pragma warning disable CA1031 // Do not catch general exception types
    [Fact]
    public void Test4()
    {
        for (int hits = 0; hits < 5; hits++)
        {
            var demo = new CoverageDemo();
            try { demo.UnreachableCode(0); } catch { }
            try { demo.UnreachableCode(-1); } catch { }
            try { demo.UnreachableCodeIf(0); } catch { }
            try { demo.UnreachableCodeIf(0); } catch { }
        }
    }
#pragma warning restore CA1031 // Do not catch general exception types
}
