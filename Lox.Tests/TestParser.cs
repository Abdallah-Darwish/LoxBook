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
}