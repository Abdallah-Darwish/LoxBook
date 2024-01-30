using System.Collections.Immutable;

namespace Lox.Core;

public record class Token(int Line, int Column, TokenType Type, string? Lexeme)
{
    private object? _value;
    public object? Value
    {
        get
        {
            if (Type != TokenType.Nil && _value is null)
            {
                _value = Type switch
                {
                    TokenType.True => true,
                    TokenType.False => false,
                    TokenType.String => Lexeme?[1..^1],
                    TokenType.Number => double.Parse(Lexeme),
                    TokenType.Identifier => Lexeme
                };
            }

            return _value;
        }
    }
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
        [TokenType.Break] = "break",
    }.ToImmutableDictionary();
    public string Text
    {
        get
        {
            if (Type == TokenType.Identifier) { return Lexeme!; } // Hot path optimization
            if (Type == TokenType.This) { return "this"; } // Hot path optimization
            if (TokenTextRepresentation.TryGetValue(Type, out var txt)) { return txt; }
            return Lexeme!;
        }
    }

    public static Token FromIdentifier(string name) => new(-1, -1, TokenType.Identifier, name);

    public static Token FromBool(bool val) => new(-1, -1, val ? TokenType.True : TokenType.False, null);

    public static Token This { get; } = new(-1, -1, TokenType.This, "this");
    public static Token Super { get; } = new(-1, -1, TokenType.Super, "super");
}