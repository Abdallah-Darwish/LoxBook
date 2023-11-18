using Lox.Core;
using Lox.Visitors.Interpreters.Callables;

namespace Lox.Visitors.Interpreters.Exceptions;

public class ArgumentCountMismatchException(int actualCount, ILoxCallable callee) : RuntimeException($"Expected {callee.Arity} arguments but got {actualCount}.")
{
    public ILoxCallable Callee { get; } = callee;
    public int ActualCount { get; } = actualCount;
}