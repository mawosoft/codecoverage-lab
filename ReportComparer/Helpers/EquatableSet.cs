// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ReportComparer.Helpers;

internal static class EquatableSet
{
    public static EquatableSet<T> Create<T>(HashSet<T>? values) => new(values);
    public static EquatableSet<T> ToEquatableSet<T>(this IEnumerable<T>? values, IEqualityComparer<T>? comparer = null) => new(values?.ToHashSet(comparer));
}

[DebuggerDisplay("{_values}")]
internal readonly struct EquatableSet<T> : IEquatable<EquatableSet<T>>
{
    private readonly HashSet<T>? _values;
    public IReadOnlySet<T> Values => (IReadOnlySet<T>?)_values ?? ImmutableHashSet<T>.Empty;
    public bool IsEmpty => _values is null || _values.Count == 0;
    public int Count => _values?.Count ?? 0;

    public EquatableSet(HashSet<T>? values) => _values = values;

    public override int GetHashCode()
    {
        if (_values is null || _values.Count == 0) return 0;
        var comparer = _values.Comparer;
        int hash = 0;
        foreach (var v in _values)
        {
            if (v is not null) hash ^= comparer.GetHashCode(v);
        }
        return hash;
    }

    public bool Equals(EquatableSet<T> other)
    {
        if (ReferenceEquals(_values, other._values)) return true;
        int count = Count;
        if (count != other.Count) return false;
        if (count == 0) return true;
        // Both counts are != 0
        return _values!.SetEquals(other._values!);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is EquatableSet<T> set && Equals(set);

    public static bool operator ==(EquatableSet<T> left, EquatableSet<T> right) => left.Equals(right);
    public static bool operator !=(EquatableSet<T> left, EquatableSet<T> right) => !(left == right);
}
