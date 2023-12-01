using Lox.Core;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Interpreters.Exceptions;
using Lox.Visitors.Resolvers;



namespace Lox.Visitors.Interpreters.Callables;

public class LoxFunction(FunctionStatement declaration, IReadOnlyList<ResolvedToken> resolvedParameters, ILoxEnvironment closure) : ILoxCallable
{
    private readonly FunctionStatement _declaration = declaration;
    private readonly IReadOnlyList<ResolvedToken> _resolvedArguments = resolvedParameters;
    private readonly ILoxEnvironment _closure = closure;

    public int Arity => _declaration.Parameters.Count;
    public object? Call(Interpreter interpreter, object?[] arguments)
    {
        var env = _closure.Push();
        foreach (var (name, val) in _resolvedArguments.Zip(arguments))
        {
            env.Define(name, val);
        }
        try
        {
            interpreter.ExecuteBlock(_declaration.Body, env);
        }
        catch (ReturnException ex)
        {
            return ex.Value;
        }
        return null;
    }
    public override string ToString() => $"<fn {(_declaration.Name.Type == TokenType.Fun ? $"$lambda{_declaration.Name.Line}_{_declaration.Name.Column}$" : _declaration.Name.Text)}>";
}