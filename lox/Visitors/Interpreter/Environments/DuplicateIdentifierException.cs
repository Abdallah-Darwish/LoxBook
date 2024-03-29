using Lox.Core;
using Lox.Visitors.Interpreters.Exceptions;

namespace Lox.Visitors.Interpreters.Environments;

public class DuplicateIdentifierException(Token id) : RuntimeException($"Duplicate identifier definition: {id.Lexeme} at ({id.Line}, {id.Column})")
{
    public Token Id { get; } = id;
}