using Lox.Core;
using Lox.Parsers;
using Lox.Scanners;
using Lox.Visitors;
using System;

namespace Lox.Tests;

public class TestParser
{
    private static IParser MakeParser(string source) => new Parser(new Scanner(new StringReader(source)));

    private static Statement? Parse(string source)
    {
        using var parser = MakeParser(source);
        return parser.Parse();
    }

    private static string? ParseAsString(string source)
    {
        var stmt = Parse(source);
        if(stmt is null) { return null; }
        var printer = new StatementAstPrinter(new ExpressionAstPrinter());
        return stmt.Accept(printer);
    }

    [Fact]
    public void TestParseComma_CommaWithAssignment_AssignmentTakesPrecedence()
    {
        string source = """
1 == 2, x = 1 == 3, y = z;
""";
        var stmt = ParseAsString(source);


        Assert.Equal("{ [ [ [ 1 == 2 ] , [ x = [ 1 == 3 ] ] ] , [ y = z ] ] }", stmt);
    }

    [Fact]
    public void TestParseAssignment_AssignmentWithTernary_TernaryTakesPrecedence()
    {
        string source = """
x = 1 == 2 ? "HOW!" : "makes sense";
""";
        var stmt = ParseAsString(source);


        Assert.Equal("""{ [ x = [ [ 1 == 2 ] ? "HOW!" : "makes sense" ] ] }""", stmt);
    }
}