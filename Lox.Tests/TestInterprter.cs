using Lox.Tests.Utilities;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Interpreters.Exceptions;

namespace Lox.Tests;

public class TestInterprter
{
    [Fact]
    public void TestVisitVariableStatement_RedefineGlobalVariable_CanRedefine()
    {
        string source = """
var x = 1;
print x;
{
    var x = 2;
    print x;
}
print x;
""";
        double[] expected = [1, 2, 1];

        Assert.Equal(expected, Utility.Interpret<double>(source));
    }

    [Fact]
    public void TestVisitVariableStatement_RedefineLocalVariable_CantRedefine()
    {
        string source = """
{
    var x = 1;
    {
        var x = 2;
    }
}
""";
        var ex = Assert.Throws<DuplicateIdentifierException>(() => Utility.Interpret(source));
        Assert.Equal("x", ex.Id.Text);
    }

    [Fact]
    public void TestVisitVariableExpression_AccessUninitializedVariable_CantAccess()
    {
        string source = """
var x;
print x;
""";

        var ex = Assert.Throws<UninitializedIdentifierException>(() => Utility.Interpret(source));
        Assert.Equal("x", ex.Id.Text);
    }

    [Fact]
    public void TestVisitWhile_VisitDesugaredFor_WorksFine()
    {
        string source = """
for(var i = 0; i < 5; i = i + 1)
    print i;
""";

        double[] expected = [0, 1, 2, 3, 4];

        Assert.Equal(expected, Utility.Interpret<double>(source));
    }

    [Fact]
    public void TestVisitBreak_NestedLoop_BreakInnerMostLoopOnly()
    {
        string source = """
for(var i = 0; i < 5; i = i + 1)
{
    var j = i;
    while(true)
    {
        if(j <= 0)
        {
            break;
        }
        print j;
        j = j - 1;
    }
}
""";

        double[] expected = [1, 2, 1, 3, 2, 1, 4, 3, 2, 1];

        Assert.Equal(expected, Utility.Interpret<double>(source));
    }

    [Fact]
    public void TestVisitCall_CallNativeFunction_ShouldCallFine()
    {
        string source = """
print typeof(nil);
""";

        string[] expected = ["$nil$"];

        Assert.Equal(expected, Utility.Interpret<string>(source));
    }

    [Fact]
    public void TestVisitCall_IncorrectArgumentCount_ShouldThrowException()
    {
        string source = """
typeof(1, 2, 3);
""";

        var ex = Assert.Throws<ArgumentCountMismatchException>(() => Utility.Interpret(source));
        Assert.Equal(3, ex.ActualCount);
        Assert.Equal("<native fn typeof>", ex.Callee.ToString());
    }

    [Fact]
    public void TestVisitCall_CalleeIsNotCallable_ShouldThrowException()
    {
        string source = """
typeof(1)("hello");
""";

        var ex = Assert.Throws<CallableExpectedException>(() => Utility.Interpret(source));
        Assert.Contains("You can only call functions and classes", ex.Message);
    }
}