// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo;

public class CoverageDemoTests
{
    [Fact]
    public void Test1()
    {
        var c1 = new Class01SingleClassSingleFile();
        c1.Method1(0);
        c1.Method2(0);
    }
}
