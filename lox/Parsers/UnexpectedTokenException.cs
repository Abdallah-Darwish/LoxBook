using Lox.Core;

namespace Lox.Parsers;

public class UnexpectedTokenException : ParserException
{
    public TokenType ExpectedTokenType { get; }
    public UnexpectedTokenException(Token token, TokenType expectedTokenType)
        : base($"Expected a token of type {expectedTokenType} instead found token of type {token.Type}", token) { }
}