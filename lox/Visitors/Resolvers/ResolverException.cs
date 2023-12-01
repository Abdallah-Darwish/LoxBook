using Lox.Core;
namespace Lox.Visitors.Resolvers.Exceptions;

public class ResolverException(string message, Token? sourceToken = null) : LoxException(message)
{
    public Token? SourceToken { get; } = sourceToken;
}