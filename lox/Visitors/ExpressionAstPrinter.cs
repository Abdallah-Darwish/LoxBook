using System.Text;
using Lox.Core;

namespace Lox.Visitors;

public class ExpressionAstPrinter : IExpressionVisitor<string>
{
    const char Separator = ' ';
    private string Parenthesize(params object?[] ops)
    {
        StringBuilder res = new();
        res.Append('[');
        foreach (var op in ops.Where(a => a is not null))
        {
            switch (op)
            {
                case string txt:
                    res.Append(Separator).Append(txt);
                    break;
                case Token token:
                    res.Append(Separator).Append(token.Text);
                    break;
                case Expression expr:
                    res.Append(Separator).Append(expr.Accept(this));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ops));

            }
        }
        res.Append(Separator).Append(']');
        return res.ToString();
    }

    public string Visit(TernaryExpression e) => Parenthesize(e.Condition, "?", e.Left, ":", e.Right);

    public string Visit(BinaryExpression e) => Parenthesize(e.Left, e.Operator, e.Right);

    public string Visit(GroupingExpression e) => Parenthesize("(", e.Expression, ")");

    public string Visit(LiteralExpression e) => e.Value.Text;

    public string Visit(UnaryExpression e) => Parenthesize(e.Operator, e.Right);

    public string Visit(VariableExpression e) => e.Name.Text;

    public string Visit(AssignmentExpression e) => Parenthesize(e.Name, "=", e.Value);

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
        return Parenthesize([.. expr]);
    }
}