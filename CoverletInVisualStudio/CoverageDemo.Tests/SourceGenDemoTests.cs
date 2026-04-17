// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

public class SourceGenDemoTests
{
    [Fact]
    public void Test1()
    {
        var demo = new Class08SourceGenDemoRegex();
        demo.Method1("foo");
    }

    [Fact]
    public void Test2()
    {
        var demo = new Class09SourceGenDemoJsonSerializer();
        demo.Method1(0);
    }

    [Fact]
    public void Test3()
    {
        var demo = new Class10SourceGenDemoLibraryImport();
        demo.Method1();
    }
}
