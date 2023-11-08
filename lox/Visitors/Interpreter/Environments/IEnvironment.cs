using Lox.Core;

namespace Lox.Visitors.Interpreters.Environemnts;

public interface IEnvironment
{
    object? Get(Token id);
    void Define(Token id, object? value);
    void Set(Token id, object? value);
}