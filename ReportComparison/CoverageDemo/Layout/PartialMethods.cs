// Copyright (c) Matthias Wolf, Mawosoft.

namespace CoverageDemo.Layout;

// Classes in the CoverageDemo.Layout namespace are mostly intended to demonstrate
// how ReportGenerator handles these class layouts.

public partial class PartialMethods
{
    partial void DeclaredOnly();
    public partial int PartialMethod(MyInt value);
}

public partial class PartialMethods
{
    public partial int PartialMethod(MyInt value)
    {
        DeclaredOnly();
        return value;
    }
}
