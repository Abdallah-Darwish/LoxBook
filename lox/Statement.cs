using Lox.Expressions;
using Lox.Visitors;

namespace Lox.Statements;

public abstract record class Statement()
{
    public abstract void Accept(IVisitor visitor);
}

public record class ExpressionStatement(Expression Expression) : Statement
{
    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}

public record class Print(Expression Expression) : Statement
{
    public override void Accept(IVisitor visitor) => visitor.Visit(this);
}