using Lox.Core;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Resolvers.Exceptions;

namespace Lox.Visitors.Resolvers;

public record class ResolvedToken(Token Token, int Index, int Depth);
public class Resolver : IStatementVisitor, IExpressionVisitor
{
    private record class ScopeVariable(Token Token, int Index, int Depth)
    {
        public bool IsDefined { get; set; } = false;
        public bool IsUsed { get; set; } = false;
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
            var cactusVar = _cactusStack[scopeVar].Pop();
            if (!cactusVar.IsUsed)
            {
                throw new UnusedVariableException(cactusVar.Token);
            }
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
        varStack.Push(new(name, lastScope.Count, Depth));
        lastScope.Add(name.Text);
    }


    private void Define(Token name)
    {
        _cactusStack[name.Text].Peek().IsDefined = true;
        Resolve(name, false);
    }

    private ScopeVariable GetVariable(Token name)
    {
        _cactusStack.TryGetValue(name.Text, out var varScope);
        if (varScope is null)
        {
            throw new UndefinedIdentifierException(name);
        }

        varScope.TryPeek(out var localVar);
        return localVar ?? throw new UndefinedIdentifierException(name);
    }

    private void UnuseVariable(Token name) => GetVariable(name).IsUsed = false;

    private void Resolve(Token name, bool markUsed)
    {
        var localVar = GetVariable(name);
        if (!localVar.IsDefined)
        {
            throw new ResolverException("You can't use a variable in its own initializer.", name);
        }
        if (markUsed)
        {
            localVar.IsUsed = true;
        }

        _store.Add(name, new(name, localVar.Index, localVar.Depth));
    }
    /// <remarks>
    /// We can't go through <see cref="Declare"/> -> <see cref="Define"/> -> <see cref="Resolve"/> because it will check for duplicates.
    /// </remarks>
    private void DirectResolve(Token name) => _store.Add(name, new(name, _scopes.Peek().Count, Depth));

    public void Visit(ExpressionStatement s) => s.Expression.Accept(this);

    public void Visit(PrintStatement s) => s.Expression.Accept(this);

    public void Visit(VariableStatement s)
    {
        Declare(s.Name);
        if (s.Initializer is not null)
        {
            s.Initializer.Accept(this);
            UnuseVariable(s.Name);
        }
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

    public void Visit(VariableExpression e) => Resolve(e.Name, true);

    public void Visit(AssignmentExpression e)
    {
        Resolve(e.Name, true);
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

    public void Visit(ClassStatement s)
    {
        Declare(s.Name);
        Define(s.Name);

        foreach (var meth in s.Methods)
        {
            meth.Accept(this);
        }
    }

    public void Visit(GetExpression e) => e.Instance.Accept(this);
}