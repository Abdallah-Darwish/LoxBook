using System.Collections.Immutable;
using Lox.Expressions;

namespace Lox.Visitors;

public class AstPrinter : IVisitor<string>
{
    private static readonly ImmutableDictionary<TokenType, string> TokenTextRepresentation = new Dictionary<TokenType, string>
    {
        [TokenType.And] = "and",
        [TokenType.Bang] = "!",
        [TokenType.BangEqual] = "!=",
        [TokenType.Class] = "class",
        [TokenType.Comma] = ",",
        [TokenType.Dot] = ".",
        [TokenType.Else] = "else",
        [TokenType.Equal] = "=",
        [TokenType.EqualEqual] = "==",
        [TokenType.False] = "false",
        [TokenType.For] = "for",
        [TokenType.Greater] = ">",
        [TokenType.GreaterEqual] = ">=",
        [TokenType.If] = "if",
        [TokenType.LeftBrace] = "{",
        [TokenType.LeftParentheses] = "(",
        [TokenType.Less] = "<",
        [TokenType.LessEqual] = "<=",
        [TokenType.Minus] = "-",
        [TokenType.Nil] = "nil",
        [TokenType.Or] = "or",
        [TokenType.Plus] = "+",
        [TokenType.Print] = "print",
        [TokenType.Return] = "return",
        [TokenType.RightBrace] = "}",
        [TokenType.RightParentheses] = "}",
        [TokenType.Semicolon] = ";",
        [TokenType.Slash] = "/",
        [TokenType.Star] = "*",
        [TokenType.Super] = "super",
        [TokenType.This] = "this",
        [TokenType.True] = "true",
        [TokenType.Var] = "var",
        [TokenType.While] = "while",
    }.ToImmutableDictionary();
    private static string ToText(Token token)
    {
        if (TokenTextRepresentation.TryGetValue(token.Type, out var txt)) { return txt; }
        return token.Lexeme!;
    }
    private string Parenthesize(string name, params Expression[] expressions) => $"({name} {string.Join(" ", expressions.Select(e => e.Accept(this)))})";
    public string Visit(Binary e) => Parenthesize(ToText(e.Operator), e.Left, e.Right);

    public string Visit(Grouping e) => Parenthesize("group", e.Expression);

    public string Visit(Literal e) => e.Value is null ? "nil" : e.Value.ToString()!;

    public string Visit(Unary e) => Parenthesize(ToText(e.Operator), e.Right);
}