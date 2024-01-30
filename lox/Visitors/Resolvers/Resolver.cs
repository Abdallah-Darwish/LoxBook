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

    private record class Scope(List<string> Names, Statement? Definition);

    public Resolver(IEnumerable<string> globals, IDictionary<Token, ResolvedToken> store)
    {
        _store = store;

        BeginScope(); // Start predfined globals scope
        foreach (var name in globals)
        {
            var nameToken = Token.FromIdentifier(name);
            Declare(nameToken);
            Define(nameToken);
        }

        BeginScope(); // Start user globals scope
    }

    private readonly IDictionary<Token, ResolvedToken> _store;
    private readonly Stack<Scope> _scopes = [];
    private readonly Dictionary<string, Stack<ScopeVariable>> _cactusStack = [];
    private void BeginScope(Statement? definition = null) => _scopes.Push(new([], definition));
    private void EndScope(bool checkVariableUsage, Statement? definition = null)
    {
        if (_scopes.Count == 1)
        {
            throw new InvalidOperationException("You can't end the global scope.");
        }

        var lastScope = _scopes.Pop();
        if (!ReferenceEquals(lastScope.Definition, definition))
        {
            throw new InvalidOperationException("Popped scope definiton != pushed scope definition");
        }
        foreach (var scopeVar in lastScope.Names)
        {
            var cactusVar = _cactusStack[scopeVar].Pop();
            if (checkVariableUsage && !cactusVar.IsUsed)
            {
                throw new UnusedVariableException(cactusVar.Token);
            }
        }
    }
    private bool IsInClass => _scopes.Peek().Definition is ClassStatement;
    private int Depth => _scopes.Count - 1;

    /// <param name="isParam">
    /// If <see langword="true/> it will allow this name to shadow any previously defined names else it will allow it to shadow names with depth less than 2.
    /// </param>
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
        varStack.Push(new(name, lastScope.Names.Count, Depth));
        lastScope.Names.Add(name.Text);
    }


    private void Define(Token name, bool markUsed = false)
    {
        _cactusStack[name.Text].Peek().IsDefined = true;
        Resolve(name, markUsed);
    }

    private ScopeVariable GetVariable(Token name)
    {
        _cactusStack.TryGetValue(name.Text, out var varStack);
        if (varStack is null)
        {
            throw new UndefinedIdentifierException(name);
        }

        varStack.TryPeek(out var localVar);
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
    private void DirectResolve(Token name) => _store.Add(name, new(name, _scopes.Peek().Names.Count, Depth));

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
        BeginScope(s);

        bool threwException = false;
        try
        {
            foreach (var stmt in s.Statements)
            {
                stmt.Accept(this);
            }
        }
        catch
        {
            threwException = true;
            throw;
        }
        finally { EndScope(!threwException, s); }
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
        bool isInClass = IsInClass;
        BeginScope(s);

        bool threwException = false;
        try
        {
            foreach (var param in s.Parameters)
            {
                Declare(param, true);
                Define(param);
            }
            if (isInClass)
            {
                var thisToken = Token.This with { Line = s.Name.Line, Column = s.Name.Column };
                Declare(thisToken, true);
                Define(thisToken, true);
            }
            foreach (var stmt in s.Body)
            {
                stmt.Accept(this);
            }
        }
        catch
        {
            threwException = true;
            throw;
        }
        finally { EndScope(!threwException, s); }
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

        var threwException = false;
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
        catch
        {
            threwException = true;
            throw;
        }
        finally { EndScope(!threwException); }
    }

    public void Visit(ClassStatement s)
    {
        Declare(s.Name);
        Define(s.Name);

        if (s.Super is not null)
        {
            Resolve(s.Super, true);
        }

        BeginScope(s);

        try
        {
            if (s.Super is not null)
            {
                var superToken = Token.This with { Line = s.Super.Line, Column = s.Super.Column };
                Declare(superToken, true);
                Define(superToken, true);
            }
            foreach (var meth in s.Methods)
            {
                meth.Accept(this);
            }
        }
        finally { EndScope(false, s); }
    }

    public void Visit(GetExpression e) => e.Instance.Accept(this);

    public void Visit(SetExpression e)
    {
        e.Instance.Accept(this);
        e.Value.Accept(this);
    }

    public void Visit(ThisExpression e) => Resolve(e.This, true);

    public void Visit(SuperExpression e) => Resolve(e.Super, true);
}