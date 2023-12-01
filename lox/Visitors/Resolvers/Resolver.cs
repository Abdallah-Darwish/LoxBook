using Lox.Core;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Resolvers.Exceptions;

namespace Lox.Visitors.Resolvers;

public record class ResolvedToken(Token Token, int Index, int Depth);
public class Resolver : IStatementVisitor, IExpressionVisitor
{
    private record class ScopeVariable(int Index, int Depth)
    {
        public bool IsDefined { get; set; } = false;
    }

    public Resolver(IEnumerable<string> globals, IDictionary<Token, ResolvedToken> store)
    {
        _store = store;

        BeginScope();
        foreach (var name in globals)
        {
            var nameToken = Token.FromIdentifier(name);
            Declare(nameToken);
            Define(nameToken);
        }

        BeginScope();
    }

    private readonly IDictionary<Token, ResolvedToken> _store;
    private readonly Stack<List<string>> _scopes = [];
    private readonly Dictionary<string, Stack<ScopeVariable>> _cactusStack = [];
    private void BeginScope() => _scopes.Push([]);
    private void EndScope()
    {
        if (_scopes.Count == 1)
        {
            throw new InvalidOperationException("You can't end the global scope.");
        }

        var lastScope = _scopes.Pop();
        foreach (var scopeVar in lastScope)
        {
            _cactusStack[scopeVar].Pop();
        }
    }
    private int Depth => _scopes.Count - 1;

    private void Declare(Token name, bool isParam = false)
    {
        if (!_cactusStack.TryGetValue(name.Text, out var varStack))
        {
            _cactusStack.Add(name.Text, varStack = []);
        }
        if (!isParam && varStack.TryPeek(out var lastVar) && lastVar.Depth > 1) // Parameters can shadow blocks vars
        {
            throw new DuplicateIdentifierException(name);
        }

        var lastScope = _scopes.Peek();
        varStack.Push(new(lastScope.Count, Depth));
        lastScope.Add(name.Text);
    }


    private void Define(Token token)
    {
        _cactusStack[token.Text].Peek().IsDefined = true;
        Resolve(token);
    }

    private void Resolve(Token token)
    {
        _cactusStack.TryGetValue(token.Text, out var varScope);
        if (varScope is null)
        {
            throw new UndefinedIdentifierException(token);
        }

        varScope.TryPeek(out var localVar);
        if (localVar is null)
        {
            throw new UndefinedIdentifierException(token);
        }
        if (!localVar.IsDefined)
        {
            throw new ResolverException("You can't use a variable in its own initializer.", token);
        }

        _store.Add(token, new(token, localVar.Index, localVar.Depth));
    }
    /// <remarks>
    /// We can't go through <see cref="Declare"/> -> <see cref="Define"/> -> <see cref="Resolve"/> because it will check for duplicates.
    /// </remarks>
    private void DirectResolve(Token token) => _store.Add(token, new(token, _scopes.Peek().Count, Depth));

    public void Visit(ExpressionStatement s) => s.Expression.Accept(this);

    public void Visit(PrintStatement s) => s.Expression.Accept(this);

    public void Visit(VariableStatement s)
    {
        Declare(s.Name);
        s.Initializer?.Accept(this);
        Define(s.Name);
    }

    public void Visit(BlockStatement s)
    {
        BeginScope();
        try
        {
            foreach (var stmt in s.Statements)
            {
                stmt.Accept(this);
            }
        }
        finally
        {
            EndScope();
        }
    }

    public void Visit(IfStatement s)
    {
        s.Condition.Accept(this);
        s.Then.Accept(this);
        s.Else?.Accept(this);
    }

    public void Visit(WhileStatement s)
    {
        s.Condition.Accept(this);
        s.Body.Accept(this);
    }

    public void Visit(BreakStatement s) { return; }

    public void Visit(FunctionStatement s)
    {
        Declare(s.Name);
        Define(s.Name);
        BeginScope();
        try
        {
            foreach (var param in s.Parameters)
            {
                Declare(param, true);
                Define(param);
            }
            foreach (var stmt in s.Body)
            {
                stmt.Accept(this);
            }
        }
        finally
        {
            EndScope();
        }
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

    public void Visit(VariableExpression e) => Resolve(e.Name);

    public void Visit(AssignmentExpression e)
    {
        Resolve(e.Name);
        e.Value.Accept(this);
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
        DirectResolve(e.Fun);

        BeginScope();
        try
        {
            foreach (var param in e.Parameters)
            {
                Declare(param, true);
                Define(param);
            }
            foreach (var stmt in e.Body)
            {
                stmt.Accept(this);
            }
        }
        finally
        {
            EndScope();
        }
    }
}