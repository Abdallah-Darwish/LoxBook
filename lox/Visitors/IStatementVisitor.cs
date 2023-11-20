using Lox.Core;

namespace Lox.Visitors;

public interface IStatementVisitor
{
    void Visit(ExpressionStatement s);
    void Visit(PrintStatement s);
    void Visit(VariableStatement s);
    void Visit(BlockStatement s);
    void Visit(IfStatement s);
    void Visit(WhileStatement s);
    void Visit(BreakStatement s);
    void Visit(FunctionStatement s);
    void Visit(ReturnStatement s);
}
public interface IStatementVisitor<T>
{
    T Visit(ExpressionStatement s);
    T Visit(PrintStatement s);
    T Visit(VariableStatement s);
    T Visit(BlockStatement s);
    T Visit(IfStatement s);
    T Visit(WhileStatement s);
    T Visit(BreakStatement s);
    T Visit(FunctionStatement s);
    T Visit(ReturnStatement s);
}