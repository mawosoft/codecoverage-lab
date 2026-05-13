// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo.Layout;

// Classes in the CoverageDemo.Layout namespace are mostly intended to demonstrate
// how ReportGenerator handles these class layouts.

public class MultiClassSingleFile
{
    public int Method1(MyInt value) => value;
}

public class AnotherClassSameFile
{
    public int Method1(MyInt value) => value;
}
