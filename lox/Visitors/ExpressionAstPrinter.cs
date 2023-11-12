using System.Text;
using Lox.Core;

namespace Lox.Visitors;

public class ExpressionAstPrinter : IExpressionVisitor<string>
{
    private string Parenthesize(string name, IEnumerable<Expression?>? expressions = null, IEnumerable<Token?>? tokens = null)
    {
        StringBuilder sb = new();
        sb.Append("[ ").Append(name);
        if (expressions is not null)
        {
            foreach (var e in expressions)
            {
                if (e is null) { continue; }
                sb.Append(' ').Append(e.Accept(this));
            }
        }
        if (tokens is not null)
        {
            foreach (var t in tokens)
            {
                if (t is null) { continue; }
                sb.Append(' ').Append(t.Text);
            }
        }
        return sb.Append(" ]").ToString();
    }

    public string Visit(TernaryExpression e) => Parenthesize("ternary", expressions: new[] { e.Condition, e.Left, e.Right });

    public string Visit(BinaryExpression e) => Parenthesize(e.Operator.Text, expressions: new[] { e.Left, e.Right });

    public string Visit(GroupingExpression e) => Parenthesize("group", expressions: new[] { e.Expression });

    public string Visit(LiteralExpression e) => e.Value.Text;

    public string Visit(UnaryExpression e) => Parenthesize(e.Operator.Text, expressions: new[] { e.Right });

    public string Visit(VariableExpression e) => e.Name.Text;

    public string Visit(AssignmentExpression e) => Parenthesize("=", tokens: new[] { e.Name }, expressions: new[] { e.Value });
}