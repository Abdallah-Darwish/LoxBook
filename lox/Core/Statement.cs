using Lox.Visitors;

namespace Lox.Core;

public abstract record class Statement()
{
    public abstract void Accept(IStatementVisitor visitor);
    public abstract T Accept<T>(IStatementVisitor<T> visitor);
}

public record class ExpressionStatement(Expression Expression) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class PrintStatement(Expression Expression) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class VariableStatement(Token Name, Expression? Initializer) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}