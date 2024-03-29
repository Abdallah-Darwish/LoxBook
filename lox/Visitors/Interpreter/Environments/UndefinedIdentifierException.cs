using Lox.Core;
using Lox.Visitors.Interpreters.Exceptions;

namespace Lox.Visitors.Interpreters.Environments;

public class UndefinedIdentifierException : RuntimeException
{
    public Token Id { get; }
    public UndefinedIdentifierException(Token id) : base($"Undefined identifier: {id.Text} at ({id.Line}, {id.Column})") => Id = id;
}