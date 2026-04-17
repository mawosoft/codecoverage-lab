// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoverageDemo;

public class Class17CoverageDemoStateMachines
{
    private readonly int _someField;

    public Class17CoverageDemoStateMachines(int value = 0) => _someField = value;

    public IEnumerable<int> Method01YieldEnumerable(int value)
    {
        int n = Math.Max(5, value);
        for (int i = 0; i < n; i++)
        {
            yield return i + value;
        }
        yield return 10 + _someField;
        if (value < n)
        {
            yield break;
        }
        yield return 20 + _someField; // Expected: nohit
    }

    public async IAsyncEnumerable<int> Method02YieldAsyncEnumerable(int value)
    {
        int n = Math.Max(5, value);
        for (int i = 0; i < n; i++)
        {
            await Task.Delay(0).ConfigureAwait(false);
            yield return i + value;
        }
        await Task.Delay(0).ConfigureAwait(false);
        yield return 10 + _someField;
        if (value < n)
        {
            yield break;
        }
        // Expected: nohit
        await Task.Delay(0).ConfigureAwait(false);
        yield return 20 + _someField;
    }

    public async Task<int> Method03AsyncTask(int value)
    {
        int result = 0;
        int n = Math.Max(5, value);
        for (int i = 0; i < n; i++)
        {
            await Task.Delay(0).ConfigureAwait(false);
            result += value - _someField;
        }
        await Task.Delay(0).ConfigureAwait(false);
        result += 10 + _someField;
        if (value >= n)
        {
            // Expected: nohit
            await Task.Delay(0).ConfigureAwait(false);
            result += 20 + _someField;
        }
        return result;
    }
}
