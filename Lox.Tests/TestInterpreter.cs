using Lox.Tests.Utilities;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Interpreters.Exceptions;
using Lox.Visitors.Resolvers.Exceptions;

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
    print x;
    {
        var x = 2;
        print x;
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

    [Fact]
    public void TestVisitCallAndFunction_FunctionHaveSameVariableNamesAsOuterScope_FunctionVariablesShadowOuterVariables()
    {
        string source = """
fun Greet(name)
{
    var userName = "error";
    fun BuildMessage(userName)
    {
        return "Hello " + userName;
    }
    userName = userName + "hehe";
    print "Greeting " + name;
    return BuildMessage(name);
}
print Greet("Abdallah");
""";

        string[] expected = ["Greeting Abdallah", "Hello Abdallah"];

        Assert.Equal(expected, Utility.Interpret<string>(source));
    }

    [Fact]
    public void TestVisitLambda_DefineAndCallLambdaInPlace_CanBeCalled()
    {
        string source = """
print fun(name)
{
    return fun(userName)
    {
        return "Hello " + userName;
    }(name);
}("Abdallah");
""";

        string[] expected = ["Hello Abdallah"];

        Assert.Equal(expected, Utility.Interpret<string>(source));
    }

    [Fact]
    public void TestVisitLambda_AssignLambdaToVariable_CanCallVariable()
    {
        string source = """
var fn = fun(name)
{
    return "Hello " + name;
};
print fn("Abdallah");
print fn;
""";

        var result = Utility.InterpretToString(source);
        string[] expected = ["Hello Abdallah", "<fn $lambda0_9$>"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TestVisitFunction_FunctionParametersHaveSimilarNameToOuterScopeVars_WillShadowOuterScope()
    {
        string source = """
fun fn1(a, b)
{
    fun fn2(a, b)
    {
        fun fn3(a, b)
        {
            return a + 3 + b + 3;
        }
        return fn3(a + 2, b + 2);
    }
    return fn2(a + 1, b + 1);
}
print fn1(1, 2);
""";

        var result = Utility.Interpret<double>(source);
        double[] expected = [15];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TestVisitBlock_UnusedVariable_ShouldThrowException()
    {
        string source = """
{
    var x = 1;
    var y = 2;
    print x;
}
""";

        var ex = Assert.Throws<UnusedVariableException>(() => Utility.Interpret(source));
        Assert.Equal("y", ex.SourceToken.Text);
    }

    [Fact]
    public void TestVisitClass_ClassDefinition_ShouldParseClass()
    {
        string source = """
class MetroBoomin
{
    Hello()
    {
        print "hello there";
    }
}
print MetroBoomin;
""";

        var result = Utility.InterpretToString(source);
        string[] expected = ["MetroBoomin"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TestVisitClass_InstantiateClass_WouldReturnInstance()
    {
        string source = """
class Arctic
{
    Sing()
    {
        print "I am goint back to 505";
    }
}
var instance = Arctic();
print instance;
""";

        var result = Utility.InterpretToString(source);
        string[] expected = ["Arctic instance"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TestVisitCall_CallMethod_WouldCallMethodAndBindThis()
    {
        string source = """
class Counter
{

    SetValue(val)
    {
        this._value = val;
    }

    Increment(addition)
    {
        this._value = this._value + addition;
        return this._value;
    }
}
var cnt = Counter();
cnt.SetValue(0);
print cnt.Increment(1);
print cnt.Increment(2);
print cnt.Increment(3);
""";

        var result = Utility.Interpret<double>(source);
        double[] expected = [1, 3, 6];

        Assert.Equal(expected, result);
    }
}