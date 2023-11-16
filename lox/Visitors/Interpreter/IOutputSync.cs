namespace Lox.Visitors.Interpreters;
/// <remarks>
/// I don't like the idea of the interpreter being an observable because:
///     1- We will never call OnCompleted
///     2- We're throwing exceptions instead of OnError
///     3- I don't think there would be ever a case where I would have multiple observers on the same interpreter
/// So Here I am creating this interface.
/// </remarks>
public interface IOutputSync<T>
{
    void Push(T value);
}