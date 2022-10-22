using Lox.Visitors;
namespace Lox.Expressions;
public abstract record class Expression()
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record class Binary(Expression Left, Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Grouping(Expression Expression) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Literal(object Value) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}

public record class Unary(Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}