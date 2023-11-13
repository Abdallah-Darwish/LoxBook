using Lox.Core;

namespace Lox.Visitors.Interpreters.Environemnts;

public class LoxEnvironemnt : ILoxEnvironment
{
    private readonly Dictionary<string, object?> _values = new();
    public bool TryDefine(string name, object? value) => _values.TryAdd(name, value);
    public void Define(Token id, object? value)
    {
        if (!TryDefine(id.Lexeme!, value))
        {
            throw new DuplicateIdentifierException(id);
        }
    }
    public bool TryGet(string id, out object? value) => _values.TryGetValue(id, out value);
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
        if (!_values.ContainsKey(id)) { return false; }
        _values[id] = value;
        return true;
    }
    public void Set(Token id, object? value)
    {
        if (!TrySet(id.Lexeme!, value))
        {
            throw new UndefinedIdentifierException(id);
        }
    }
}