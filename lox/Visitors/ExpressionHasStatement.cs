using Lox.Core;

namespace Lox.Visitors;

public class ExpressionHasStatement : IExpressionVisitor<bool>
{
    public bool Visit(TernaryExpression e) => e.Condition.Accept(this) || e.Left.Accept(this) || e.Right.Accept(this);

    public bool Visit(BinaryExpression e) => e.Left.Accept(this) || e.Right.Accept(this);

    public bool Visit(GroupingExpression e) => e.Expression.Accept(this);

    public bool Visit(LiteralExpression e) => false;

    public bool Visit(UnaryExpression e) => e.Right.Accept(this);

    public bool Visit(VariableExpression e) => false;

    public bool Visit(AssignmentExpression e) => e.Value.Accept(this);

    public bool Visit(CallExpression e) => e.Callee.Accept(this) || e.Arguments.Any(a => a.Accept(this));

    public bool Visit(LambdaExpression e) => true;

    public bool Visit(GetExpression e) => e.Instance.Accept(this);

    public bool Visit(SetExpression e) => e.Instance.Accept(this) || e.Value.Accept(this);
}