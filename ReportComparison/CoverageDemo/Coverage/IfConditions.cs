// Copyright (c) Matthias Wolf, Mawosoft.

using System.Diagnostics.CodeAnalysis;

namespace CoverageDemo.Coverage;

// If statement and ternary operator coverage.

public class IfConditions
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new IfConditions();
            _ = c.SingleCondition(0);
            _ = c.SingleCondition(1);
            _ = c.SingleCondition(2);
            _ = c.SingleCondition(3);
            _ = c.SingleCondition(4);
            _ = c.MultiCondition(0, 0);
            _ = c.MultiCondition(1, 1);
            _ = c.MultiCondition(2, 2);
            _ = c.MultiCondition(3, 3);
            _ = c.MultiCondition(4, 4);
            _ = c.PatternMatching(0, 0, 0, 0);
            _ = c.PatternMatching(1, 1, 1, 1);
            _ = c.PatternMatching(2, 2, 2, 2);
            _ = c.PatternMatching(3, 3, 3, 3);
            _ = c.PatternMatching(4, 4, 4, 4);
        }
    }

    public int SingleCondition(MyInt value)
    {
        int result = 0;

        // true/false
        if (value > 0)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (value > 1)
        {
            result += 5;
        }
        if (value > 2) result += value;
        result += value > 3 ? 10 : 20;

        // Always false
        if (value > 4)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (value > 5)
        {
            result += 5;
        }
        if (value > 6) result += value;
        result += value > 7 ? 10 : 20;

        // Always true
        if (value < 10)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (value < 9)
        {
            result += 5;
        }
        if (value < 8) result += value;
        result += value < 7 ? 10 : 20;
        return result;
    }

    public int MultiCondition(MyInt value1, MyInt value2)
    {
        int result = 0;

        // true/false && true/false
        if (value1 > 0 && value2 > 0)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (value1 > 1 && value2 > 1)
        {
            result += 5;
        }
        if (value1 > 2 && value2 > 2) result += value1;
        result += value1 > 3 && value2 > 3 ? 10 : 20;

        // true && true/false
        if (value1 < 10 && value2 > 0)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (value1 < 9 && value2 > 1)
        {
            result += 5;
        }
        if (value1 < 8 && value2 > 2) result += value1;
        result += value1 < 7 && value2 > 3 ? 10 : 20;

        // Always false && false
        if (value1 > 4 && value2 > 4)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (value1 > 5 && value2 > 5)
        {
            result += 5;
        }
        if (value1 > 6 && value2 > 6) result += value1;
        result += value1 > 7 && value2 > 7 ? 10 : 20;

        // Always true && true
        if (value1 < 10 && value2 < 10)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (value1 < 9 && value2 < 9)
        {
            result += 5;
        }
        if (value1 < 8 && value2 < 8) result += value1;
        result += value1 < 7 && value2 < 7 ? 10 : 20;
        return result;
    }

    public int PatternMatching(MyInt value1, MyInt value2, MyInt value3, MyInt value4)
    {
        int result = 0;
        int v1 = value1;
        int v2 = value2;
        int v3 = value3;
        int v4 = value4;

        // true/false && true/false
        if (v1 is > 0 and < 4)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (v2 is > 0 and < 4)
        {
            result += 5;
        }
        if (v3 is > 0 and < 4) result += value1;
        result += v4 is > 0 and < 4 ? 10 : 20;

        // true && true/false
        if (v1 is < 10 and > 0)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (v2 is < 10 and > 0)
        {
            result += 5;
        }
        if (v3 is < 10 and > 0) result += value1;
        result += v4 is < 10 and > 0 ? 10 : 20;

        // Always false && true
        if (v1 is < 0 and > -10)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (v2 is < 0 and > -10)
        {
            result += 5;
        }
        if (v3 is < 0 and > -10) result += value1;
        result += v4 is < 0 and > -10 ? 10 : 20;

        // Always true && true
        if (v1 is < 10 and > -10)
        {
            result++;
        }
        else
        {
            result--;
        }
        if (v2 is < 10 and > -10)
        {
            result += 5;
        }
        if (v3 is < 10 and > -10) result += value1;
        result += v4 is < 10 and > -10 ? 10 : 20;
        return result;
    }
}
