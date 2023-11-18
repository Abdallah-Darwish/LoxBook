using Lox.Core;
namespace Lox.Visitors.Interpreters.Exceptions;

public class ZeroDivisionException(double left, double right, Expression source) : RuntimeException($"Invalid division by Zero: {left} / {right}")
{
    public double Left { get; } = left;
    public double Right { get; } = right;
    public Expression SourceExpression { get; } = source;
}