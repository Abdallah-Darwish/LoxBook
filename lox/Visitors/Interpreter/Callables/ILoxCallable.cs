namespace Lox.Visitors.Interpreters.Callables;
public interface ILoxCallable
{
    public int Arity { get; }
    object? Call(Interpreter interpreter, object?[] arguments);
}
