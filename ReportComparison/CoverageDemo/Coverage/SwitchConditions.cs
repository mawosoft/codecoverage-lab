// Copyright (c) Matthias Wolf, Mawosoft.

using System.Diagnostics.CodeAnalysis;

namespace CoverageDemo.Coverage;

// Switch statement and expression coverage.

public class SwitchConditions
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new SwitchConditions();
            for (int k = 0; k <= 5; k++)
            {
                _ = c.SwitchStatementFull(k);
                _ = c.SwitchExpressionFull(k);
                if (k is 2 or 3)
                {
                    _ = c.SwitchStatementPartial(k);
                    _ = c.SwitchExpressionPartial(k);
                }
            }
        }
    }

    public int SwitchStatementFull(MyInt value)
    {
        int result = 0;
        switch (value)
        {
            case 0:
                result = value;
                break;
            case 1:
                value += 10;
                break;
            case 2:
                result = -value;
                break;
            case 3:
                result = 2 * value;
                break;
            case 4:
                result = value + 20;
                break;
            default:
                result = -1;
                break;
        }
        return result + value;
    }

    public int SwitchStatementPartial(MyInt value)
    {
        int result = 0;
        switch (value)
        {
            case 0:
                result = value;
                break;
            case 1:
                value += 10;
                break;
            case 2:
                result = -value;
                break;
            case 3:
                result = 2 * value;
                break;
            case 4:
                result = value + 20;
                break;
            default:
                result = -1;
                break;
        }
        return result + value;
    }

    public int SwitchExpressionFull(MyInt value)
    {
        int v = value;
        int result = v switch
        {
            0 => value,
            1 => value + 10,
            2 => -value,
            3 => 2 * value,
            4 => value + 20,
            _ => -1,
        };
        return result;
    }

    public int SwitchExpressionPartial(MyInt value)
    {
        int v = value;
        int result = v switch
        {
            0 => value,
            1 => value + 10,
            2 => -value,
            3 => 2 * value,
            4 => value + 20,
            _ => -1,
        };
        return result;
    }
}
