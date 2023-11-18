using Lox.Core;
using Lox.Visitors.Interpreters.Callables;

namespace Lox.Visitors.Interpreters.Environments;

public class LoxEnvironment : ILoxEnvironment
{
    private static LoxEnvironment BuildGlobalEnvironment()
    {
        LoxEnvironment global = new();
        global.TryDefine("clock", new Clock());
        global.TryDefine("typeof", new TypeOf());
        return global;
    }
    private static readonly Lazy<LoxEnvironment> s_globalEnvironment = new(BuildGlobalEnvironment, LazyThreadSafetyMode.ExecutionAndPublication);
    public static LoxEnvironment GlobalEnvironment => s_globalEnvironment.Value;
    private readonly LoxEnvironment? _enclosing;
    private readonly Dictionary<string, object?> _values = [];

    private readonly int _depth;

    public LoxEnvironment() : this(null) { }
    public LoxEnvironment(LoxEnvironment? enclosing)
    {
        _enclosing = enclosing;
        _depth = (_enclosing?._depth ?? -1) + 1;
    }

    private bool CanDefine(string name) => _depth <= 1 || (!_values.ContainsKey(name) && _enclosing!.CanDefine(name));

    public bool TryDefine(string name, object? value)
    {
        if (_values.ContainsKey(name) || !(_enclosing?.CanDefine(name) ?? true))
        {
            return false;
        }
        return _values.TryAdd(name, value);
    }

    public void Define(Token id, object? value)
    {
        if (!TryDefine(id.Lexeme!, value))
        {
            throw new DuplicateIdentifierException(id);
        }
    }
    public bool TryGet(string id, out object? value)
    {
        if (_values.TryGetValue(id, out value))
        {
            return true;
        }
        return _enclosing?.TryGet(id, out value) ?? false;
    }

    public object? Get(Token id)
    {
        if (!TryGet(id.Lexeme!, out var val))
        {
            throw new UndefinedIdentifierException(id);
        }
        if (val == Uninitialized.Instance)
        {
            throw new UninitializedIdentifierException(id);
        }
        return val;
    }
    public bool TrySet(string id, object? value)
    {
        if (_values.ContainsKey(id))
        {
            _values[id] = value;
            return true;
        }
        return _enclosing?.TrySet(id, value) ?? false;
    }
    public void Set(Token id, object? value)
    {
        if (!TrySet(id.Lexeme!, value))
        {
            throw new UndefinedIdentifierException(id);
        }
    }
}