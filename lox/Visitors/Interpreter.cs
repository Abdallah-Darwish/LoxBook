using Lox.Expressions;

namespace Lox.Visitors;

public class Interpreter : IVisitor<object>
{
    private bool IsTruthy(object? obj) {
        if (obj is null) return false;
        if (obj is bool b) return b;
        return true;
    }
    public object Visit(Ternary e)
    {
        throw new NotImplementedException();
    }

    public object Visit(Binary e)
    {
        throw new NotImplementedException();
    }

    public object Visit(Grouping e) => e.Accept(this);

    public object Visit(Literal e) => e.Value.Value;

    public object Visit(Unary e)
    {
        var right = e.Accept(this);
        if (e.Operator.Type == TokenType.Minus)
        {
            return -(double)right;
        }
        if(e.Operator.Type == TokenType.Bang)
        {
            return !IsTruthy(e.Accept(this));
        }
        throw new 
    }
}