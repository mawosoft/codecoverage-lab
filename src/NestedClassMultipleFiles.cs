// Copyright (c) Matthias Wolf, Mawosoft.

namespace FooLib;

public partial class NestedClassMultipleFiles
{
    public int AutoProp1 { get; set; }

    public int Method1(int value)
    {
        if (value < 0)
        {
            var inner = new Inner { AutoProp1 = value };
            return inner.Method1(value);
        }
        else
        {
            return value - AutoProp1;
        }
    }
}
