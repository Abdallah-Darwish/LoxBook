using Lox.Core;
using Lox.Visitors.Interpreters.Exceptions;

namespace Lox.Visitors.Interpreters.Callables;

public class LoxInstance(LoxClass klass)
{
    private readonly LoxClass Klass { get; } = klass;

    private readonly Dictionary<string, object?> _fields = [];

    public override string ToString() => $"{Klass.Name} instance";

    public object? Get(string name, GetExpression? source = null)
    {
        if (!_fields.TryGetValue(name, out var val))
        {
            throw new UndefinedPropertyException(this, name, source);
        }
        return val;
    }
}