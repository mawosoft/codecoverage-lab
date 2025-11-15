// Copyright (c) Matthias Wolf, Mawosoft.

using System.Linq;
using System.Threading.Tasks;

namespace CoverageDemo;

public class CoverageDemoTests
{
    private const int HitCount = 5;

    [Fact]
    public void Test1()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class13CoverageDemoProperties()
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
        }
    }

    [Fact]
    public void Test2()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class11CoverageDemo();
            demo.Method01NoBranch(0);
#pragma warning disable format
            int[] p1 = [10, 10, 0];
            int[] p2 = [ 0, 10, 0];
#pragma warning restore format
            demo.Method02SingleConditionMultiLineBranches(p1);
            demo.Method02SingleConditionMultiLineBranches(p2);
            demo.Method03SingleConditionSingleLineBranches(p1);
            demo.Method03SingleConditionSingleLineBranches(p2);
#pragma warning disable format
            (int, int)[] p10 = [(10,  0), (10, 10), (0, 10), (0, 0), (10, 0)];
            (int, int)[] p20 = [( 0, 10), (10, 10), (0, 10), (0, 0), (10, 0)];
            (int, int)[] p30 = [( 0,  0), ( 0, 10), (0, 10), (0, 0), (10, 0)];
#pragma warning restore format
            demo.Method04MultiConditionMultiLineBranches(p10);
            demo.Method04MultiConditionMultiLineBranches(p20);
            demo.Method04MultiConditionMultiLineBranches(p30);
            demo.Method05MultiConditionSingleLineBranches(p10);
            demo.Method05MultiConditionSingleLineBranches(p20);
            demo.Method05MultiConditionSingleLineBranches(p30);
#pragma warning disable format
            int[] p100 = [10, 10, 20, 0, 10];
            int[] p200 = [20, 10, 20, 0, 10];
            int[] p300 = [ 0, 20, 20, 0, 10];
#pragma warning restore format
            demo.Method06MultiPatternConditionMultiLineBranches(p100);
            demo.Method06MultiPatternConditionMultiLineBranches(p200);
            demo.Method06MultiPatternConditionMultiLineBranches(p300);
            demo.Method07MultiPatternConditionSingleLineBranches(p100);
            demo.Method07MultiPatternConditionSingleLineBranches(p200);
            demo.Method07MultiPatternConditionSingleLineBranches(p300);
        }
    }

    [Fact]
    public void Test3()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class11CoverageDemo();
            for (int i = 0; i <= 10; i++)
            {
                demo.Method08SwitchStatementFullCoverage(i);
                demo.Method10SwitchExpressionFullCoverage(i);
                if ((i & 1) == 0)
                {
                    demo.Method09SwitchStatementPartialCoverage(i);
                    demo.Method11SwitchExpressionPartialCoverage(i);
                }
            }
        }
    }

#pragma warning disable CA1031 // Do not catch general exception types
    [Fact]
    public void Test4()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class11CoverageDemo();
            try { demo.Method15UnreachableCode(0); } catch { }
            try { demo.Method15UnreachableCode(-1); } catch { }
            try { demo.Method16UnreachableCodeIf(0); } catch { }
            try { demo.Method16UnreachableCodeIf(-1); } catch { }
        }
    }
#pragma warning restore CA1031 // Do not catch general exception types

    [Fact]
    public void Test5()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class11CoverageDemo();
            var result = demo.Method12YieldEnumerable(0).ToArray();
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public void Test6()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class11CoverageDemo();
            var result = demo.Method13YieldAsyncEnumerable(0).ToBlockingEnumerable().ToArray();
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public async Task Test7()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class11CoverageDemo();
            await demo.Method14AsyncTask(0);
        }
    }

    [Fact]
    public void Test8()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class12CoverageDemoGenerics();
            demo.Method01GenericsFullCoverage(0, 1, true);
            demo.Method01GenericsFullCoverage(0, 1, false);
            demo.Method02GenericsFullCoverageViaDifferentTypeParam(0, 1, true);
            demo.Method02GenericsFullCoverageViaDifferentTypeParam(0.0, 1.0, false);
            demo.Method03GenericsTypeOptimizationApplied(0, 1);
            demo.Method04GenericsTypeOptimizationNotApplied(0.0, 1.0);
            demo.Method05GenericsTypeOptimizationBoth(0, 1);
            demo.Method05GenericsTypeOptimizationBoth(0.0, 1.0);
        }
    }
}
