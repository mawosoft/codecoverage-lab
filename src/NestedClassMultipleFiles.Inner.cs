// Copyright (c) Matthias Wolf, Mawosoft.

namespace FooLib;

#pragma warning disable CA1822 // Mark members as static
public partial class NestedClassMultipleFiles
{
    private sealed class Inner
    {
        public int Method1(int value)
        {
            return value;
        }
    }
}
