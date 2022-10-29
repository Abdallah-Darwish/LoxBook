using Lox.Expressions;

namespace Lox.Visitors;
public class ReversePolishNotationPrinter : IVisitor<string>
{
    public string Visit(Binary e) => $"{e.Left.Accept(this)} {e.Right.Accept(this)} {e.Operator.Text}";

    public string Visit(Grouping e) => e.Expression.Accept(this);

    public string Visit(Literal e) => e.Value.Text;

    public string Visit(Unary e) => $"{e.Right.Accept(this)} {e.Operator.Text}";
}