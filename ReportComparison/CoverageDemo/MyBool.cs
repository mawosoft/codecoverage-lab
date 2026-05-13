// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

/// <summary>
/// Custom bool type to use whenever we are not interested in signature differences.
/// </summary>
public struct MyBool
{
    public bool Value { get; set; }
    public static implicit operator MyBool(bool value) => new() { Value = value };
    public static implicit operator bool(MyBool value) => value.Value;
}
