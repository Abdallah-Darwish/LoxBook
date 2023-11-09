using Lox.Core;

namespace Lox.Visitors.Interpreters.Environemnts;

public interface ILoxEnvironment
{
    object? Get(Token id);
    void Define(Token id, object? value);
    void Set(Token id, object? value);
}