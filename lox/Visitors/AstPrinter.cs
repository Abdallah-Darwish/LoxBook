using System.Collections.Immutable;
using Lox.Expressions;

namespace Lox.Visitors;

public class AstPrinter : IVisitor<string>
{
    private string Parenthesize(string name, params Expression[] expressions) => $"({name} {string.Join(" ", expressions.Select(e => e.Accept(this)))})";
    public string Visit(Binary e) => Parenthesize(e.Operator.Text, e.Left, e.Right);

    public string Visit(Grouping e) => Parenthesize("group", e.Expression);

    public string Visit(Literal e) => e.Value is null ? "nil" : e.Value.ToString()!;

    public string Visit(Unary e) => Parenthesize(e.Operator.Text, e.Right);
}