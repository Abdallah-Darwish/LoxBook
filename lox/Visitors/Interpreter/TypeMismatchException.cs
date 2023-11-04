using Expression = Lox.Expressions.Expression;
namespace Lox.Visitors.Interpreters;

[Serializable]
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
    protected TypeMismatchException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}