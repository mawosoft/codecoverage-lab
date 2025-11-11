// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

#pragma warning disable CA1822 // Mark members as static
public class Class01SingleClassSingleFile
{
    public int Method1(int value)
    {
        return value;
    }

    public int Method2(int value)
    {
        return Method2Inner(value);

        static int Method2Inner(int value)
        {
            return value;
        }
    }
}
