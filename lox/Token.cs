using System.Collections.Immutable;

namespace Lox;

public record class Token(int Line, int Column, TokenType Type, string? Lexeme)
{
    public T GetValue<T>()
    {
        var type = typeof(T);
        return Type switch
        {
            TokenType.String when type != typeof(string) || Lexeme is null => throw new InvalidCastException(
                $"Can't convert {this} value to {type}"),
            TokenType.String => (T)(object)Lexeme[1..^1],
            TokenType.Number when type != typeof(double) || Lexeme is null => throw new InvalidCastException(
                $"Can't convert {this} value to {type}"),
            TokenType.Number => (T)(object)double.Parse(Lexeme),
            _ => throw new ArgumentOutOfRangeException(nameof(T), type,
                "This method only supports: [string, double] types")
        };
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
    }.ToImmutableDictionary();
    public string Text
    {
        get
        {
            if (TokenTextRepresentation.TryGetValue(Type, out var txt)) { return txt; }
            return Lexeme!;
        }
    }
}