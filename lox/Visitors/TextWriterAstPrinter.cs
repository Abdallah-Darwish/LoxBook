using Lox.Core;

namespace Lox.Visitors;

public class TextWriterAstPrinter(TextWriter output, IStatementVisitor<string> astPrinter) : IStatementVisitor
{
    private readonly TextWriter _output = output;
    private readonly IStatementVisitor<string> _astPrinter = astPrinter;

    private void Print(Statement s) => _output.WriteLine(s.Accept(_astPrinter));
    public void Visit(ExpressionStatement s) => Print(s);

    public void Visit(PrintStatement s) => Print(s);

    public void Visit(VariableStatement s) => Print(s);

    public void Visit(BlockStatement s) => Print(s);

    public void Visit(IfStatement s) => Print(s);

    public void Visit(WhileStatement s) => Print(s);

    public void Visit(BreakStatement s) => Print(s);

    public void Visit(FunctionStatement s) => Print(s);

    public void Visit(ReturnStatement s) => Print(s);
}