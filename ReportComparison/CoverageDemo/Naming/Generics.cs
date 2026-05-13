// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo.Naming;

public class Generics
{
    public int Method1(MyInt value) => value;
}

public class Generics<TFoo>
{
    public TFoo Method1(TFoo value) => value;

    public class Inner<TBar>
    {
        public TBar Method2(TBar value) => value;
    }
}

public class Generics<TFoo, TBar>
{
    public (TFoo, TBar) Method1(TFoo first, TBar second) => (first, second);
    public (TFoo, TBar, TBuzz) Method1<TBuzz>(TFoo first, TBar second, TBuzz third) => (first, second, third);

    // TODO MSCC moves all type parameters to the end, resulting in the same name for these:
    // Generics<TFoo>+Inner<TBar> and Generics<TFoo, TBar>+Inner become Generics+Inner<TFoo, TBar>
    public class Inner
    {
        public TBar Method2(TBar value) => value;
    }
}
