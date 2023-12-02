using Lox.Core;
using Lox.Visitors.Interpreters.Callables;
using Lox.Visitors.Resolvers;

namespace Lox.Visitors.Interpreters.Environments;

public class LoxEnvironment : ILoxEnvironment
{
    public static readonly IReadOnlyList<(Token Name, object? Value)> Globals = [(Token.FromIdentifier("clock"), new Clock()), (Token.FromIdentifier("typeof"), new TypeOf())];
    private static LoxEnvironment BuildGlobalEnvironment()
    {
        LoxEnvironment global = new();
        for (int i = 0; i < Globals.Count; i++)
        {
            global.Define(new(Globals[i].Name, i, 0), Globals[i].Value);
        }
        return global;
    }
    private static readonly Lazy<LoxEnvironment> s_globalEnvironment = new(BuildGlobalEnvironment, LazyThreadSafetyMode.ExecutionAndPublication);
    public static ILoxEnvironment GlobalEnvironment => s_globalEnvironment.Value;
    private readonly ILoxEnvironment? _enclosing;
    private readonly List<object?> _values = [];

    public int Depth { get; }
    public int Count => _values.Count;

    public LoxEnvironment() : this(null) { }
    private LoxEnvironment(ILoxEnvironment? enclosing)
    {
        _enclosing = enclosing;
        Depth = (_enclosing?.Depth ?? -1) + 1;
    }

    public ILoxEnvironment Push() => new LoxEnvironment(this);

    public bool TryDefine(ResolvedToken name, object? value)
    {
        if (name.Depth != Depth || name.Index != _values.Count) { return false; }
        _values.Add(value);
        return true;
    }
    public void Define(ResolvedToken name, object? value)
    {
        if (!TryDefine(name, value))
        {
            throw new DuplicateIdentifierException(name.Token);
        }
    }

    public bool TryGet(ResolvedToken name, out object? value)
    {
        if (name.Depth > Depth)
        {
            value = false;
            return false;
        }
        if (name.Depth == Depth)
        {
            if (name.Index >= _values.Count)
            {
                value = null;
                return false;
            }
            value = _values[name.Index];
            return true;
        }
        if (_enclosing is null)
        {
            value = null;
            return false;
        }
        return _enclosing.TryGet(name, out value);
    }

    public object? Get(ResolvedToken name)
    {
        if (!TryGet(name, out var val))
        {
            throw new UndefinedIdentifierException(name.Token);
        }
        if (val == Uninitialized.Instance)
        {
            throw new UninitializedIdentifierException(name.Token);
        }
        return val;
    }
    public bool TrySet(ResolvedToken name, object? value)
    {
        if (name.Depth > Depth) { return false; }
        if (name.Depth == Depth)
        {
            if (name.Index >= _values.Count) { return false; }
            _values[name.Index] = value;
            return true;
        }
        return _enclosing?.TrySet(name, value) ?? false;
    }
    public void Set(ResolvedToken name, object? value)
    {
        if (!TrySet(name, value))
        {
            throw new UndefinedIdentifierException(name.Token);
        }
    }
}