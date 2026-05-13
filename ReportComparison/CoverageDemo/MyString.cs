// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

/// <summary>
/// Custom string type to use whenever we are not interested in signature differences.
/// </summary>
public struct MyString
{
    public string Value { get; set; }
    public static implicit operator MyString(string value) => new() { Value = value };
    public static implicit operator string(MyString value) => value.Value;
}
