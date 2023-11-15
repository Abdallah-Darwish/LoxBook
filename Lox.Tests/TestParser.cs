using Lox.Core;
using Lox.Parsers;
using Lox.Scanners;

namespace Lox.Tests;

public class UnitTestParser
{
    [Theory]
    [InlineData("123456789")]
    [InlineData("99999")]
    [InlineData("00000.999")]
    [InlineData("0")]
    [InlineData("55")]
    public void TestParseNumbers_ValidNumber_ReturnsValue(string num)
    {
        var source = new StringReader(num);
        Scanner scanner = new(source);

        var token = scanner.GetAndMoveNext();
        Assert.Equal(TokenType.Number, token.Type);
        Assert.Equal(num, token.Lexeme);
        Assert.Equal(double.Parse(num), token.Value);
    }
}