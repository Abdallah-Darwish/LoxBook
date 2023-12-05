using Lox.Core;
namespace Lox.Visitors.Interpreters.Exceptions;


public class ObjectExpectedException(object? actualValue, Expression source) :
    RuntimeException($"Expected an object after '.' operator instead found a value of type: {actualValue?.GetType().Name ?? "$nil$"}")
{
    public Type? ActualType { get; } = actualValue?.GetType();
    public object? ActualValue { get; } = actualValue;
    public Expression SourceExpression { get; } = source;
}