using Lox.Core;
using Lox.Parsers;
using Lox.Scanners;
using Lox.Utilities;
using Lox.Visitors;
using Lox.Visitors.Interpreters;
using Lox.Visitors.Interpreters.Environemnts;

namespace Lox.Tests;

public static class Utility
{
    public static IScanner MakeScanner(string source) => new Scanner(new StringReader(source));

    public static Token Scan(string source)
    {
        using var scanner = MakeScanner(source);
        scanner.MoveNext();
        return scanner.Current;
    }

    public static IParser MakeParser(string source) => new Parser(MakeScanner(source));

    public static Statement? Parse(string source)
    {
        using var parser = MakeParser(source);
        return parser.Parse();
    }

    public static string? ParseAsString(string source)
    {
        var stmt = Parse(source);
        if (stmt is null) { return null; }
        var printer = new StatementAstPrinter(new ExpressionAstPrinter());
        return stmt.Accept(printer);
    }

    public static string Interpret(string source)
    {
        using StringWriter sw = new();
        using var parser = MakeParser(source);
        ParserAdapter ad = new(parser, new IStatementVisitor[] { new Interpreter(new LoxEnvironment(), sw) });
        ad.Visit();
        return sw.ToString();
    }

}

