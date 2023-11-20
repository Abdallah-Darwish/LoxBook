using Lox.Core;
namespace Lox.Visitors.Interpreters.Exceptions;

public class ReturnException(object? value, ReturnStatement sourceStatement) : RuntimeException
{
    public ReturnStatement SourceStatement { get; } = sourceStatement;
    public object? Value { get; } = value;
}