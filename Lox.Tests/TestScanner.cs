using Lox.Core;
using Lox.Scanners;

namespace Lox.Tests;

public class TestScanner
{
    [Theory]
    [InlineData("123456789")]
    [InlineData("99999")]
    [InlineData("00000.999")]
    [InlineData("0")]
    [InlineData("55")]
    public void TestScanNumbers_ValidNumber_ReturnsValue(string num)
    {
        var token = Utility.Scan(num);

        Assert.Equal(TokenType.Number, token.Type);
        Assert.Equal(num, token.Lexeme);
        Assert.Equal(double.Parse(num), token.Value);
    }

    [Theory]
    [InlineData("1.2.3456789")]
    [InlineData("99.99.9")]
    public void TestScanNumbers_MultipleDecimalPoints_ThrowsException(string num)
    {
        var ex = Assert.Throws<ScannerException>(() => Utility.Scan(num));
        Assert.Equal("Number contains multiple decimal points.", ex.Message);
    }

    [Theory]
    [InlineData("0.")]
    [InlineData("127.")]
    public void TestScanNumbers_EndsWithDecimalPoint_ThrowsException(string num)
    {
        var ex = Assert.Throws<ScannerException>(() => Utility.Scan(num));
        Assert.Equal("Number ends with a decimal point.", ex.Message);
    }
}