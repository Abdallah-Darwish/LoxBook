using Lox.Core;
using Lox.Visitors.Interpreters.Exceptions;

namespace Lox.Visitors.Interpreters.Callables;

public class LoxInstance(LoxClass klass)
{
    private readonly Dictionary<string, LoxFunction> _boundMethods = [];
    public LoxClass Klass { get; } = klass;

    private readonly Dictionary<string, object?> _fields = [];

    public override string ToString() => $"{Klass.Name} instance";

    public object? Get(string name, Interpreter interpreter, GetExpression? source = null)
    {
        if (_fields.TryGetValue(name, out var val))
        {
            return val;
        }
        if (!_boundMethods.TryGetValue(name, out var method))
        {
            method = Klass.FindMethod(name)?.Bind(this) ?? throw new UndefinedPropertyException(this, name, source);
            _boundMethods[name] = method;
        }
        if (method.IsProperty)
        {
            return method.Call(interpreter, []);
        }
        return method;
    }

    public void Set(string name, object? value) => _fields[name] = value;
}