// Copyright (c) Matthias Wolf, Mawosoft.

namespace DemoLib;

#pragma warning disable CA1822 // Mark members as static
public class Class05NestedClassSingleFile
{
    public int Method1(int value)
    {
        var inner = new Class05NestedInner();
        return inner.Method1(value);
    }

    private sealed class Class05NestedInner
    {
        public int Method1(int value)
        {
            return value;
        }
    }
}
