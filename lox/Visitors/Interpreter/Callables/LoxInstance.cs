using Lox.Core;
using Lox.Visitors.Interpreters.Exceptions;

namespace Lox.Visitors.Interpreters.Callables;

public class LoxInstance(LoxClass klass)
{
    private readonly Dictionary<string, LoxFunction> _boundMethods = [];
    public LoxClass Klass { get; } = klass;

    private readonly Dictionary<string, object?> _fields = [];

    public override string ToString() => $"{Klass.Name} instance";
    private static string GetMethodBindingName(string name, LoxClass klass) => $"{klass.Name}${name}";
    public LoxFunction BindMethod(string name, LoxClass? klass = null, GetExpression? source = null)
    {
        klass ??= Klass;
        var bindingName = GetMethodBindingName(name, klass);

        if (!_boundMethods.TryGetValue(bindingName, out var method))
        {
            _boundMethods[bindingName] = method = klass.FindMethod(name)?.Bind(this) ?? throw new UndefinedPropertyException(this, name, source);
        }
        return method;
    }

    public object? Get(string name, Interpreter interpreter, GetExpression? source = null)
    {
        if (_fields.TryGetValue(name, out var val))
        {
            return val;
        }

        var method = BindMethod(name, source: source);
        if (method.IsProperty)
        {
            return method.Call(interpreter, []);
        }
        return method;
    }

    public void Set(string name, object? value) => _fields[name] = value;
}