// Copyright (c) Matthias Wolf, Mawosoft.

using System;

namespace CoverageDemo;

public class GenericsTests
{
    [Fact]
    public void Test1()
    {
        var c1 = new Class07Generics<int, double>();
        c1.Method1(default);
        c1.Method2(default);
        var c2 = new Class07Generics<double, int>();
        c2.Method1(default);
        c2.Method2(default);
        var c3 = new Class07Generics<DateTime, TimeSpan>.Class07GenericsInner1();
        c3.Method1(default);
        var c4 = new Class07Generics<double, long>.Class07GenericsInner2<int>();
        c4.Method1(0);
        var c5 = new Class07GenericsDerived<DateOnly>();
        c5.Method1(default);
        c5.Method2(default!);
        c5.Method3(default);
    }
}
