using Lox.Core;
using System.Text;

namespace Lox.Visitors;

public class AstPrinter(IExpressionVisitor<bool> expressionHasStatementVisitor) : IExpressionVisitor<string>, IStatementVisitor<string>
{
    private readonly IExpressionVisitor<bool> _expressionHasStatementVisitor = expressionHasStatementVisitor;
    const string IdentationChar = "    ";
    private string _indentation = string.Empty;
    public string Push() => _indentation += IdentationChar;
    public string Pop() => _indentation = _indentation[..^IdentationChar.Length];

    private string Parenthesize(bool isExpression, params object?[] ops)
    {
        StringBuilder res = new();
        string sep = isExpression ? string.Empty : _indentation;
        res.Append(sep).Append(isExpression ? '[' : '{');
        sep = " ";
        var push = ops.Any(op => op is Statement) || ops.Where(e => e is Expression).Any(e => ((Expression)e!).Accept(_expressionHasStatementVisitor));
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
                    var exprTxt = expr.Accept(this);
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
        res.Append(sep).Append(isExpression ? ']' : '}');
        return res.ToString();
    }
    public string Visit(TernaryExpression e) => Parenthesize(true, e.Condition, "?", e.Left, ":", e.Right);

    public string Visit(BinaryExpression e) => Parenthesize(true, e.Left, e.Operator, e.Right);

    public string Visit(GroupingExpression e) => Parenthesize(true, "(", e.Expression, ")");

    public string Visit(LiteralExpression e) => e.Value.Text;

    public string Visit(UnaryExpression e) => Parenthesize(true, e.Operator, e.Right);

    public string Visit(VariableExpression e) => e.Name.Text;

    public string Visit(AssignmentExpression e) => Parenthesize(true, e.Name, "=", e.Value);

    public string Visit(CallExpression e)
    {
        List<object?> expr = [e.Callee, "("];
        foreach (var arg in e.Arguments)
        {
            if (expr.Count > 2)
            {
                expr.Add(",");
            }
            expr.Add(arg);
        }
        expr.Add(")");
        return Parenthesize(true, [.. expr]);
    }

    public string Visit(LambdaExpression e) => Parenthesize(true, ["fun", "(", .. e.Parameters, ")", .. e.Body]);


    public string Visit(ExpressionStatement s) => Parenthesize(false, s.Expression);

    public string Visit(PrintStatement s) => Parenthesize(false, "print", s.Expression);

    public string Visit(VariableStatement s) => Parenthesize(false, "var", s.Name, "=", s.Initializer);

    public string Visit(BlockStatement s) => Parenthesize(false, s.Statements.Cast<object>().Prepend("block").ToArray());

    public string Visit(IfStatement s) => Parenthesize(false, "if", s.Condition, s.Then, s.Else);

    public string Visit(WhileStatement s) => Parenthesize(false, "while", s.Condition, s.Body);

    public string Visit(BreakStatement s) => Parenthesize(false, "break");

    public string Visit(FunctionStatement s) => Parenthesize(false, ["fun", s.Name, "(", .. s.Parameters, ")", .. s.Body]);

    public string Visit(ReturnStatement s) => Parenthesize(false, s.Return, s.Value);
}
