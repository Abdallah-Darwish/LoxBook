using Lox.Core;

namespace Lox.Visitors.Interpreters.Environemnts;

public class UndefinedIdentifierException : RuntimeException
{
    public Token Id { get; }
    public UndefinedIdentifierException(Token id) : base($"Undefined identifier: {id.Lexeme}") => Id = id;
}