// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;


#pragma warning disable CA1062 // Validate arguments of public methods
public class Class13CoverageDemoIfConditions
{
    private readonly int _someField;

    public Class13CoverageDemoIfConditions(int value = 0) => _someField = value;

    public int Method01SingleConditionMultiLineBranches(params int[] values)
    {
        int result = 0;
        // Expected coverage: 100% -> 100%
        if (values[0] == 10)
        {
            result += values[0] + _someField;
        }
        else
        {
            result += values[0] - _someField;
        }
        // Expected coverage: 50% -> 50%, hit | nohit
        if (values[1] == 10)
        {
            result += values[1] + _someField;
        }
        else
        {
            result += values[1] - _someField;
        }
        // Expected coverage: 50% -> 50%, nohit | hit
        if (values[2] == 10)
        {
            result += values[2] + _someField;
        }
        else
        {
            result += values[2] - _someField;
        }
        return result;
    }

    public int Method02SingleConditionSingleLineBranches(params int[] values)
    {
        int result = 0;
        // Expected coverage: 100% -> 100%
        result += values[0] == 10 ? values[0] + _someField : values[0] - _someField;
        // Expected coverage: 50% -> 50%, hit | nohit
        result += values[1] == 10 ? values[1] + _someField : values[1] - _someField;
        // Expected coverage: 50% -> 50%, nohit | hit
        result += values[2] == 10 ? values[2] + _someField : values[2] - _someField;
        return result;
    }

    public int Method03MultiConditionMultiLineBranches(params (int, int)[] values)
    {
        int result = 0;
        // Expected coverage: 100% + 100% -> 100%
        if (values[0].Item1 == 10 || values[0].Item2 == 10)
        {
            result += values[0].Item1 + _someField;
        }
        else
        {
            result += values[0].Item2 - _someField;
        }
        // Expected coverage: 100% + 50% -> 75%, hit | nohit
        if (values[1].Item1 == 10 || values[1].Item2 == 10)
        {
            result += values[1].Item1 + _someField;
        }
        else
        {
            result += values[1].Item2 - _someField;
        }
        // Expected coverage: 50% + 50% -> 50%, hit | nohit
        if (values[2].Item1 == 10 || values[2].Item2 == 10)
        {
            result += values[2].Item1 + _someField;
        }
        else
        {
            result += values[2].Item2 - _someField;
        }
        // Expected coverage: 50% + 50% -> 50%, nohit | hit
        if (values[3].Item1 == 10 || values[3].Item2 == 10)
        {
            result += values[3].Item1 + _someField;
        }
        else
        {
            result += values[3].Item2 - _someField;
        }
        // Expected coverage: 50% + 0% -> 25%, hit | nohit
        // Actual coverage: 50% + 50% -> 50%, hit | nohit
        if (values[4].Item1 == 10 || values[4].Item2 == 10)
        {
            result += values[4].Item1 + _someField;
        }
        else
        {
            result += values[4].Item2 - _someField;
        }
        return result;
    }

    public int Method04MultiConditionSingleLineBranches(params (int, int)[] values)
    {
        int result = 0;
        // Expected coverage: 100% + 100% -> 100%
        result += values[0].Item1 == 10 || values[0].Item2 == 10 ? values[0].Item1 + _someField : values[0].Item2 - _someField;
        // Expected coverage: 100% + 50% -> 75%, hit | nohit
        result += values[1].Item1 == 10 || values[1].Item2 == 10 ? values[1].Item1 + _someField : values[1].Item2 - _someField;
        // Expected coverage: 50% + 50% -> 50%, hit | nohit
        // Actual coverage: 100% + 50% -> 75%
        result += values[2].Item1 == 10 || values[2].Item2 == 10 ? values[2].Item1 + _someField : values[2].Item2 - _someField;
        // Expected coverage: 50% + 50% -> 50%, nohit | hit
        result += values[3].Item1 == 10 || values[3].Item2 == 10 ? values[3].Item1 + _someField : values[3].Item2 - _someField;
        // Expected coverage: 50% + 0% -> 25%, hit | nohit
        // Actual coverlet: 50% + 50% -> 50%
        // Actual mscc: as expected
        result += values[4].Item1 == 10 || values[4].Item2 == 10 ? values[4].Item1 + _someField : values[4].Item2 - _someField;
        return result;
    }

    public int Method05MultiPatternConditionMultiLineBranches(params int[] values)
    {
        int result = 0;
        // Expected coverage: 100% + 100% -> 100%
        // Actual coverage: 100% + 100% + 100% -> 100%
        if (values[0] is 10 or 20)
        {
            result += values[0] + _someField;
        }
        else
        {
            result += values[0] - _someField;
        }
        // Expected coverage: 100% + 50% -> 75%, hit | nohit
        // Actual coverage: 100% + 50% + 50% -> 66%, hit | nohit
        if (values[1] is 10 or 20)
        {
            result += values[1] + _someField;
        }
        else
        {
            result += values[1] - _someField;
        }
        // Expected coverage: 50% + 50% -> 50%, hit | nohit
        // Actual coverage: 100% + 50% + 50% -> 66%, hit | nohit
        if (values[2] is 10 or 20)
        {
            result += values[2] + _someField;
        }
        else
        {
            result += values[2] - _someField;
        }
        // Expected coverage: 50% + 50% -> 50%, nohit | hit
        // Actual coverage: 50% + 50% + 50% -> 50%, nohit | hit
        if (values[3] is 10 or 20)
        {
            result += values[3] + _someField;
        }
        else
        {
            result += values[3] - _someField;
        }
        // Expected coverage: 50% + 0% -> 25%, hit | nohit
        // Actual coverlet: 50% + 50% + 50% -> 50%, hit | nohit
        // Actual mscc: 50% + 0% + 50% -> 33%, hit | nohit
        if (values[4] is 10 or 20)
        {
            result += values[4] + _someField;
        }
        else
        {
            result += values[4] - _someField;
        }
        return result;
    }

    public int Method06MultiPatternConditionSingleLineBranches(params int[] values)
    {
        int result = 0;
        // Expected coverage: 100% + 100% -> 100%
        // Actual coverage: 100% + 100% + 100% -> 100%
        result += values[0] is 10 or 20 ? values[0] + _someField : values[0] - _someField;
        // Expected coverage: 100% + 50% -> 75%, hit | nohit
        // Actual coverage: 100% + 50% + 50% -> 66%
        result += values[1] is 10 or 20 ? values[1] + _someField : values[1] - _someField;
        // Expected coverage: 50% + 50% -> 50%, hit | nohit
        // Actual coverage: 100% + 50% + 50% -> 66%
        result += values[2] is 10 or 20 ? values[2] + _someField : values[2] - _someField;
        // Expected coverage: 50% + 50% -> 50%, nohit | hit
        // Actual coverage: 50% + 50% + 50% -> 50%
        result += values[3] is 10 or 20 ? values[3] + _someField : values[3] - _someField;
        // Expected coverage: 50% + 0% -> 25%, hit | nohit
        // Actual coverlet: 50% + 50% + 50% -> 50%
        // Actual mscc: 50% + 0% + 50% -> 33%
        result += values[4] is 10 or 20 ? values[4] + _someField : values[4] - _someField;
        return result;
    }
}
