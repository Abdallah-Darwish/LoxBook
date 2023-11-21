using Lox.Core;

namespace Lox.Parsers;

public class UnexpectedTokenException(Token token, TokenType expectedTokenType, string? calrifyingMessage = null) : ParserException($"Expected a token of type {expectedTokenType}{(calrifyingMessage is null ? string.Empty : $" after {calrifyingMessage}")} instead found token of type {token.Type} at Line {token.Line}, Column: {token.Column}", token)
{
    public TokenType ExpectedTokenType { get; } = expectedTokenType;
}