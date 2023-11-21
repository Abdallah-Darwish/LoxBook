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

    public static IReadOnlyList<Statement> Parse(string source)
    {
        ParserBuffer buffer = new();
        using var parser = MakeParser(source);
        ParserAdapter ad = new(parser, [buffer]);
        ad.Visit();
        return buffer.Buffer;
    }

    public static string ParseAsString(string source)
    {
        using var parser = MakeParser(source);
        using var sw = new StringWriter();
        var printer = new TextWriterAstPrinter(sw, new AstPrinter(new ExpressionHasStatement()));
        ParserAdapter ad = new(parser, [printer]);
        ad.Visit();
        sw.Flush();
        return sw.ToString().TrimEnd();
    }

    public static IReadOnlyList<object?> Interpret(string source)
    {
        var sync = new BufferOutputSync<object?>();
        using var parser = MakeParser(source);
        ParserAdapter ad = new(parser, [new Interpreter(LoxEnvironment.GlobalEnvironment, sync)]);
        ad.Visit();
        return sync.Buffer;
    }

    public static IReadOnlyList<T> Interpret<T>(string source)
    {
        var objList = Interpret(source);
        return objList.Cast<T>().ToArray();
    }

}

