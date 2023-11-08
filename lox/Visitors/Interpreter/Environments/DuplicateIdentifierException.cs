using Lox.Core;

namespace Lox.Visitors.Interpreters.Environemnts;

public class DuplicateIdentifierException : RuntimeException
{
    public Token Id { get; }
    public DuplicateIdentifierException(Token id) : base($"Duplicate identifier definition: {id.Lexeme}") => Id = id;
}