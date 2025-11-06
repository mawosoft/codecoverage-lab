// Copyright (c) Matthias Wolf, Mawosoft.

namespace FooLib;

#pragma warning disable CA1822 // Mark members as static
public class NestedClassSingleFile
{
    public int Method1(int value)
    {
        var inner = new Inner();
        return inner.Method1(value);
    }

    private sealed class Inner
    {
        public int Method1(int value)
        {
            return value;
        }
    }
}
