// Copyright (c) Matthias Wolf, Mawosoft.

using Xunit;

namespace FooLib;

public class MiscTests
{
    [Fact]
    public void Test1()
    {
        var c1 = new MultipleClassesPerFile();
        c1.Method1(0);
        var c2 = new AnotherClass();
        c2.Method1(0);
        var c3 = new SingleClassPerFile();
        c3.Method1(0);
    }

    [Fact]
    public void Test2()
    {
        var c1 = new NestedClassMultipleFiles();
        c1.Method1(-1);
        var c2 = new NestedClassSingleFile();
        c2.Method1(-1);
    }
}
