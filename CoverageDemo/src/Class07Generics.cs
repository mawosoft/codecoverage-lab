// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

#pragma warning disable CA1034 // Nested types should not be visible
public class Class07Generics<TFoo, TBar>
{
    public TFoo Method1(TFoo value)
    {
        return value;
    }

    public TBar Method2(TBar value)
    {
        return value;
    }

    public class Class07GenericsInner1
    {
        public int Method1(int value)
        {
            return value;
        }
    }

    public class Class07GenericsInner2<TBuzz>
    {
        public TBuzz Method1(TBuzz value)
        {
            return value;
        }
    }
}

public class Class07GenericsDerived<TFoo> : Class07Generics<TFoo, string>
{
    public TFoo Method3(TFoo value)
    {
        return value;
    }
}
