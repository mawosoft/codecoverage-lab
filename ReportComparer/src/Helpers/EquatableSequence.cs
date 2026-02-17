// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ReportComparer.Helpers;

internal static class EquatableSequence
{
    public static EquatableSequence<T> Create<T>(IReadOnlyCollection<T>? values) => new(values);
    public static EquatableSequence<T> ToEquatableSequence<T>(this IEnumerable<T>? values) => new(values?.ToArray());
}

[DebuggerDisplay("{_values}")]
internal readonly struct EquatableSequence<T> : IEquatable<EquatableSequence<T>>
{
    private readonly IReadOnlyCollection<T>? _values;

    public IReadOnlyCollection<T> Values => _values ?? [];
    public bool IsEmpty => _values is null || _values.Count == 0;
    public int Count => _values?.Count ?? 0;

    public EquatableSequence(IReadOnlyCollection<T>? values) => _values = values;

    public override int GetHashCode()
    {
        if (_values is null || _values.Count == 0) return 0;
        var hash = new HashCode();
        foreach (var v in _values) hash.Add(v);
        return hash.ToHashCode();
    }

    public bool Equals(EquatableSequence<T> other) => Values.SequenceEqual(other.Values);

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is EquatableSequence<T> sequence && Equals(sequence);

    public static bool operator ==(EquatableSequence<T> left, EquatableSequence<T> right) => left.Equals(right);
    public static bool operator !=(EquatableSequence<T> left, EquatableSequence<T> right) => !(left == right);
}
