using System.Collections.Immutable;
using Lox.Core;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Resolvers;

namespace Lox.Visitors.Interpreters.Callables;

public class LoxClass : ILoxCallable
{
    public LoxClass(ClassStatement declaration, IReadOnlyDictionary<Token, ResolvedToken> resolverStore, ILoxEnvironment closure)
    {
        _declaration = declaration;
        var methodsClosure = closure.Push(); // We do this because the class will open a scope for it self
        _methods = declaration.Methods.ToDictionary(m => m.Name.Text, m => new LoxFunction(m, m.Parameters.Select(p => resolverStore[p]).ToArray(), methodsClosure));
        _methods.TryGetValue("init", out _initializer);
    }
    private readonly ClassStatement _declaration;
    private readonly IReadOnlyDictionary<string, LoxFunction> _methods;
    private readonly LoxFunction? _initializer;

    public string Name => _declaration.Name.Text;

    public int Arity => _initializer?.Arity ?? 0;

    public object? Call(Interpreter interpreter, object?[] arguments)
    {
        var instance = new LoxInstance(this);
        if (_initializer is not null)
        {
            var constructor = instance.Get("init") as ILoxCallable;
            constructor.Call(interpreter, arguments);
        }
        return instance;
    }

    public override string ToString() => Name;

    public LoxFunction? FindMethod(string name) => _methods.GetValueOrDefault(name);
}