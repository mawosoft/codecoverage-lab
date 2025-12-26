// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ReportComparer.Helpers;

internal static class EquatableSequence
{
    public static EquatableSequence<T> Create<T>(IReadOnlyCollection<T>? values)
    {
        return new EquatableSequence<T>(values);
    }
}

[DebuggerDisplay("{Values}")]
internal readonly struct EquatableSequence<T> : IEquatable<EquatableSequence<T>>
{
    public IReadOnlyCollection<T>? Values { get; }
    public IReadOnlyCollection<T> ValuesOrDefault => Values ?? [];

    public EquatableSequence(IReadOnlyCollection<T>? values) => Values = values;

    public override int GetHashCode()
    {
        if (Values is null || Values.Count == 0) return 0;
        var hash = new HashCode();
        foreach (var v in Values) hash.Add(v);
        return hash.ToHashCode();
    }

    public bool Equals(EquatableSequence<T> other) => ValuesOrDefault.SequenceEqual(other.ValuesOrDefault);

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is EquatableSequence<T> sequence && Equals(sequence);

    public static bool operator ==(EquatableSequence<T> left, EquatableSequence<T> right) => left.Equals(right);
    public static bool operator !=(EquatableSequence<T> left, EquatableSequence<T> right) => !(left == right);
}
