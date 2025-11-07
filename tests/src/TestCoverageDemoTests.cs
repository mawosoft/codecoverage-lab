// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using Xunit;

namespace FooLib;

public class TestCoverageDemoTests
{
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1515 // Consider making public types internal
    public class DemoTheoryData : TheoryData<int, int>
#pragma warning restore CA1515 // Consider making public types internal
#pragma warning restore CA1034 // Nested types should not be visible
    {
        public DemoTheoryData()
        {
            Add(1, 1);
            Add(2, 2);
        }
    }

    public static readonly object[][] DemoMemberDataField =
    [
        [1, 1],
        [2, 2]
    ];

    public static IEnumerable<object[]> DemoMemberDataFunction(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return [i, i];
        }
    }

    [Theory]
    [ClassData(typeof(DemoTheoryData))]
    public void Test1(int p1, int p2)
    {
        Assert.Equal(p1, p2);
    }

    [Theory]
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(DemoMemberDataField))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    public void Test2(int p1, int p2)
    {
        Assert.Equal(p1, p2);
    }

    [Theory]
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(DemoMemberDataFunction), 2)]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    public void Test3(int p1, int p2)
    {
        Assert.Equal(p1, p2);
    }
}
