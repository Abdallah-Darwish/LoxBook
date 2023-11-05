using Lox.Core;
namespace Lox.Visitors.Interpreters;

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
}