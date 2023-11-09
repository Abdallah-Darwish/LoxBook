using System.Collections;
using Lox.Core;
using Lox.Visitors.Interpreters.Environemnts;
namespace Lox.Visitors.Interpreters;

public class Interpreter : IExpressionVisitor<object?>, IStatementVisitor
{
    private readonly TextWriter _output;
    private readonly ILoxEnvironment _environment;
    public Interpreter(ILoxEnvironment environment, TextWriter output)
    {
        _output = output;
        _environment = environment;
    }
    private static bool IsTruthy(object? obj)
    {
        if (obj is null) return false;
        if (obj is bool b) return b;
        return true;
    }
    private bool IsTruthy(Expression e) => IsTruthy(e.Accept(this));
    private double AsDouble(Expression e)
    {
        var right = e.Accept(this);
        if (right is not double dr)
        {
            throw new TypeMismatchException(right, e, typeof(double));
        }
        return dr;
    }
    private static IComparer GetComparer(Type t)
    {
        if (t == typeof(string)) { return StringComparer.Ordinal; }
        if (t == typeof(double)) { return Comparer<double>.Default; }
        throw new ArgumentException($"Unsupported Type: {t.Name}", nameof(t));
    }
    private int Compare(Expression left, Expression right)
    {
        object? leftValue = left.Accept(this), rightValue = right.Accept(this);
        if (leftValue is null && rightValue is null) { return 0; }
        if (leftValue is null) { return -1; }
        if (rightValue is null) { return 1; }

        if (leftValue.GetType() != rightValue.GetType())
        {
            throw new TypeMismatchException(rightValue, right, leftValue.GetType());
        }

        return GetComparer(leftValue.GetType()).Compare(leftValue, rightValue);
    }
    public object? Visit(TernaryExpression e) => IsTruthy(e.Condition) ? e.Left.Accept(this) : e.Right.Accept(this);

    public object? Visit(BinaryExpression e)
    {
        switch (e.Operator.Type)
        {
            case TokenType.Plus:
                {
                    var leftValue = e.Left.Accept(this);
                    if (leftValue is null || (leftValue is not double _ && leftValue is not string _))
                    {
                        throw new TypeMismatchException(leftValue, e.Left, typeof(double), typeof(string));
                    }
                    var rightValue = e.Right.Accept(this);
                    if (rightValue is null || (rightValue is not double _ && rightValue is not string _))
                    {
                        throw new TypeMismatchException(rightValue, e.Right, typeof(double), typeof(string));
                    }
                    if (leftValue is string _ || rightValue is string _)
                    {
                        return leftValue.ToString() + rightValue.ToString();
                    }
                    return (double)leftValue + (double)rightValue;
                }
            case TokenType.Minus:
                return AsDouble(e.Right) - AsDouble(e.Left);
            case TokenType.Star:
                return AsDouble(e.Right) * AsDouble(e.Right);
            case TokenType.Slash:
                {
                    var leftValue = AsDouble(e.Left);
                    var rightValue = AsDouble(e.Right);
                    if (rightValue - 0 == double.Epsilon)
                    {
                        throw new ZeroDivisionException(leftValue, rightValue, e);
                    }
                    return leftValue / rightValue;
                }
            case TokenType.EqualEqual:
                return Compare(e.Left, e.Right) == 0;
            case TokenType.BangEqual:
                return Compare(e.Left, e.Right) != 0;
            case TokenType.Less:
                return Compare(e.Left, e.Right) == -1;
            case TokenType.LessEqual:
                return Compare(e.Left, e.Right) <= 0;
            case TokenType.Greater:
                return Compare(e.Left, e.Right) == 1;
            case TokenType.GreaterEqual:
                return Compare(e.Left, e.Right) >= 0;
            case TokenType.Comma:
                e.Left.Accept(this);
                return e.Right.Accept(this);
            default:
                throw new InvalidOperationException($"Unsupported operator {e.Operator.Type} type for {nameof(BinaryExpression)}");
        };
    }

    public object? Visit(GroupingExpression e) => e.Accept(this);

    public object? Visit(LiteralExpression e) => e.Value.Value;

    public object? Visit(UnaryExpression e)
    {
        return e.Operator.Type switch
        {
            TokenType.Minus => -AsDouble(e.Right),
            TokenType.Bang => !IsTruthy(e.Right),
            _ => throw new InvalidOperationException($"Unsupported operator {e.Operator.Type} type for {nameof(UnaryExpression)}"),
        };
    }

    public void Visit(ExpressionStatement s) => s.Expression.Accept(this);

    public void Visit(PrintStatement s) => _output.WriteLine(s.Expression.Accept(this) ?? "$nil$");

    public void Visit(VariableStatement s)
    {
        _environment.Define(s.Name, s.Initializer?.Accept(this));
    }

    public object? Visit(VariableExpression e) => _environment.Get(e.Name);

    public object? Visit(AssignmentExpression e)
    {
        var val = e.Value.Accept(this);
        _environment.Set(e.Name, val);
        return val;
    }
}
