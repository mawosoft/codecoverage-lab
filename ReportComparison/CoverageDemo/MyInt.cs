// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

/// <summary>
/// Custom int type to use whenever we are not interested in signature differences.
/// </summary>
public struct MyInt
{
    public int Value { get; set; }
    public static implicit operator MyInt(int value) => new() { Value = value };
    public static implicit operator int(MyInt value) => value.Value;
}
