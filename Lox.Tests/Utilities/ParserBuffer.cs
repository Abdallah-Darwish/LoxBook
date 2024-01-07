using Lox.Core;
using Lox.Visitors;

namespace Lox.Tests.Utilities;

public class ParserBuffer : IStatementVisitor
{
    private readonly List<Statement> _buffer = [];
    public IReadOnlyList<Statement> Buffer => _buffer;
    private void Push(Statement stmt) => _buffer.Add(stmt);

    public void Visit(ExpressionStatement s) => Push(s);
    public void Visit(PrintStatement s) => Push(s);
    public void Visit(VariableStatement s) => Push(s);
    public void Visit(BlockStatement s) => Push(s);
    public void Visit(IfStatement s) => Push(s);
    public void Visit(WhileStatement s) => Push(s);
    public void Visit(BreakStatement s) => Push(s);
    public void Visit(FunctionStatement s) => Push(s);
    public void Visit(ReturnStatement s) => Push(s);

    public void Visit(ClassStatement s) => Push(s);
}