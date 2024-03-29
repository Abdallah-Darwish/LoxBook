using Lox.Visitors;

namespace Lox.Core;

public abstract record class Expression()
{
    public abstract void Accept(IExpressionVisitor visitor);
    public abstract T Accept<T>(IExpressionVisitor<T> visitor);
}

public record class TernaryExpression(Expression Condition, Expression Left, Expression Right) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class GroupingExpression(Expression Expression) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class LiteralExpression(Token Value) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class UnaryExpression(Token Operator, Expression Right) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class VariableExpression(Token Name) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class AssignmentExpression(Token Name, Expression Value) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class CallExpression(Expression Callee, Token RightParentheses, Expression[] Arguments) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class LambdaExpression(Token Fun, IReadOnlyList<Token> Parameters, IReadOnlyList<Statement> Body) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class GetExpression(Expression Instance, Token Name) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class SetExpression(Expression Instance, Token Name, Expression Value) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class ThisExpression(Token This) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class SuperExpression(Token Super, Token Name) : Expression
{
    public override void Accept(IExpressionVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}