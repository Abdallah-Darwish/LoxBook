using Lox.Core;

namespace Lox.Visitors.Interpreters.Exceptions;

public class CallableExpectedException(Token sourceToken) : RuntimeException("You can only call functions and classes.")
{
    public Token SourceToken { get; } = sourceToken;
}