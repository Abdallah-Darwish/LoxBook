using Lox.Visitors;

namespace Lox.Core;

public abstract record class Expression()
{
    public abstract T Accept<T>(IExpressionVisitor<T> visitor);
}

public record class TernaryExpression(Expression Condition, Expression Left, Expression Right) : Expression
{
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class GroupingExpression(Expression Expression) : Expression
{
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class LiteralExpression(Token Value) : Expression
{
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class UnaryExpression(Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class VariableExpression(Token Name) : Expression
{
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class AssignmentExpression(Token Name, Expression Value) : Expression
{
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}