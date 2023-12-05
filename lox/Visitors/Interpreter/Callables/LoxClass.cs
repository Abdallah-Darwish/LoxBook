using Lox.Core;

namespace Lox.Visitors.Interpreters.Callables;

public class LoxClass(ClassStatement declaration) : ILoxCallable
{
    private readonly ClassStatement _declaration = declaration;

    public string Name => _declaration.Name.Text;

    public int Arity => 0;

    public object? Call(Interpreter interpreter, object?[] arguments) => new LoxInstance(this);

    public override string ToString() => Name;
}