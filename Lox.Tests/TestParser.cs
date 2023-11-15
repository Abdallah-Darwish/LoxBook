using Lox.Core;
using Lox.Scanners;

namespace Lox.Tests;

public class TestParser
{
    [Theory]
    [InlineData("123456789")]
    [InlineData("99999")]
    [InlineData("00000.999")]
    [InlineData("0")]
    [InlineData("55")]
    public void TestScanNumbers_ValidNumber_ReturnsValue(string num)
    {
        var source = new StringReader(num);
        Scanner scanner = new(source);


        Assert.True(scanner.MoveNext());
        var token = scanner.GetAndMoveNext();
        Assert.Equal(TokenType.Number, token.Type);
        Assert.Equal(num, token.Lexeme);
        Assert.Equal(double.Parse(num), token.Value);
    }
}