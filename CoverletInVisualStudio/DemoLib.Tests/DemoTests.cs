// Copyright (c) Matthias Wolf, Mawosoft.

using Xunit;

namespace DemoLib;

public class DemoTests
{
    [Fact]
    public void Test1()
    {
        var c = new Demo();
        Assert.Equal(0, c.Method1());
    }

    [Fact]
    public void Test2()
    {
        var c1 = new SourceGenJson { AutoProp1 = new System.DateTime(42) };
        var c2 = c1.Method1() as SourceGenJson;
        Assert.NotNull(c2);
        Assert.Equal(c1.AutoProp1, c2.AutoProp1);
    }
}
