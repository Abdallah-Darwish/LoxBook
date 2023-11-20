using Lox.Core;
using System.Text;

namespace Lox.Visitors;

public class StatementAstPrinter : IStatementVisitor<string>
{
    const string IdentationChar = "    ";
    private readonly IExpressionVisitor<string> _expressionPrinter;
    private string _indentation = string.Empty;
    public StatementAstPrinter(IExpressionVisitor<string> expressionPrinter) => _expressionPrinter = expressionPrinter;
    public string Push() => _indentation += IdentationChar;
    public string Pop() => _indentation = _indentation[..^IdentationChar.Length];

    private string Parenthesize(params object?[] ops)
    {
        StringBuilder res = new();
        string sep = _indentation;
        res.Append(sep).Append('{');
        sep = " ";
        var push = ops.Any(op => op is Statement);
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
                    res.Append(sep).Append(expr.Accept(_expressionPrinter));
                    sep = " ";
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

public class ConsoleStatementAstPrinter : IStatementVisitor
{
    private readonly IStatementVisitor<string> _statementPrinter;
    public ConsoleStatementAstPrinter(IStatementVisitor<string> statementPrinter) => _statementPrinter = statementPrinter;
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