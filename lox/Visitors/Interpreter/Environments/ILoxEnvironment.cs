using Lox.Core;

namespace Lox.Visitors.Interpreters.Environments;

public interface ILoxEnvironment
{
    object? Get(Token id);
    void Define(Token id, object? value);
    void Set(Token id, object? value);
}