// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;

namespace ReportComparer.Helpers;

internal static class EnumerableExtensions
{
    public static T? FirstIfSingleOrDefault<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var e = source.GetEnumerator();
        if (!e.MoveNext()) return default;
        var first = e.Current;
        if (e.MoveNext()) return default;
        return first;
    }
}
