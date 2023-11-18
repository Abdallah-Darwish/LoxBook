using Lox.Core;
using Lox.Visitors.Interpreters.Exceptions;

namespace Lox.Visitors.Interpreters.Environments;

public class UninitializedIdentifierException : RuntimeException
{
    public Token Id { get; }
    public UninitializedIdentifierException(Token id) : base($"Uninitialized identifier: {id.Lexeme}") => Id = id;
}