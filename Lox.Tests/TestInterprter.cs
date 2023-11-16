using Lox.Tests.Utilities;
using Lox.Visitors.Interpreters.Environemnts;

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

        var expected = new double[] { 1, 2, 1 };

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

        var expected = new double[] { 0, 1, 2, 3, 4 };

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

        var expected = new double[] { 1, 2, 1, 3, 2, 1, 4, 3, 2, 1 };

        Assert.Equal(expected, Utility.Interpret<double>(source));
    }
}