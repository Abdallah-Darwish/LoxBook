using Lox.Core;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Interpreters.Exceptions;
using Lox.Visitors.Resolvers;

namespace Lox.Visitors.Interpreters.Callables;

public class LoxFunction(FunctionStatement declaration, IReadOnlyList<ResolvedToken> resolvedParameters, ILoxEnvironment closure) : ILoxCallable
{
    private LoxFunction(FunctionStatement declaration, IReadOnlyList<ResolvedToken> resolvedParameters, ILoxEnvironment closure, LoxInstance instance) : this(declaration, resolvedParameters, closure)
    {
        _instance = instance;
        IsInitializer = _instance is not null && _declaration.Name.Text == "init" && declaration.Type != FunctionType.Property;
    }
    private readonly FunctionStatement _declaration = declaration;
    private readonly IReadOnlyList<ResolvedToken> _resolvedArguments = resolvedParameters;
    private readonly ILoxEnvironment _closure = closure;
    private readonly LoxInstance? _instance;

    public int Arity => _declaration.Parameters.Count;
    public bool IsInitializer { get; }
    public bool IsProperty => _declaration.Type == FunctionType.Property;
    public object? Call(Interpreter interpreter, object?[] arguments)
    {
        var env = _closure.Push();
        foreach (var (name, val) in _resolvedArguments.Zip(arguments))
        {
            env.Define(name, val);
        }
        if (_instance is not null)
        {
            env.Define(new(Token.This, env.Count, env.Depth), _instance);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, env);
        }
        catch (ReturnException ex)
        {
            return IsInitializer ? _instance : ex.Value;
        }
        return IsInitializer ? _instance : null;
    }

    public LoxFunction Bind(LoxInstance instance) => new(_declaration, _resolvedArguments, _closure, instance);
    public override string ToString() => $"<fn {(_declaration.Name.Type == TokenType.Fun ? $"$lambda{_declaration.Name.Line}_{_declaration.Name.Column}$" : _declaration.Name.Text)}>";
}