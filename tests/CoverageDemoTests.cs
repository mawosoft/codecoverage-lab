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
            var demo = new Class10CoverageDemo()
            {
                AutoProp07InitSingleLineFullCoverage = 1,
                AutoProp08InitSingleLineInitCoverage = 1,
                AutoProp09InitMultiLineFullCoverage = 1,
                AutoProp10InitMultiLineInitCoverage = 1,
            };
            int v = demo.AutoProp07InitSingleLineFullCoverage;
            v += demo.AutoProp09InitMultiLineFullCoverage;
            demo.AutoProp01SingleLineFullCoverage = v;
            v = demo.AutoProp01SingleLineFullCoverage;
            v += demo.AutoProp02SingleLineGetCoverage;
            demo.AutoProp03SingleLineSetCoverage = v;
            demo.AutoProp04MultiLineFullCoverage = 1;
            v = demo.AutoProp04MultiLineFullCoverage;
            v += demo.AutoProp05MultiLineGetCoverage;
            demo.AutoProp06MultiLineSetCoverage = v;

            demo.FullProp1SingleLineFullCoverage = 1;
            v = demo.FullProp1SingleLineFullCoverage;
            v += demo.FullProp2SingleLineGetCoverage;
            demo.FullProp3SingleLineSetCoverage = v;
            demo.FullProp4MultiLineFullCoverage = 1;
            v = demo.FullProp4MultiLineFullCoverage;
            v += demo.FullProp5MultiLineGetCoverage;
            demo.FullProp6MultiLineSetCoverage = v;

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
    public void Test2(int value1, int value2)
    {
        for (int hits = 0; hits < 5; hits++)
        {
            var demo = new Class10CoverageDemo();
            demo.MultiBranchFullCoverage(value1, value2);
            demo.MultiBranchSingleLineFullCoverage(value1, value2);
            if (value1 != 0)
            {
                demo.MultiBranchPartialCoverage(value1, value2);
                demo.MultiBranchSingleLinePartialCoverage(value1, value2);
            }
        }
    }

    [Fact]
    public void Test3()
    {
        for (int hits = 0; hits < 5; hits++)
        {
            var demo = new Class10CoverageDemo();
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
            var demo = new Class10CoverageDemo();
            try { demo.UnreachableCode(0); } catch { }
            try { demo.UnreachableCode(-1); } catch { }
            try { demo.UnreachableCodeIf(0); } catch { }
            try { demo.UnreachableCodeIf(-1); } catch { }
        }
    }
#pragma warning restore CA1031 // Do not catch general exception types
}
