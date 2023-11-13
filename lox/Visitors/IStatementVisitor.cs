using Lox.Core;

namespace Lox.Visitors;

public interface IStatementVisitor
{
    void Visit(ExpressionStatement s);
    void Visit(PrintStatement s);
    void Visit(VariableStatement s);
    void Visit(BlockStatement s);
}
public interface IStatementVisitor<T>
{
    T Visit(ExpressionStatement s);
    T Visit(PrintStatement s);
    T Visit(VariableStatement s);
    T Visit(BlockStatement s);
}