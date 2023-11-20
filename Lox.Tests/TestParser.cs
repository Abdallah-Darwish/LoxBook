using Lox.Parsers;
using Lox.Tests.Utilities;

namespace Lox.Tests;

public class TestParser
{
    [Fact]
    public void TestParseComma_CommaWithAssignment_AssignmentTakesPrecedence()
    {
        string source = """
1 == 2, x = 1 == 3, y = z;
""";
        var stmt = Utility.ParseAsString(source);

        Assert.Equal("{ [ [ [ 1 == 2 ] , [ x = [ 1 == 3 ] ] ] , [ y = z ] ] }", stmt);
    }

    [Fact]
    public void TestParseAssignment_AssignmentWithTernary_TernaryTakesPrecedence()
    {
        string source = """
x = 1 == 2 ? "HOW!" : "makes sense";
""";
        var stmt = Utility.ParseAsString(source);

        Assert.Equal("""{ [ x = [ [ 1 == 2 ] ? "HOW!" : "makes sense" ] ] }""", stmt);
    }

    [Fact]
    public void TestParseFor_WithAllParts_ShouldDesugarAsBlock()
    {
        string source = """
for(var x = 1; x < 10; x = x + 1)
    print x;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ block
    { var x = 1 }
    { while [ x < 10 ]
        { block
            { print x }
            { [ x = [ x + 1 ] ] }
        }
    }
}
""";

        Assert.Equal(expected, stmt);
    }
    [Fact]
    public void TestParseFor_MissingInitializer_ShouldDesugarAsWhile()
    {
        string source = """
for(; x < 10; x = x + 1)
    print x;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ while [ x < 10 ]
    { block
        { print x }
        { [ x = [ x + 1 ] ] }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseFor_MissingCondition_ShouldDesugarAsBlockWithTrue()
    {
        string source = """
for(var x = 1;; x = x + 1)
    print x;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ block
    { var x = 1 }
    { while true
        { block
            { print x }
            { [ x = [ x + 1 ] ] }
        }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseFor_MissingIterator_ShouldDesugarAsBlock()
    {
        string source = """
for(var x = 1; x < 505;)
    print x;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ block
    { var x = 1 }
    { while [ x < 505 ]
        { print x }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseBreak_NotInLoop_ShouldThrowParserException()
    {
        string source = """
break;
""";

        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("No enclosing loop out of which to break", ex.Message);
    }

    [Fact]
    public void TestParseBreak_InLoop_ShouldParseBreak()
    {
        string source = """
for(var i = "hehe"; i or 2;)
{
    if(i == 10)
    {
        if(i == 11)
        {
            print i;
            break;
        }
    }
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ block
    { var i = "hehe" }
    { while [ i or 2 ]
        { block
            { if [ i == 10 ]
                { block
                    { if [ i == 11 ]
                        { block
                            { print i }
                            { break }
                        }
                    }
                }
            }
        }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Theory]
    [InlineData("nil(1, 2);")]
    [InlineData("true(1, 2);")]
    [InlineData("\"hello\"(1, 2);")]
    public void ParseCall_CalleeIsLiteral_ThrowsException(string source)
    {
        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("Literals are not callable", ex.Message);
    }

    [Fact]
    public void ParseCall_NestedFunctionCalls_ArgsShouldBeOfTypeCall()
    {
        string source = """
min(max(1, 2), sqrt(1, -pow(5, 6), floor(10)), !gcd(505, 506, "hello", 1 + 2), clock());
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ [ min ( [ max ( 1 , 2 ) ] , [ sqrt ( 1 , [ - [ pow ( 5 , 6 ) ] ] , [ floor ( 10 ) ] ) ] , [ ! [ gcd ( 505 , 506 , "hello" , [ 1 + 2 ] ) ] ] , [ clock ( ) ] ) ] }
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void ParseCall_AggregateFunctionCalls_CalleeShouldBeCallExpression()
    {
        string source = """
min(1, 2)(3, 4)();
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ [ [ [ min ( 1 , 2 ) ] ( 3 , 4 ) ] ( ) ] }
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void ParseCall_AdditionalComma_ThrowsException()
    {
        string source = """
min(1,);
""";
        var ex = Assert.Throws<UnexpectedTokenException>(() => Utility.Parse(source));
        Assert.Contains("Expected a token of type LeftParentheses instead found token of type RightParentheses at Line", ex.Message);
        // This error is confusing but honestly I don't want to fix it and I want to focus on finishing the book
    }

    [Fact]
    public void ParseReturn_NotInsideFunction_ThrowsException()
    {
        string source = """
return 1;
""";
        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("No enclosing function out of which to return", ex.Message);
    }

    [Fact]
    public void ParseFunction_NormalFunction_ParsedSuccessfuly()
    {
        string source = """
fun greet(name)
{
    print "hello " + name;
    return "greeted " + name;
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ [ [ [ min ( 1 , 2 ) ] ( 3 , 4 ) ] ( ) ] }
""";

        Assert.Equal(expected, stmt);

    }
}
