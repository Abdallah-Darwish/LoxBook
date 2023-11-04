using Lox.Expressions;
using Lox.Visitors;

namespace Lox.Statements;

public abstract record class Statement()
{
    public abstract T Accept<T>(IStatementVisitor<T> visitor);
}

public record class ExpressionStatement(Expression Expression) : Statement
{
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class Print(Expression Expression) : Statement
{
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}