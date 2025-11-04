// Copyright (c) Matthias Wolf, Mawosoft.

namespace FooLib;

public class NestedClassSingleFile
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

    private sealed class Inner
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
}
