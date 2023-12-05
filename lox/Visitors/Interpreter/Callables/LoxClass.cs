using Lox.Core;

namespace Lox.Visitors.Interpreters.Callables;

public class LoxClass : ILoxCallable
{
    public LoxClass(ClassStatement declaration)
    {
        _declaration = declaration;
        _methods = declaration.Methods.ToDictionary(m => m.Name.Text);

    }
    private readonly ClassStatement _declaration;
    private readonly IReadOnlyDictionary<string, FunctionStatement> _methods;

    public string Name => _declaration.Name.Text;

    public int Arity => 0;

    public object? Call(Interpreter interpreter, object?[] arguments) => new LoxInstance(this);

    public override string ToString() => Name;

    public FunctionStatement? FindMethod(string name) => _methods.GetValueOrDefault(name);
}