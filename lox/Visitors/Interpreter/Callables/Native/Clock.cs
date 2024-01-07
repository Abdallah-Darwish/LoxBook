namespace Lox.Visitors.Interpreters.Callables;

public class Clock : ILoxCallable
{
    public bool IsProperty { get; } = false;
    public int Arity { get; } = 0;

    public object? Call(Interpreter interpreter, object?[] arguments) => (double)DateTime.UtcNow.Ticks;

    public override string ToString() => "<native fn clock>";
}