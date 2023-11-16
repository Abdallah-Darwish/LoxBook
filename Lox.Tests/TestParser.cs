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
}
