using Lox.Core;

namespace Lox.Visitors.Interpreters.Environemnts;

public class UninitializedIdentifierException : RuntimeException
{
    public Token Id { get; }
    public UninitializedIdentifierException(Token id) : base($"Uninitialized identifier: {id.Lexeme}") => Id = id;
}