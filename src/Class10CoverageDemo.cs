// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics.CodeAnalysis;

namespace DemoLib;

#pragma warning disable CA1062 // Validate arguments of public methods
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

    public int Method02SingleConditionMultiLineBranches(params int[] values)
    {
        int result = 0;
        // Expected coverage: 100% -> 100%
        if (values[0] == 10)
        {
            result += values[0] + _fullProp;
        }
        else
        {
            result += values[0] - _fullProp;
        }
        // Expected coverage: 50% -> 50%, hit | nohit
        if (values[1] == 10)
        {
            result += values[1] + _fullProp;
        }
        else
        {
            result += values[1] - _fullProp;
        }
        // Expected coverage: 50% -> 50%, nohit | hit
        if (values[2] == 10)
        {
            result += values[2] + _fullProp;
        }
        else
        {
            result += values[2] - _fullProp;
        }
        return result;
    }

    public int Method03SingleConditionSingleLineBranches(params int[] values)
    {
        int result = 0;
        // Expected coverage: 100% -> 100%
        result += values[0] == 10 ? values[0] + _fullProp : values[0] - _fullProp;
        // Expected coverage: 50% -> 50%, hit | nohit
        result += values[1] == 10 ? values[1] + _fullProp : values[1] - _fullProp;
        // Expected coverage: 50% -> 50%, nohit | hit
        result += values[2] == 10 ? values[2] + _fullProp : values[2] - _fullProp;
        return result;
    }

    public int Method04MultiConditionMultiLineBranches(params (int, int)[] values)
    {
        int result = 0;
        // Expected coverage: 100% + 100% -> 100%
        if (values[0].Item1 == 10 || values[0].Item2 == 10)
        {
            result += values[0].Item1 + _fullProp;
        }
        else
        {
            result += values[0].Item2 - _fullProp;
        }
        // Expected coverage: 100% + 50% -> 75%, hit | nohit
        if (values[1].Item1 == 10 || values[1].Item2 == 10)
        {
            result += values[1].Item1 + _fullProp;
        }
        else
        {
            result += values[1].Item2 - _fullProp;
        }
        // Expected coverage: 50% + 50% -> 50%, hit | nohit
        if (values[2].Item1 == 10 || values[2].Item2 == 10)
        {
            result += values[2].Item1 + _fullProp;
        }
        else
        {
            result += values[2].Item2 - _fullProp;
        }
        // Expected coverage: 50% + 50% -> 50%, nohit | hit
        if (values[3].Item1 == 10 || values[3].Item2 == 10)
        {
            result += values[3].Item1 + _fullProp;
        }
        else
        {
            result += values[3].Item2 - _fullProp;
        }
        // Expected coverage: 50% + 0% -> 25%, hit | nohit
        // Actual coverage: 50% + 50% -> 50%, hit | nohit
        if (values[4].Item1 == 10 || values[4].Item2 == 10)
        {
            result += values[4].Item1 + _fullProp;
        }
        else
        {
            result += values[4].Item2 - _fullProp;
        }
        return result;
    }

    public int Method05MultiConditionSingleLineBranches(params (int, int)[] values)
    {
        int result = 0;
        // Expected coverage: 100% + 100% -> 100%
        result += values[0].Item1 == 10 || values[0].Item2 == 10 ? values[0].Item1 + _fullProp : values[0].Item2 - _fullProp;
        // Expected coverage: 100% + 50% -> 75%, hit | nohit
        result += values[1].Item1 == 10 || values[1].Item2 == 10 ? values[1].Item1 + _fullProp : values[1].Item2 - _fullProp;
        // Expected coverage: 50% + 50% -> 50%, hit | nohit
        // Actual coverage: 100% + 50% -> 75%
        result += values[2].Item1 == 10 || values[2].Item2 == 10 ? values[2].Item1 + _fullProp : values[2].Item2 - _fullProp;
        // Expected coverage: 50% + 50% -> 50%, nohit | hit
        result += values[3].Item1 == 10 || values[3].Item2 == 10 ? values[3].Item1 + _fullProp : values[3].Item2 - _fullProp;
        // Expected coverage: 50% + 0% -> 25%, hit | nohit
        // Actual coverlet: 50% + 50% -> 50%
        // Actual mscc: as expected
        result += values[4].Item1 == 10 || values[4].Item2 == 10 ? values[4].Item1 + _fullProp : values[4].Item2 - _fullProp;
        return result;
    }

    public int Method06MultiPatternConditionMultiLineBranches(params int[] values)
    {
        int result = 0;
        // Expected coverage: 100% + 100% -> 100%
        // Actual coverage: 100% + 100% + 100% -> 100%
        if (values[0] is 10 or 20)
        {
            result += values[0] + _fullProp;
        }
        else
        {
            result += values[0] - _fullProp;
        }
        // Expected coverage: 100% + 50% -> 75%, hit | nohit
        // Actual coverage: 100% + 50% + 50% -> 66%, hit | nohit
        if (values[1] is 10 or 20)
        {
            result += values[1] + _fullProp;
        }
        else
        {
            result += values[1] - _fullProp;
        }
        // Expected coverage: 50% + 50% -> 50%, hit | nohit
        // Actual coverage: 100% + 50% + 50% -> 66%, hit | nohit
        if (values[2] is 10 or 20)
        {
            result += values[2] + _fullProp;
        }
        else
        {
            result += values[2] - _fullProp;
        }
        // Expected coverage: 50% + 50% -> 50%, nohit | hit
        // Actual coverage: 50% + 50% + 50% -> 50%, nohit | hit
        if (values[3] is 10 or 20)
        {
            result += values[3] + _fullProp;
        }
        else
        {
            result += values[3] - _fullProp;
        }
        // Expected coverage: 50% + 0% -> 25%, hit | nohit
        // Actual coverlet: 50% + 50% + 50% -> 50%, hit | nohit
        // Actual mscc: 50% + 0% + 50% -> 33%, hit | nohit
        if (values[4] is 10 or 20)
        {
            result += values[4] + _fullProp;
        }
        else
        {
            result += values[4] - _fullProp;
        }
        return result;
    }

    public int Method07MultiPatternConditionSingleLineBranches(params int[] values)
    {
        int result = 0;
        // Expected coverage: 100% + 100% -> 100%
        // Actual coverage: 100% + 100% + 100% -> 100%
        result += values[0] is 10 or 20 ? values[0] + _fullProp : values[0] - _fullProp;
        // Expected coverage: 100% + 50% -> 75%, hit | nohit
        // Actual coverage: 100% + 50% + 50% -> 66%
        result += values[1] is 10 or 20 ? values[1] + _fullProp : values[1] - _fullProp;
        // Expected coverage: 50% + 50% -> 50%, hit | nohit
        // Actual coverage: 100% + 50% + 50% -> 66%
        result += values[2] is 10 or 20 ? values[2] + _fullProp : values[2] - _fullProp;
        // Expected coverage: 50% + 50% -> 50%, nohit | hit
        // Actual coverage: 50% + 50% + 50% -> 50%
        result += values[3] is 10 or 20 ? values[3] + _fullProp : values[3] - _fullProp;
        // Expected coverage: 50% + 0% -> 25%, hit | nohit
        // Actual coverlet: 50% + 50% + 50% -> 50%
        // Actual mscc: 50% + 0% + 50% -> 33%
        result += values[4] is 10 or 20 ? values[4] + _fullProp : values[4] - _fullProp;
        return result;
    }

    public int Method08SwitchStatementFullCoverage(int value)
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

    public int Method09SwitchStatementPartialCoverage(int value)
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

    public int Method10SwitchExpressionFullCoverage(int value)
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

    public int Method11SwitchExpressionPartialCoverage(int value)
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

    public int Method12UnreachableCode(int value)
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

    public int Method13UnreachableCodeIf(int value)
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
