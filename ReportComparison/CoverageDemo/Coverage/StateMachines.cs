// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace CoverageDemo.Coverage;

// State machine coverage.

public class StateMachines
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new StateMachines();
            _ = c.AsyncTaskFull(0).GetAwaiter().GetResult();
            _ = c.AsyncTaskFull(-1).GetAwaiter().GetResult();
            _ = c.AsyncTaskPartial(0).GetAwaiter().GetResult();
            _ = c.AsyncEnumerableFull(0).ToBlockingEnumerable().ToArray();
            _ = c.AsyncEnumerableFull(-1).ToBlockingEnumerable().ToArray();
            _ = c.AsyncEnumerablePartial(0).ToBlockingEnumerable().ToArray();
            _ = c.EnumerableFull(0).ToArray();
            _ = c.EnumerableFull(-1).ToArray();
            _ = c.EnumerablePartial(0).ToArray();
        }
    }

    public async Task<int> AsyncTaskFull(MyInt value)
    {
        await Task.Delay(1).ConfigureAwait(false);
        if (value >= 0) return 2 * value;
        await Task.CompletedTask.ConfigureAwait(false);
        return value;
    }

    public async Task<int> AsyncTaskPartial(MyInt value)
    {
        await Task.Delay(1).ConfigureAwait(false);
        if (value >= 0) return 2 * value;
        // Expected: no hit
        await Task.CompletedTask.ConfigureAwait(false);
        return value;
    }

    public async IAsyncEnumerable<int> AsyncEnumerableFull(MyInt value)
    {
        yield return 3 * value;
        await Task.Delay(1).ConfigureAwait(false);
        if (value >= 0) yield break;
        yield return 2 * value;
        await Task.CompletedTask.ConfigureAwait(false);
        yield return value;
    }

    public async IAsyncEnumerable<int> AsyncEnumerablePartial(MyInt value)
    {
        yield return 3 * value;
        await Task.Delay(1).ConfigureAwait(false);
        if (value >= 0) yield break;
        // Expected: no hit
        yield return 2 * value;
        await Task.CompletedTask.ConfigureAwait(false);
        yield return value;
    }

    public IEnumerable<int> EnumerableFull(MyInt value)
    {
        yield return 3 * value;
        if (value >= 0) yield break;
        yield return value;
    }

    public IEnumerable<int> EnumerablePartial(MyInt value)
    {
        yield return 3 * value;
        if (value >= 0) yield break;
        // Expected: no hit
        yield return value;
    }
}
