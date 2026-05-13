// Copyright (c) Matthias Wolf, Mawosoft.

public class GlobalClass
{
    public int Method1(CoverageDemo.MyInt value) => value;
}

namespace CoverageDemo.Naming
{
    namespace SubNamespace
    {
        public class SomeClass
        {
            public int Method1(MyInt value) => value;
        }
    }

    // HolderClass has no coverable code itself and can be mistaken for a namespace
    // if the coverage report does not distinguish nested types.
    public sealed class HolderClass
    {
        public class Inner
        {
            public int Method1(MyInt value) => value;
        }
    }
}
