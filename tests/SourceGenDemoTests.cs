// Copyright (c) Matthias Wolf, Mawosoft.

using Xunit;

namespace FooLib;

public class SourceGenDemoTests
{
    [Fact]
    public void Test1()
    {
        var demo = new SourceGenDemoJsonSerializer();
        demo.Method1(0);
    }

    [Fact]
    public void Test2()
    {
        var demo = new SourceGenDemoLibraryImport();
        demo.Method1();
    }

    [Fact]
    public void Test3()
    {
        var demo = new SourceGenDemoRegex();
        demo.Method1("foo");
    }
}
