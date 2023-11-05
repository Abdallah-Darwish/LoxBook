using Lox.Visitors;

namespace Lox.Core;

public abstract record class Expression()
{
public abstract T Accept<T>(IExpressionVisitor<T> visitor);
}

public record class Ternary(Expression Condition, Expression Left, Expression Right) : Expression
{
	public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class Binary(Expression Left, Token Operator, Expression Right) : Expression
{
	public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class Grouping(Expression Expression) : Expression
{
	public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class Literal(Token Value) : Expression
{
	public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class Unary(Token Operator, Expression Right) : Expression
{
	public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record class Variable(Token Name) : Expression
{
	public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}