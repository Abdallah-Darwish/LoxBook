using Lox.Core;

namespace Lox.Visitors;

public interface IExpressionVisitor
{
    void Visit(TernaryExpression e);
    void Visit(BinaryExpression e);
    void Visit(GroupingExpression e);
    void Visit(LiteralExpression e);
    void Visit(UnaryExpression e);
    void Visit(VariableExpression e);
    void Visit(AssignmentExpression e);
    void Visit(CallExpression e);
    void Visit(LambdaExpression e);
    void Visit(GetExpression e);
    void Visit(SetExpression e);
    void Visit(ThisExpression e);
}
public interface IExpressionVisitor<T>
{
    T Visit(TernaryExpression e);
    T Visit(BinaryExpression e);
    T Visit(GroupingExpression e);
    T Visit(LiteralExpression e);
    T Visit(UnaryExpression e);
    T Visit(VariableExpression e);
    T Visit(AssignmentExpression e);
    T Visit(CallExpression e);
    T Visit(LambdaExpression e);
    T Visit(GetExpression e);
    T Visit(SetExpression e);
    T Visit(ThisExpression e);
}