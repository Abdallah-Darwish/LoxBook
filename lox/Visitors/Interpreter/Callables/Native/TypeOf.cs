namespace Lox.Visitors.Interpreters.Callables;

public class TypeOf : ILoxCallable
{
    public bool IsProperty { get; } = false;
    public int Arity { get; } = 1;

    public object? Call(Interpreter interpreter, object?[] arguments) => arguments[0] is null ? "$nil$" : arguments[0].GetType().Name;

    public override string ToString() => "<native fn typeof>";
}