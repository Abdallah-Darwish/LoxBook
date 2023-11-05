using Lox.Core;
namespace Lox.Scanners;
public interface IScanner : IEnumerator<Token>
{
    public bool IsExhausted { get; }
    Token GetAndMoveNext();
    Token GetAndMoveNext(TokenType expectedType);
    new Token Current { get; }
}