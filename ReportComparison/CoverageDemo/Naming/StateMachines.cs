// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoverageDemo.Naming;

public class StateMachines
{
    public async Task<int> Method1(MyInt value)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        return value;
    }

    public async IAsyncEnumerable<int> Method2(MyInt value)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield return value;
    }

    public IEnumerable<int> Method3(MyInt value)
    {
        yield return value;
    }
}
