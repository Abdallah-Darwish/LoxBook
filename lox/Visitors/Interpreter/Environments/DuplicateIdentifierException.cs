using Lox.Core;
using Lox.Visitors.Interpreters.Exceptions;

namespace Lox.Visitors.Interpreters.Environments;

public class DuplicateIdentifierException : RuntimeException
{
    public Token Id { get; }
    public DuplicateIdentifierException(Token id) : base($"Duplicate identifier definition: {id.Lexeme}") => Id = id;
}