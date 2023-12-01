using Lox.Visitors.Resolvers;

namespace Lox.Visitors.Interpreters.Environments;

public interface ILoxEnvironment
{
    int Depth { get; }
    int Count { get; }

    bool TryGet(ResolvedToken name, out object? value);
    object? Get(ResolvedToken name);

    bool TryDefine(ResolvedToken name, object? value);
    void Define(ResolvedToken id, object? value);

    bool TrySet(ResolvedToken name, object? value);
    void Set(ResolvedToken id, object? value);

    ILoxEnvironment Push();
}