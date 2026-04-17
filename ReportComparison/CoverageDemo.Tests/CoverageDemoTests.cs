// Copyright (c) Matthias Wolf, Mawosoft.

using System.Linq;
using System.Threading.Tasks;

namespace CoverageDemo;

public class CoverageDemoTests
{
    private const int HitCount = 5;

#pragma warning disable CA1031 // Do not catch general exception types
    [Fact]
    public void TestBasics()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class11CoverageDemoBasics();
            demo.Method01NoBranch(0);
            try { demo.Method02UnreachableCode(0); } catch { }
            try { demo.Method02UnreachableCode(-1); } catch { }
            try { demo.Method03UnreachableCodeIf(0); } catch { }
            try { demo.Method03UnreachableCodeIf(-1); } catch { }
        }
    }
#pragma warning restore CA1031 // Do not catch general exception types

    [Fact]
    public void TestProperties()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class12CoverageDemoProperties()
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
    public void TestIfConditions()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class13CoverageDemoIfConditions();
#pragma warning disable format
            int[] p1 = [10, 10, 0];
            int[] p2 = [ 0, 10, 0];
#pragma warning restore format
            demo.Method01SingleConditionMultiLineBranches(p1);
            demo.Method01SingleConditionMultiLineBranches(p2);
            demo.Method02SingleConditionSingleLineBranches(p1);
            demo.Method02SingleConditionSingleLineBranches(p2);
#pragma warning disable format
            (int, int)[] p10 = [(10,  0), (10, 10), (0, 10), (0, 0), (10, 0)];
            (int, int)[] p20 = [( 0, 10), (10, 10), (0, 10), (0, 0), (10, 0)];
            (int, int)[] p30 = [( 0,  0), ( 0, 10), (0, 10), (0, 0), (10, 0)];
#pragma warning restore format
            demo.Method03MultiConditionMultiLineBranches(p10);
            demo.Method03MultiConditionMultiLineBranches(p20);
            demo.Method03MultiConditionMultiLineBranches(p30);
            demo.Method04MultiConditionSingleLineBranches(p10);
            demo.Method04MultiConditionSingleLineBranches(p20);
            demo.Method04MultiConditionSingleLineBranches(p30);
#pragma warning disable format
            int[] p100 = [10, 10, 20, 0, 10];
            int[] p200 = [20, 10, 20, 0, 10];
            int[] p300 = [ 0, 20, 20, 0, 10];
#pragma warning restore format
            demo.Method05MultiPatternConditionMultiLineBranches(p100);
            demo.Method05MultiPatternConditionMultiLineBranches(p200);
            demo.Method05MultiPatternConditionMultiLineBranches(p300);
            demo.Method06MultiPatternConditionSingleLineBranches(p100);
            demo.Method06MultiPatternConditionSingleLineBranches(p200);
            demo.Method06MultiPatternConditionSingleLineBranches(p300);
        }
    }

    [Fact]
    public void TestSwitchConditions()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class14CoverageDemoSwitchConditions();
            for (int i = 0; i <= 10; i++)
            {
                demo.Method01SwitchStatementFullCoverage(i);
                demo.Method03SwitchExpressionFullCoverage(i);
                if ((i & 1) == 0)
                {
                    demo.Method02SwitchStatementPartialCoverage(i);
                    demo.Method04SwitchExpressionPartialCoverage(i);
                }
            }
        }
    }


    [Fact]
    public void TestLambdas()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class15CoverageDemoLamdas();
            demo.Method01LambdaFullCoverage(0);
            demo.Method02LambdaPartialCoverage(0);
        }
    }

    [Fact]
    public void TestGenerics()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class16CoverageDemoGenerics();
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


#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
    [Fact]
    public void TestStateMachines()
    {
        for (int hits = 0; hits < HitCount; hits++)
        {
            var demo = new Class17CoverageDemoStateMachines();
            _ = demo.Method01YieldEnumerable(0).ToArray();
            _ = demo.Method02YieldAsyncEnumerable(0).ToBlockingEnumerable().ToArray();
            _ = demo.Method03AsyncTask(0).GetAwaiter().GetResult();
        }
    }
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
}
