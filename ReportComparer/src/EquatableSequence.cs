// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ReportComparer;

internal struct EquatableSequence<T> : IEquatable<EquatableSequence<T>> where T : IEquatable<T>
{
    public ICollection<T>? Value { get; set; }

    public EquatableSequence(ICollection<T>? value) => Value = value;

    public readonly bool Equals(EquatableSequence<T> other)
    {
        if (Value is null || Value.Count == 0) return other.Value is null || other.Value.Count == 0;
        if (other.Value is null || other.Value.Count == 0) return false;
        return Value.SequenceEqual(other.Value);
    }

    public override readonly int GetHashCode()
    {
        if (Value is null || Value.Count == 0) return 0;
        var hash = new HashCode();
        foreach (var v in Value) hash.Add(v);
        return hash.ToHashCode();
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is EquatableSequence<T> sequence && Equals(sequence);
    public static bool operator ==(EquatableSequence<T> left, EquatableSequence<T> right) => left.Equals(right);
    public static bool operator !=(EquatableSequence<T> left, EquatableSequence<T> right) => !(left == right);
}
