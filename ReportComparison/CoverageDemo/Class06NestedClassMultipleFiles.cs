// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

#pragma warning disable CA1822 // Mark members as static
public partial class Class06NestedClassMultipleFiles
{
    public int Method1(int value)
    {
        var inner = new Class06NestedInner();
        return inner.Method1(value);
    }
}
