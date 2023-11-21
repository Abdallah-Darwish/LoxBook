using Lox.Core;
namespace Lox.Scanners;
public interface IScanner : IEnumerator<Token>
{
    public bool IsExhausted { get; }
    Token GetAndMoveNext();
    Token GetAndMoveNext(TokenType expectedType, string? calrifyingMessage = null);
    new Token Current { get; }
}