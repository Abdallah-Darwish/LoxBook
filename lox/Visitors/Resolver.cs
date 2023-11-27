using Lox.Core;

namespace Lox.Visitors;
public class ResolvedToken(Token token)
{
    public Token Token { get; } = token;
    public override int GetHashCode() => Token.GetHashCode();
    public override bool Equals(object? obj)
    {
        if (obj is ResolvedToken tk)
        {
            return Token == tk.Token;
        }
        return false;
    }

}
public class Resolver : IStatementVisitor, IExpressionVisitor
{
    private readonly Stack<Dictionary<string, bool>> _scopes = [];
    private void BeginScope() => _scopes.Push([]);
    private void EndScope() => _scopes.Pop();
    public void Visit(ExpressionStatement s)
    {
        throw new NotImplementedException();
    }

    public void Visit(PrintStatement s) => s.Expression.Accept(this);

    public void Visit(VariableStatement s)
    {
        throw new NotImplementedException();
    }

    public void Visit(BlockStatement s)
    {
        BeginScope();
        try
        {
            foreach (var ss in s.Statements)
            {
                ss.Accept(this);
            }
        }
        finally
        {
            EndScope();
        }
    }

    public void Visit(IfStatement s)
    {
        throw new NotImplementedException();
    }

    public void Visit(WhileStatement s)
    {
        throw new NotImplementedException();
    }

    public void Visit(BreakStatement s) { return; }

    public void Visit(FunctionStatement s)
    {
        throw new NotImplementedException();
    }

    public void Visit(ReturnStatement s) => s.Value?.Accept(this);

    public void Visit(TernaryExpression e)
    {
        e.Condition.Accept(this);
        e.Left.Accept(this);
        e.Right.Accept(this);
    }

    public void Visit(BinaryExpression e)
    {
        e.Left.Accept(this);
        e.Right.Accept(this);
    }

    public void Visit(GroupingExpression e) => e.Expression.Accept(this);

    public void Visit(LiteralExpression e) { return; }

    public void Visit(UnaryExpression e) => e.Right.Accept(this);

    public void Visit(VariableExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(AssignmentExpression e)
    {
        throw new NotImplementedException();
    }

    public void Visit(CallExpression e)
    {
        e.Callee.Accept(this);
        foreach (var arg in e.Arguments)
        {
            arg.Accept(this);
        }
    }

    public void Visit(LambdaExpression e)
    {
        throw new NotImplementedException();
    }
}