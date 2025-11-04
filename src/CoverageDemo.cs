// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics.CodeAnalysis;

namespace FooLib;

public class CoverageDemo
{
    private int _prop2;
    public int AutoProp1 { get; set; }
    public int Prop2
    {
        get => _prop2;
        set => _prop2 = value;
    }

    public int NoBranch(int value)
    {
        return value - _prop2;
    }

    public int SingleBranchFullCoverage(int value)
    {
        if (value < 0)
        {
            return value + _prop2;
        }
        else
        {
            return value - _prop2;
        }
    }

    public int SingleBranchPartialCoverage(int value)
    {
        if (value < 0)
        {
            return value + _prop2;
        }
        else
        {
            return value - _prop2;
        }
    }

    public int SingleBranchSingleLineFullCoverage(int value)
    {
        return value < 0 ? value + _prop2 : value - _prop2;
    }

    public int SingleBranchSingleinePartialCoverage(int value)
    {
        return value < 0 ? value + _prop2 : value - _prop2;
    }

    public int MultiBranchFullCoverage(int value)
    {
        if (value < 0 || AutoProp1 < 0)
        {
            return value + _prop2;
        }
        else
        {
            return value - _prop2;
        }
    }

    public int MultiBranchPartialCoverage(int value)
    {
        if (value < 0 || AutoProp1 < 0)
        {
            return value + _prop2;
        }
        else
        {
            return value - _prop2;
        }
    }

    public int MultiBranchSingleLineFullCoverage(int value)
    {
        return value < 0 || AutoProp1 < 0 ? value + _prop2 : value - _prop2;
    }

    public int MultiBranchSingleLinePartialCoverage(int value)
    {
        return value < 0 || AutoProp1 < 0 ? value + _prop2 : value - _prop2;
    }

    public int SwitchStatementFullCoverage(int value)
    {
        int result;
        switch (value)
        {
            case 0:
                result = _prop2;
                break;
            case 1:
                result = _prop2 * 10;
                AutoProp1 = 5;
                break;
            case 2:
                result = _prop2 + 10;
                AutoProp1 += 5;
                break;
            case 3:
                result = _prop2 + value;
                break;
            case 4:
                result = _prop2 + 2 * value;
                break;
            case 5:
                result = value;
                break;
            case 6:
                result = value - _prop2;
                break;
            case 7:
                result = value - 2 * _prop2;
                break;
            case 8:
                result = value + value * _prop2;
                break;
            case 9:
                result = -2;
                break;
            default:
                result = -1;
                break;
        }
        return result;
    }

    public int SwitchStatementPartialCoverage(int value)
    {
        int result;
        switch (value)
        {
            case 0:
                result = _prop2;
                break;
            case 1:
                result = _prop2 * 10;
                AutoProp1 = 5;
                break;
            case 2:
                result = _prop2 + 10;
                AutoProp1 += 5;
                break;
            case 3:
                result = _prop2 + value;
                break;
            case 4:
                result = _prop2 + 2 * value;
                break;
            case 5:
                result = value;
                break;
            case 6:
                result = value - _prop2;
                break;
            case 7:
                result = value - 2 * _prop2;
                break;
            case 8:
                result = value + value * _prop2;
                break;
            case 9:
                result = -2;
                break;
            default:
                result = -1;
                break;
        }
        return result;
    }

    public int SwitchExpressionFullCoverage(int value)
    {
        var result = value switch
        {
            0 => _prop2,
            1 => _prop2 * 10,
            2 => _prop2 + 10,
            3 => _prop2 + value,
            4 => _prop2 + 2 * value,
            5 => value,
            6 => value - _prop2,
            7 => value - 2 * _prop2,
            8 => value + value * _prop2,
            9 => -2,
            _ => -1,
        };
        return result;
    }

    public int SwitchExpressionPartialCoverage(int value)
    {
        var result = value switch
        {
            0 => _prop2,
            1 => _prop2 * 10,
            2 => _prop2 + 10,
            3 => _prop2 + value,
            4 => _prop2 + 2 * value,
            5 => value,
            6 => value - _prop2,
            7 => value - 2 * _prop2,
            8 => value + value * _prop2,
            9 => -2,
            _ => -1,
        };
        return result;
    }

    public int UnreachableCode(int value)
    {
        if (value < 0)
        {
            ThrowSomething();
            return value + _prop2;
        }
        else
        {
            return value - _prop2;
        }
    }

    public int UnreachableCodeIf(int value)
    {
        ThrowSomethingIf(value < 0);
        return value + _prop2;
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
