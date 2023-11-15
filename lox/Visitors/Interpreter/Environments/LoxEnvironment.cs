using Lox.Core;

namespace Lox.Visitors.Interpreters.Environemnts;

public class LoxEnvironment : ILoxEnvironment
{
    private readonly LoxEnvironment? _enclosing;
    private readonly Dictionary<string, object?> _values = new();
    private bool IsGlobal => _enclosing is null;

    public LoxEnvironment() : this(null) { }
    public LoxEnvironment(LoxEnvironment? enclosing) => _enclosing = enclosing;

    public bool TryDefine(string name, object? value)
    {
        if (!(_enclosing?.IsGlobal ?? true) && _enclosing._values.ContainsKey(name))
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