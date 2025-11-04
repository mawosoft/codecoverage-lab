// Copyright (c) Matthias Wolf, Mawosoft.

namespace FooLib;

public class SingleClassPerFile
{
    public int AutoProp1 { get; set; }

    public int Method1(int value)
    {
        if (value < 0)
        {
            return value + AutoProp1;
        }
        else
        {
            return value - AutoProp1;
        }
    }
}
