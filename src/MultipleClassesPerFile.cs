// Copyright (c) Matthias Wolf, Mawosoft.

namespace FooLib;

#pragma warning disable CA1822 // Mark members as static
public class MultipleClassesPerFile
{
    public int Method1(int value)
    {
        return value;
    }
}

public class AnotherClass
{
    public int Method1(int value)
    {
        return value;
    }
}
