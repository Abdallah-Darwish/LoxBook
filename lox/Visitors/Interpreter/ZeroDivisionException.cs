using Expression = Lox.Expressions.Expression;
namespace Lox.Visitors.Interpreters;

[Serializable]
public class ZeroDivisionException : RuntimeException
{
    public double Left { get; }
    public double Right { get; }
    public Expression SourceExpression { get; }
    public ZeroDivisionException(double left, double right, Expression source) :
        base($"Invalid division by Zero: {left} / {right}")
    {
        Left = left;
        Right = right;
        SourceExpression = source;
    }
    protected ZeroDivisionException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}