// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoverageDemo;

/// <summary>
/// Custom IEnumerable/IAsyncEnumerable type to use whenever we are not interested in signature differences.
/// </summary>
public readonly struct MyIntEnumerable : IEnumerable<int>, IAsyncEnumerable<int>
{
    private readonly IEnumerable<int> _inner;

    public MyIntEnumerable(IEnumerable<int> inner) => _inner = inner;

    public IEnumerator<int> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();

    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new AsyncEnumerator(_inner.GetEnumerator());

    public readonly struct AsyncEnumerator : IAsyncEnumerator<int>
    {
        private readonly IEnumerator<int> _inner;

        public AsyncEnumerator(IEnumerator<int> inner) => _inner = inner;

        public int Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
