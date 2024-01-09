using Lox.Core;

namespace Lox.Visitors.Interpreters.Exceptions;

public class ClassExpectedException(Token sourceToken) : RuntimeException("You can only inherit from classes.")
{
    public Token SourceToken { get; } = sourceToken;
}