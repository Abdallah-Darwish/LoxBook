using Lox.Core;
namespace Lox.Visitors.Interpreters;


public class TypeMismatchException : RuntimeException
{
    public Type[] ExpectedTypes { get; }
    public Type ExpectedType => ExpectedTypes.First();
    public Type? ActualType { get; }
    public object? ActualValue { get; }
    public Expression SourceExpression { get; }
    public TypeMismatchException(object? actualValue, Expression source, params Type[] expectedTypes) :
        base($"Unexpected type({actualValue?.GetType().Name ?? "nil"}) expected types([{string.Join(", ", expectedTypes.Select(t => t.Name))}])")
    {
        ExpectedTypes = expectedTypes;
        ActualValue = actualValue;
        ActualType = actualValue?.GetType();
        SourceExpression = source;
    }
}