using Lox.Core;

public interface IParser
{
    bool IsExhausted { get; }
    Statement? Parse();
}