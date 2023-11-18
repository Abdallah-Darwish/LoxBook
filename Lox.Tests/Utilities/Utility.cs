using Lox.Core;
using Lox.Parsers;
using Lox.Scanners;
using Lox.Utilities;
using Lox.Visitors;
using Lox.Visitors.Interpreters;
using Lox.Visitors.Interpreters.Environments;

namespace Lox.Tests.Utilities;

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

    public static IReadOnlyList<object?> Interpret(string source)
    {
        var sync = new BufferOutputSync<object?>();
        using var parser = MakeParser(source);
        ParserAdapter ad = new(parser, new IStatementVisitor[] { new Interpreter(LoxEnvironment.GlobalEnvironment, sync) });
        ad.Visit();
        return sync.Buffer;
    }

    public static IReadOnlyList<T> Interpret<T>(string source)
    {
        var objList = Interpret(source);
        return objList.Cast<T>().ToArray();
    }

}

