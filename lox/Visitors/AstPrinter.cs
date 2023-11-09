using System.Text;
using Lox.Core;

namespace Lox.Visitors;

public class AstPrinter : IExpressionVisitor<string>
{
    private string Parenthesize(string name, bool isStatement = false, Expression[]? expressions = null, Token[]? tokens = null)
    {
        StringBuilder sb = new();
        sb.Append(isStatement ? '[' : '{').Append(name).Append(isStatement ? ']' : '}');
        if (expressions is not null && expressions.Length > 0)
        {
            foreach (var e in expressions)
            {
                sb.Append(' ').Append(e.Accept(this));
            }
        }
        if (tokens is not null && tokens.Length > 0)
        {
            foreach (var t in tokens)
            {
                sb.Append(' ').Append(t.Text);
            }
        }
        return sb.ToString();
    }

    public string Visit(TernaryExpression e) => Parenthesize("ternary", expressions: new[] { e.Condition, e.Left, e.Right });

    public string Visit(BinaryExpression e) => Parenthesize(e.Operator.Text, expressions: new[] { e.Left, e.Right });

    public string Visit(GroupingExpression e) => Parenthesize("group", expressions: new[] { e.Expression });

    public string Visit(LiteralExpression e) => e.Value.Text;

    public string Visit(UnaryExpression e) => Parenthesize(e.Operator.Text, expressions: new[] { e.Right });

    public string Visit(VariableExpression e) => e.Name.Text;

}