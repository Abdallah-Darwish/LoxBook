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

    public string Visit(Ternary e) => Parenthesize("ternary", expressions: new[] { e.Condition, e.Left, e.Right });

    public string Visit(Binary e) => Parenthesize(e.Operator.Text, expressions: new[] { e.Left, e.Right });

    public string Visit(Grouping e) => Parenthesize("group", expressions: new[] { e.Expression });

    public string Visit(Literal e) => e.Value.Text;

    public string Visit(Unary e) => Parenthesize(e.Operator.Text, expressions: new[] { e.Right });

    public string Visit(Variable e) => e.Name.Text;

}