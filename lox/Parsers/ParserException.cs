using Lox.Core;

namespace Lox.Parsers;

public class ParserException(string message, Token? token = null) : LoxException(message)
{
    public Token? Token { get; } = token;
}