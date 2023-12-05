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

public record class BlockStatement(IReadOnlyList<Statement> Statements) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class IfStatement(Expression Condition, Statement Then, Statement? Else) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class WhileStatement(Expression Condition, Statement Body) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class BreakStatement() : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class FunctionStatement(Token Name, IReadOnlyList<Token> Parameters, IReadOnlyList<Statement> Body) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class ReturnStatement(Token Return, Expression? Value) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}

public record class ClassStatement(Token Name, IReadOnlyList<FunctionStatement> Methods) : Statement
{
    public override void Accept(IStatementVisitor visitor) => visitor.Visit(this);
    public override T Accept<T>(IStatementVisitor<T> visitor) => visitor.Visit(this);
}