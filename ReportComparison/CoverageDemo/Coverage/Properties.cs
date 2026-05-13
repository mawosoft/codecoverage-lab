// Copyright (c) Matthias Wolf, Mawosoft.

using System.Diagnostics.CodeAnalysis;

namespace CoverageDemo.Coverage;

// Property accessor coverage.

// - 'init' is just a 'set' accessor with extra metadata.
// - Auto and full properties are the same from an instrumentation perspective.
// - Indexers (this[]) are just properties with parameters.

public class Properties
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new Properties();
            _ = c.GetSet1; c.GetSet1 = true;
            _ = c.GetOnly1;
            c.SetOnly1 = true;
            _ = c.GetSet2; c.GetSet2 = true;
            _ = c.GetOnly2;
            c.SetOnly2 = true;
        }
    }

    public MyBool GetSet1 { get; set; }
    public MyBool GetOnly1 { get; set; }
    public MyBool SetOnly1 { get; set; }
    public MyBool GetSet2
    {
        get;
        set;
    }
    public MyBool GetOnly2
    {
        get;
        set;
    }
    public MyBool SetOnly2
    {
        get;
        set;
    }
}
