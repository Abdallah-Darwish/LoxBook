using Lox.Core;

namespace Lox.Visitors;
public class ReversePolishNotationPrinter : IExpressionVisitor<string>
{
    public string Visit(TernaryExpression e) => $"{e.Condition.Accept(this)} {e.Left.Accept(this)} {e.Right.Accept(this)} ?";

    public string Visit(BinaryExpression e) => $"{e.Left.Accept(this)} {e.Right.Accept(this)} {e.Operator.Text}";

    public string Visit(GroupingExpression e) => e.Expression.Accept(this);

    public string Visit(LiteralExpression e) => e.Value.Text;

    public string Visit(UnaryExpression e) => $"{e.Right.Accept(this)} {(e.Operator.Text == "-" ? "~" : e.Operator.Text)}";

    public string Visit(VariableExpression e) => e.Name.Text;
}