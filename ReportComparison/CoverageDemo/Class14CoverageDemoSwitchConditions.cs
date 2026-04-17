// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

public class Class14CoverageDemoSwitchConditions
{
    private int _someField;

    public Class14CoverageDemoSwitchConditions(int value = 0) => _someField = value;

    public int Method01SwitchStatementFullCoverage(int value)
    {
        int result;
        switch (value)
        {
            case 0:
                result = _someField;
                break;
            case 1:
                result = _someField * 10;
                _someField = 5;
                break;
            case 2:
                result = _someField + 10;
                _someField += 5;
                break;
            case 3:
                result = _someField + value;
                break;
            case 4:
                result = _someField + 2 * value;
                break;
            case 5:
                result = value;
                break;
            case 6:
                result = value - _someField;
                break;
            case 7:
                result = value - 2 * _someField;
                break;
            case 8:
                result = value + value * _someField;
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

    public int Method02SwitchStatementPartialCoverage(int value)
    {
        int result;
        // Expected coverage: 6/11. nohit on odd values.
        switch (value)
        {
            case 0:
                result = _someField;
                break;
            case 1:
                result = _someField * 10;
                _someField = 5;
                break;
            case 2:
                result = _someField + 10;
                _someField += 5;
                break;
            case 3:
                result = _someField + value;
                break;
            case 4:
                result = _someField + 2 * value;
                break;
            case 5:
                result = value;
                break;
            case 6:
                result = value - _someField;
                break;
            case 7:
                result = value - 2 * _someField;
                break;
            case 8:
                result = value + value * _someField;
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

    public int Method03SwitchExpressionFullCoverage(int value)
    {
        var result = value switch
        {
            0 => _someField,
            1 => _someField * 10,
            2 => _someField + 10,
            3 => _someField + value,
            4 => _someField + 2 * value,
            5 => value,
            6 => value - _someField,
            7 => value - 2 * _someField,
            8 => value + value * _someField,
            9 => -2,
            _ => -1,
        };
        return result;
    }

    public int Method04SwitchExpressionPartialCoverage(int value)
    {
        // Expected coverage: 6/11. nohit on odd values.
        var result = value switch
        {
            0 => _someField,
            1 => _someField * 10,
            2 => _someField + 10,
            3 => _someField + value,
            4 => _someField + 2 * value,
            5 => value,
            6 => value - _someField,
            7 => value - 2 * _someField,
            8 => value + value * _someField,
            9 => -2,
            _ => -1,
        };
        return result;
    }
}
