namespace lox;

public record class Token(int Line, int Column, TokenType Type, string? Lexeme)
{
    public T GetValue<T>()
    {
        var type = typeof(T);
        return Type switch
        {
            TokenType.String when type != typeof(string) || Lexeme is null => throw new InvalidCastException(
                $"Can't convert {this} value to {type}"),
            TokenType.String => (T)(object)Lexeme[1..^1],
            TokenType.Number when type != typeof(double) || Lexeme is null => throw new InvalidCastException(
                $"Can't convert {this} value to {type}"),
            TokenType.Number => (T)(object)double.Parse(Lexeme),
            _ => throw new ArgumentOutOfRangeException(nameof(T), type,
                $"This method only supports: [string, double] types")
        };
    }
}