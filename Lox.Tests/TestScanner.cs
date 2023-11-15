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
        var source = new StringReader(num);
        Scanner scanner = new(source);

        Assert.True(scanner.MoveNext());
        Assert.Equal(TokenType.Number, scanner.Current.Type);
        Assert.Equal(num, scanner.Current.Lexeme);
        Assert.Equal(double.Parse(num), scanner.Current.Value);
    }

    [Theory]
    [InlineData("1.2.3456789")]
    [InlineData("99.99.9")]
    public void TestScanNumbers_MultipleDecimalPoints_ThrowsException(string num)
    {
        var source = new StringReader(num);
        Scanner scanner = new(source);

        var ex = Assert.Throws<ScannerException>(() => scanner.MoveNext());
        Assert.Equal("Number contains multiple decimal points.", ex.Message);
    }

    [Theory]
    [InlineData("0.")]
    [InlineData("127.")]
    public void TestScanNumbers_EndsWithDecimalPoint_ThrowsException(string num)
    {
        var source = new StringReader(num);
        Scanner scanner = new(source);

        var ex = Assert.Throws<ScannerException>(() => scanner.MoveNext());
        Assert.Equal("Number ends with a decimal point.", ex.Message);
    }
}