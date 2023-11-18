using Lox.Core;
namespace Lox.Visitors.Interpreters.Exceptions;


public class TypeMismatchException(object? actualValue, Expression source, params Type[] expectedTypes) :
    RuntimeException($"Unexpected type({actualValue?.GetType().Name ?? "nil"}) expected types([{string.Join(", ", expectedTypes.Select(t => t.Name))}])")
{
    public Type[] ExpectedTypes { get; } = expectedTypes;
    public Type ExpectedType => ExpectedTypes.First();
    public Type? ActualType { get; } = actualValue?.GetType();
    public object? ActualValue { get; } = actualValue;
    public Expression SourceExpression { get; } = source;
}