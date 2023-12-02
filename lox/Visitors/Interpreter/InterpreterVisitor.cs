using System.Collections;
using Lox.Core;
using Lox.Visitors.Interpreters.Callables;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Interpreters.Exceptions;
using Lox.Visitors.Resolvers;
namespace Lox.Visitors.Interpreters;

public class Interpreter(ILoxEnvironment? globals, IOutputSync<object?> outputSync, IReadOnlyDictionary<Token, ResolvedToken> resolverStore) : IExpressionVisitor<object?>, IStatementVisitor
{
    private readonly IOutputSync<object?> _outputSync = outputSync;
    private readonly IReadOnlyDictionary<Token, ResolvedToken> _resolverStore = resolverStore;
    private ILoxEnvironment _environment = globals?.Push() ?? new LoxEnvironment();

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
                return AsDouble(e.Left) - AsDouble(e.Right);
            case TokenType.Star:
                return AsDouble(e.Left) * AsDouble(e.Right);
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
            case TokenType.And:
                {
                    var lft = e.Left.Accept(this);
                    if (!IsTruthy(lft)) { return lft; }
                    return e.Right.Accept(this);
                }
            case TokenType.Or:
                {
                    var lft = e.Left.Accept(this);
                    if (IsTruthy(lft)) { return lft; }
                    return e.Right.Accept(this);
                }
            default:
                throw new InvalidOperationException($"Unsupported operator {e.Operator.Type} type for {nameof(BinaryExpression)}");
        };
    }

    public object? Visit(GroupingExpression e) => e.Expression.Accept(this);

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

    public void Visit(PrintStatement s) => _outputSync.Push(s.Expression.Accept(this));

    public void Visit(VariableStatement s)
    {
        var resolvedName = _resolverStore[s.Name];
        _environment.Define(resolvedName, Uninitialized.Instance);
        if (s.Initializer is not null)
        {
            _environment.Set(resolvedName, s.Initializer.Accept(this));
        }
    }

    public object? Visit(VariableExpression e) => _environment.Get(_resolverStore[e.Name]);

    public object? Visit(AssignmentExpression e)
    {
        var val = e.Value.Accept(this);
        _environment.Set(_resolverStore[e.Name], val);
        return val;
    }

    public void Visit(BlockStatement s) => ExecuteBlock(s.Statements, _environment.Push());

    internal void ExecuteBlock(IEnumerable<Statement> statements, ILoxEnvironment environment)
    {
        var prevEnv = _environment;
        try
        {
            _environment = environment;
            foreach (var stmt in statements)
            {
                stmt.Accept(this);
            }
        }
        finally { _environment = prevEnv; }
    }

    public void Visit(IfStatement s)
    {
        if (IsTruthy(s.Condition.Accept(this)))
        {
            s.Then.Accept(this);
        }
        else
        {
            s.Else?.Accept(this);
        }
    }

    public void Visit(WhileStatement s)
    {
        while (IsTruthy(s.Condition.Accept(this)))
        {
            try
            {
                s.Body.Accept(this);
            }
            catch (BreakException)
            {
                return;
            }
        }
    }

    public void Visit(BreakStatement s) => throw new BreakException(s);

    public object? Visit(CallExpression e)
    {
        var callee = e.Callee.Accept(this);
        if (callee is not ILoxCallable loxCallee)
        {
            throw new CallableExpectedException(e.RightParentheses);
        }
        if (e.Arguments.Length != loxCallee.Arity)
        {
            throw new ArgumentCountMismatchException(e.Arguments.Length, loxCallee);
        }
        var args = e.Arguments.Select(a => a.Accept(this)).ToArray();
        return loxCallee.Call(this, args);
    }

    public void Visit(FunctionStatement s) => _environment.Define(_resolverStore[s.Name], new LoxFunction(s, s.Parameters.Select(p => _resolverStore[p]).ToArray(), _environment));

    public void Visit(ReturnStatement s) => throw new ReturnException(s.Value?.Accept(this), s);

    public object? Visit(LambdaExpression e)
    {
        new FunctionStatement(e.Fun, e.Parameters, e.Body).Accept(this);
        return _environment.Get(_resolverStore[e.Fun]);
    }
}
