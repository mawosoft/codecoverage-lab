// Copyright (c) Matthias Wolf, Mawosoft.

using CoverageDemo.Coverage;
using Xunit;

namespace CoverageDemo;

public class CoverageTests
{
    [Fact]
    public void TestAttributes()
    {
        Attributes.Run();
    }

    [Fact]
    public void TestExceptionBlocks()
    {
        ExceptionBlocks.Run();
    }

    [Fact]
    public void TestGenerics()
    {
        Generics.Run();
    }

    [Fact]
    public void TestIfConditions()
    {
        IfConditions.Run();
    }

    [Fact]
    public void TestLambdas()
    {
        Lambdas.Run();
    }

    [Fact]
    public void TestLoopConditions()
    {
        LoopConditions.Run();
    }

    [Fact]
    public void TestMultiStatementLines()
    {
        MultiStatementLines.Run();
    }

    [Fact]
    public void TestNullConditions()
    {
        NullConditions.Run();
    }

    [Fact]
    public void TestProperties()
    {
        Properties.Run();
    }

    [Fact]
    public void TestStateMachines()
    {
        StateMachines.Run();
    }

    [Fact]
    public void TestSwitchConditions()
    {
        SwitchConditions.Run();
    }
}
