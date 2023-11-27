using Lox.Core;

namespace Lox.Parsers;
public interface IParser : IDisposable
{
    bool IsExhausted { get; }
    /// <summary>
    /// Would return null when the underlying scanner is exhausted.
    /// </summary>
    Statement? Parse();
}
