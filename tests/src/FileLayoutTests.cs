// Copyright (c) Matthias Wolf, Mawosoft.

namespace DemoLib;

public class FileLayoutTests
{
    [Fact]
    public void Test1()
    {
        var c1 = new Class01SingleClassSingleFile();
        c1.Method1(0);
        var c2 = new Class02SingleClassMultipleFiles();
        c2.Method1(0);
        c2.Method2(0);
        var c3 = new Class03MultipleClassesPerFile();
        c3.Method1(0);
        var c4 = new Class04MultipleClassesPerFile();
        c4.Method1(0);
        var c5 = new Class05NestedClassSingleFile();
        c5.Method1(0);
        var c6 = new Class06NestedClassMultipleFiles();
        c6.Method1(0);
    }
}
