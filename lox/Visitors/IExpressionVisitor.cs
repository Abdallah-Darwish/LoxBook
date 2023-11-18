using Lox.Core;

namespace Lox.Visitors;

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
}