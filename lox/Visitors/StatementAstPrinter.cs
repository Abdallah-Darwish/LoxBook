using Lox.Core;
using System.Text;

namespace Lox.Visitors;

public class StatementAstPrinter(IExpressionVisitor<string> expressionPrinter, IExpressionVisitor<bool> expressionHasStatementVisitor) : IStatementVisitor<string>
{
    const string IdentationChar = "    ";
    private readonly IExpressionVisitor<string> _expressionPrinter = expressionPrinter;
    private readonly IExpressionVisitor<bool> _expressionHasStatementVisitor = expressionHasStatementVisitor;
    private string _indentation = string.Empty;
    public string Push() => _indentation += IdentationChar;
    public string Pop() => _indentation = _indentation[..^IdentationChar.Length];

    private string Parenthesize(params object?[] ops)
    {
        StringBuilder res = new();
        string sep = _indentation;
        res.Append(sep).Append('{');
        sep = " ";
        var push = ops.Any(op => op is Statement) || ops.OfType<Expression>().Any(e => e.Accept(_expressionHasStatementVisitor));
        if (push)
        {
            Push();
        }
        foreach (var op in ops?.Where(a => a is not null) ?? Enumerable.Empty<object>())
        {
            switch (op)
            {
                case string txt:
                    res.Append(sep).Append(txt);
                    sep = " ";
                    break;
                case Token token:
                    res.Append(sep).Append(token.Text);
                    sep = " ";
                    break;
                case Expression expr:
                    var exprTxt = expr.Accept(_expressionPrinter);
                    res.Append(sep).Append(exprTxt);
                    if (exprTxt.Contains('\n'))
                    {
                        if (res[^1] != '\n')
                        {
                            res.AppendLine();
                        }
                        sep = _indentation;
                    }
                    else
                    {
                        sep = " ";
                    }
                    break;
                case Statement stmt:
                    if (res[^1] != '\n')
                    {
                        res.AppendLine();
                    }
                    res.AppendLine(stmt.Accept(this));
                    sep = _indentation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ops));

            }
        }
        if (push)
        {
            bool setSep = sep == _indentation;
            Pop();
            if (setSep) { sep = _indentation; }
        }
        res.Append(sep).Append('}');
        return res.ToString();
    }

    public string Visit(ExpressionStatement s) => Parenthesize(s.Expression);

    public string Visit(PrintStatement s) => Parenthesize("print", s.Expression);

    public string Visit(VariableStatement s) => Parenthesize("var", s.Name, "=", s.Initializer);

    public string Visit(BlockStatement s) => Parenthesize(s.Statements.Cast<object>().Prepend("block").ToArray());

    public string Visit(IfStatement s) => Parenthesize("if", s.Condition, s.Then, s.Else);

    public string Visit(WhileStatement s) => Parenthesize("while", s.Condition, s.Body);

    public string Visit(BreakStatement s) => Parenthesize("break");

    public string Visit(FunctionStatement s) => Parenthesize(["fun", s.Name, "(", .. s.Parameters, ")", .. s.Body]);

    public string Visit(ReturnStatement s) => Parenthesize(s.Return, s.Value);
}

public class ConsoleStatementAstPrinter(IStatementVisitor<string> statementPrinter) : IStatementVisitor
{
    private readonly IStatementVisitor<string> _statementPrinter = statementPrinter;

    private void Print(Statement s) => Console.WriteLine(s.Accept(_statementPrinter));
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