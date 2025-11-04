// Copyright (c) Matthias Wolf, Mawosoft.

using System.Text.Json;
using Xunit;

namespace FooLib;

public class SourceGenDemoTests
{
    [Fact]
    public void Test1()
    {
        var demo1 = new SourceGenDemoJsonSerializer();
        var json = JsonSerializer.Serialize(demo1, SourceGenDemoContext.Default.SourceGenDemoJsonSerializer);
        var demo2 = JsonSerializer.Deserialize(json, SourceGenDemoContext.Default.SourceGenDemoJsonSerializer);
        Assert.Equivalent(demo1, demo2);
    }

    [Fact]
    public void Test2()
    {
        var demo = new SourceGenDemoLibraryImport();
        demo.Method1(SourceGenDemoLibraryImport.STD_OUTPUT_HANDLE);
    }

    [Fact]
    public void Test3()
    {
        var demo = new SourceGenDemoRegex();
        demo.Method1("foo");
    }
}
