// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics.CodeAnalysis;

namespace FooLib;

public class Class10CoverageDemo
{
    private int _fullProp;
    public int AutoProp01SingleLineFullCoverage { get; set; }
    public int AutoProp02SingleLineGetCoverage { get; set; }
    public int AutoProp03SingleLineSetCoverage { get; set; }

    public int AutoProp04MultiLineFullCoverage
    {
        get;
        set;
    }

    public int AutoProp05MultiLineGetCoverage
    {
        get;
        set;
    }

    public int AutoProp06MultiLineSetCoverage
    {
        get;
        set;
    }

    public int AutoProp07InitSingleLineFullCoverage { get; init; }
    public int AutoProp08InitSingleLineInitCoverage { get; init; }

    public int AutoProp09InitMultiLineFullCoverage
    {
        get;
        init;
    }

    public int AutoProp10InitMultiLineInitCoverage
    {
        get;
        init;
    }

    public int FullProp1SingleLineFullCoverage { get => _fullProp; set => _fullProp = value; }
    public int FullProp2SingleLineGetCoverage { get => _fullProp; set => _fullProp = value; }
    public int FullProp3SingleLineSetCoverage { get => _fullProp; set => _fullProp = value; }

    public int FullProp4MultiLineFullCoverage
    {
        get => _fullProp;
        set => _fullProp = value;
    }

    public int FullProp5MultiLineGetCoverage
    {
        get => _fullProp;
        set => _fullProp = value;
    }

    public int FullProp6MultiLineSetCoverage
    {
        get => _fullProp;
        set => _fullProp = value;
    }

    public int Method01NoBranch(int value)
    {
        return value - _fullProp;
    }

    public int Method02SingleBranchFullCoverage(int value)
    {
        if (value < 0)
        {
            return value + _fullProp;
        }
        else
        {
            return value - _fullProp;
        }
    }

    public int Method03SingleBranchPartialCoverage(int value)
    {
        if (value < 0)
        {
            return value + _fullProp;
        }
        else
        {
            return value - _fullProp;
        }
    }

    public int Method04SingleBranchSingleLineFullCoverage(int value)
    {
        return value < 0 ? value + _fullProp : value - _fullProp;
    }

    public int Method05SingleBranchSingleLinePartialCoverage(int value)
    {
        return value < 0 ? value + _fullProp : value - _fullProp;
    }

    public int Method06MultiBranchFullCoverage(int value1, int value2)
    {
        if (value1 < 0 || value2 < 0)
        {
            return value1 + _fullProp;
        }
        else
        {
            return value1 - _fullProp;
        }
    }

    public int Method07MultiBranchPartialCoverage(int value1, int value2)
    {
        if (value1 < 0 || value2 < 0)
        {
            return value1 + _fullProp;
        }
        else
        {
            return value1 - _fullProp;
        }
    }

    public int Method08MultiBranchSingleLineFullCoverage(int value1, int value2)
    {
        return value1 < 0 || value2 < 0 ? value1 + _fullProp : value1 - _fullProp;
    }

    public int Method09MultiBranchSingleLinePartialCoverage(int value1, int value2)
    {
        return value1 < 0 || value2 < 0 ? value1 + _fullProp : value1 - _fullProp;
    }

    public int Method10SwitchStatementFullCoverage(int value)
    {
        int result;
        switch (value)
        {
            case 0:
                result = _fullProp;
                break;
            case 1:
                result = _fullProp * 10;
                _fullProp = 5;
                break;
            case 2:
                result = _fullProp + 10;
                _fullProp += 5;
                break;
            case 3:
                result = _fullProp + value;
                break;
            case 4:
                result = _fullProp + 2 * value;
                break;
            case 5:
                result = value;
                break;
            case 6:
                result = value - _fullProp;
                break;
            case 7:
                result = value - 2 * _fullProp;
                break;
            case 8:
                result = value + value * _fullProp;
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

    public int Method11SwitchStatementPartialCoverage(int value)
    {
        int result;
        switch (value)
        {
            case 0:
                result = _fullProp;
                break;
            case 1:
                result = _fullProp * 10;
                _fullProp = 5;
                break;
            case 2:
                result = _fullProp + 10;
                _fullProp += 5;
                break;
            case 3:
                result = _fullProp + value;
                break;
            case 4:
                result = _fullProp + 2 * value;
                break;
            case 5:
                result = value;
                break;
            case 6:
                result = value - _fullProp;
                break;
            case 7:
                result = value - 2 * _fullProp;
                break;
            case 8:
                result = value + value * _fullProp;
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

    public int Method12SwitchExpressionFullCoverage(int value)
    {
        var result = value switch
        {
            0 => _fullProp,
            1 => _fullProp * 10,
            2 => _fullProp + 10,
            3 => _fullProp + value,
            4 => _fullProp + 2 * value,
            5 => value,
            6 => value - _fullProp,
            7 => value - 2 * _fullProp,
            8 => value + value * _fullProp,
            9 => -2,
            _ => -1,
        };
        return result;
    }

    public int Method13SwitchExpressionPartialCoverage(int value)
    {
        var result = value switch
        {
            0 => _fullProp,
            1 => _fullProp * 10,
            2 => _fullProp + 10,
            3 => _fullProp + value,
            4 => _fullProp + 2 * value,
            5 => value,
            6 => value - _fullProp,
            7 => value - 2 * _fullProp,
            8 => value + value * _fullProp,
            9 => -2,
            _ => -1,
        };
        return result;
    }

    public int Method14UnreachableCode(int value)
    {
        if (value < 0)
        {
            ThrowSomething();
            return value + _fullProp;
        }
        else
        {
            return value - _fullProp;
        }
    }

    public int Method15UnreachableCodeIf(int value)
    {
        ThrowSomethingIf(value < 0);
        return value + _fullProp;
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
