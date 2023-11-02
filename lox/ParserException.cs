namespace Lox;

public class ParserException : Exception
{
    public Token? Token { get; }

    public ParserException(string message, Token? token = null) : base(message)
    {
        Token = token;
    }
}