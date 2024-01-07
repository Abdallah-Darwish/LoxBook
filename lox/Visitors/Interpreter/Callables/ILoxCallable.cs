using Lox.Core;

namespace Lox.Visitors.Interpreters.Callables;
public interface ILoxCallable
{
    bool IsProperty { get; }
    int Arity { get; }
    object? Call(Interpreter interpreter, object?[] arguments);
}
