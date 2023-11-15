using Lox.Core;

public interface IParser : IDisposable
{
    bool IsExhausted { get; }
    /// <summary>
    /// Would return null when the underlying scanner is exhausted.
    /// </summary>
    Statement? Parse();
}