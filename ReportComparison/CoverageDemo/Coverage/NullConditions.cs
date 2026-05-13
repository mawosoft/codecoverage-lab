// Copyright (c) Matthias Wolf, Mawosoft.

using System.Diagnostics.CodeAnalysis;

namespace CoverageDemo.Coverage;

// Null conditionals coverage.

public class NullConditions
{
    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new NullConditions();
            _ = c.NullCoalescingUnary(default, default, "");
            _ = c.NullCoalescingUnary("", default, "");
            _ = c.NullCoalescingBinary(default, default, "");
            _ = c.NullCoalescingBinary("", default, "");
            _ = c.NullConditional(null, null, c);
            _ = c.NullConditional(c, null, c);
            c.NullConditionalAssignment(null, null, c);
            c.NullConditionalAssignment(c, null, c);
            var c4 = new NullConditions();
            var c3 = new NullConditions { Next = c4 };
            var c2 = new NullConditions { Next = c3 };
            var c1 = new NullConditions { Next = c2 };
            _ = c.ChainedNullConditionals(c1, c2, c3, c4);
        }
    }

    public NullConditions? Next { get; set; }
    public MyString Value { get; set; }

    public string NullCoalescingUnary(MyString value1, MyString value2, MyString value3)
    {
        string? v1 = value1.Value;
        string? v2 = value2.Value;
        string? v3 = value3.Value;
        // null/!null
        v1 ??= "";
        // Always null
        v2 ??= "";
        // Always !null
        v3 ??= "";
        return v1 + v2 + v3;
    }

    public string NullCoalescingBinary(MyString value1, MyString value2, MyString value3)
    {
        // null/!null
        string v1 = value1.Value ?? "";
        // Always null
        string v2 = value2.Value ?? "";
        // Always !null
        string v3 = value3.Value ?? "";
        return v1 + v2 + v3;
    }

    public string NullConditional(NullConditions? obj1, NullConditions? obj2, NullConditions? obj3)
    {
        // null/!null
        string? v1 = obj1?.Value;
        // Always null
        string? v2 = obj2?.Value;
        // Always !null
        string? v3 = obj3?.Value;
        return v1 + v2 + v3;
    }

    public void NullConditionalAssignment(NullConditions? obj1, NullConditions? obj2, NullConditions? obj3)
    {
        // null/!null
        obj1?.Value = "";
        // Always null
        obj2?.Value = "";
        // Always !null
        obj3?.Value = "";
    }

    public (NullConditions?, NullConditions?, NullConditions?, NullConditions?) ChainedNullConditionals(
        NullConditions? obj1,
        NullConditions? obj2,
        NullConditions? obj3,
        NullConditions? obj4)
    {
        // !null.!null.!null.!null.last
        NullConditions? o1 = obj1?.Next?.Next?.Next?.Next;
        // !null.!null.!null.null
        NullConditions? o2 = obj2?.Next?.Next?.Next?.Next;
        // !null.!null.null
        NullConditions? o3 = obj3?.Next?.Next?.Next?.Next;
        // !null.null
        NullConditions? o4 = obj4?.Next?.Next?.Next?.Next;
        return (o1, o2, o3, o4);
    }
}
