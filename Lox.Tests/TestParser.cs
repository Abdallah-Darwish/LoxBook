﻿using Lox.Parsers;
using Lox.Tests.Utilities;

namespace Lox.Tests;

public class TestParser
{
    [Fact]
    public void TestParseComma_CommaWithAssignment_AssignmentTakesPrecedence()
    {
        string source = """
1 == 2, x = 1 == 3, y = z;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """{ [ [ [ 1 == 2 ] , [ x = [ 1 == 3 ] ] ] , [ y = z ] ] }""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseAssignment_AssignmentWithTernary_TernaryTakesPrecedence()
    {
        string source = """
x = 1 == 2 ? "HOW!" : "makes sense";
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """{ [ x = [ [ 1 == 2 ] ? "HOW!" : "makes sense" ] ] }""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseFor_WithAllParts_ShouldDesugarAsBlock()
    {
        string source = """
for(var x = 1; x < 10; x = x + 1)
    print x;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ block
    { var x = 1 }
    { while [ x < 10 ]
        { block
            { print x }
            { [ x = [ x + 1 ] ] }
        }
    }
}
""";

        Assert.Equal(expected, stmt);
    }
    [Fact]
    public void TestParseFor_MissingInitializer_ShouldDesugarAsWhile()
    {
        string source = """
for(; x < 10; x = x + 1)
    print x;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ while [ x < 10 ]
    { block
        { print x }
        { [ x = [ x + 1 ] ] }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseFor_MissingCondition_ShouldDesugarAsBlockWithTrue()
    {
        string source = """
for(var x = 1;; x = x + 1)
    print x;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ block
    { var x = 1 }
    { while true
        { block
            { print x }
            { [ x = [ x + 1 ] ] }
        }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseFor_MissingIterator_ShouldDesugarAsBlock()
    {
        string source = """
for(var x = 1; x < 505;)
    print x;
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ block
    { var x = 1 }
    { while [ x < 505 ]
        { print x }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseBreak_NotInLoop_ShouldThrowParserException()
    {
        string source = """
break;
""";

        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("No enclosing loop out of which to break", ex.Message);
    }

    [Fact]
    public void TestParseBreak_InLoop_ShouldParseBreak()
    {
        string source = """
for(var i = "hehe"; i or 2;)
{
    if(i == 10)
    {
        if(i == 11)
        {
            print i;
            break;
        }
    }
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ block
    { var i = "hehe" }
    { while [ i or 2 ]
        { block
            { if [ i == 10 ]
                { block
                    { if [ i == 11 ]
                        { block
                            { print i }
                            { break }
                        }
                    }
                }
            }
        }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Theory]
    [InlineData("nil(1, 2);")]
    [InlineData("true(1, 2);")]
    [InlineData("\"hello\"(1, 2);")]
    public void ParseCall_CalleeIsLiteral_ThrowsException(string source)
    {
        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("Literals are not callable", ex.Message);
    }

    [Fact]
    public void TestParseCall_NestedFunctionCalls_ArgsShouldBeOfTypeCall()
    {
        string source = """
min(max(1, 2), sqrt(1, -pow(5, 6), floor(10)), !gcd(505, 506, "hello", 1 + 2), clock());
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ [ min ( [ max ( 1 , 2 ) ] , [ sqrt ( 1 , [ - [ pow ( 5 , 6 ) ] ] , [ floor ( 10 ) ] ) ] , [ ! [ gcd ( 505 , 506 , "hello" , [ 1 + 2 ] ) ] ] , [ clock ( ) ] ) ] }
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseCall_AggregateFunctionCalls_CalleeShouldBeCallExpression()
    {
        string source = """
min(1, 2)(3, 4)();
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ [ [ [ min ( 1 , 2 ) ] ( 3 , 4 ) ] ( ) ] }
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseCall_AdditionalComma_ThrowsException()
    {
        string source = """
min(1,);
""";
        var ex = Assert.Throws<UnexpectedTokenException>(() => Utility.Parse(source));
        Assert.Contains("Expected a token of type LeftParentheses instead found token of type RightParentheses at Line", ex.Message);
        // This error is confusing but honestly I don't want to fix it and I want to focus on finishing the book
    }

    [Fact]
    public void TestParseReturn_NotInsideFunction_ThrowsException()
    {
        string source = """
return 1;
""";
        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("No enclosing function or method out of which to return", ex.Message);
    }

    [Fact]
    public void TestParseFunction_NormalFunction_ParsedSuccessfuly()
    {
        string source = """
fun Greet(name)
{
    print "Hello " + name;
    return "Greeted " + name;
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ fun Greet ( name )
    { print [ "Hello " + name ] }
    { return [ "Greeted " + name ] }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseFunction_NestedFunctions_InnerFunctionIsStatementInOuterOne()
    {
        string source = """
fun Greet(name)
{
    fun BuildMessage()
    {
        return "Hello " + name;
    }
    print BuildMessage();
    return "ok";
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ fun Greet ( name )
    { fun BuildMessage ( )
        { return [ "Hello " + name ] }
    }
    { print [ BuildMessage ( ) ] }
    { return "ok" }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseLambda_LambdaWithAssignment_CanAppearInExpressionPlace()
    {
        string source = """
var x = fun (name)
{
    print "Hello " + name;
};
x();
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ var x = [ fun ( name )
        { print [ "Hello " + name ] }
    ]
}
{ [ x ( ) ] }
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseBreak_InFunctionInsideLoop_ThrowsException()
    {
        string source = """
while(1 < 10)
{
    fun fn()
    {
        break;
    }
}
""";
        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("No enclosing loop out of which to break", ex.Message);
    }

    [Fact]
    public void TestParseClassDeclaration_NormalClass_ParsedSuccessfuly()
    {
        string source = """
class Cls
{
    Classing()
    {
        print "Classing from Dubai metro";
    }

    Speak(lang)
    {
        print lang + " has no class!";
    }
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ class Cls
    { fun Classing ( )
        { print "Classing from Dubai metro" }
    }
    { fun Speak ( lang )
        { print [ lang + " has no class!" ] }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseCall_GetExpressionWithCall_ShouldBeParsedAsNestedExpressions()
    {
        string source = """
what.you.know(about, rolling.down.in(the).deep);
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ [ [ [ what . you ] . know ] ( about , [ [ [ [ rolling . down ] . in ] ( the ) ] . deep ] ) ] }
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseThis_OutsideClass_ThrowsException()
    {
        string source = """
print this;
""";
        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("Can't use 'this' outside class", ex.Message);
        Assert.Equal("this", ex.Token!.Text);
    }

    [Fact]
    public void TestParseThis_InsideClass_ParsedFine()
    {
        string source = """
class PublicTransport
{
    WasteTime(time)
    {
        this._wastedTime = time + 100;
        return "Wasted " + this._wastedTime + " of this man's life";
    }
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ class PublicTransport
    { fun WasteTime ( time )
        { [ this . _wastedTime = [ time + 100 ] ] }
        { return [ [ "Wasted " + [ this . _wastedTime ] ] + " of this man's life" ] }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseFunction_ReturnFromFunctionNamedInit_Allowed()
    {
        string source = """
fun init()
{
    return 1;
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ fun init ( )
    { return 1 }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseFunction_ReturnFromConstructor_ThrowsException()
    {
        string source = """
class Cls
{
    init()
    {
        return 1;
    }
}
""";
        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("Can't return a value from an initializer.", ex.Message);
        Assert.Equal("return", ex.Token!.Text);
    }

    [Fact]
    public void TestParseProperty_ClassWithPropertiesAndMethods_ShouldParsePropertyAsFunction()
    {
        string source = """
class Circle {
  init(radius) {
    this.radius = radius;
  }

  area {
    return 3.14 * this.radius * this.radius;
  }
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ class Circle
    { fun init ( radius )
        { [ this . radius = radius ] }
    }
    { area
        { return [ [ 3.14 * [ this . radius ] ] * [ this . radius ] ] }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseClass_ClassHasSuper_WorksFine()
    {
        string source = """
class Shape {
    name() {
        return nil;
    }
}
class Circle < Shape {
  init(radius) {
    this.radius = radius;
  }
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ class Shape
    { fun name ( )
        { return nil }
    }
}
{ class Circle < Shape
    { fun init ( radius )
        { [ this . radius = radius ] }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseSuper_ClassHasSuper_WorksFine()
    {
        string source = """
class Shape {
    name() {
        return "Shape";
    }
}
class Circle < Shape {
  init(radius) {
    this.radius = radius;
  }

  name() {
    return super.name() + this.radius;
  }
}
""";
        var stmt = Utility.ParseAsString(source);
        var expected = """
{ class Shape
    { fun name ( )
        { return "Shape" }
    }
}
{ class Circle < Shape
    { fun init ( radius )
        { [ this . radius = radius ] }
    }
    { fun name ( )
        { return [ [ super.name ( ) ] + [ this . radius ] ] }
    }
}
""";

        Assert.Equal(expected, stmt);
    }

    [Fact]
    public void TestParseSuper_ClassDoesntHaveSuper_ThrowsException()
    {
        string source = """
class Shape {
    name() {
        return "Shape";
    }
}
class Circle {
  init(radius) {
    this.radius = radius;
  }

  name() {
    return super.name() + this.radius;
  }
}
""";

        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("Class Circle doesn't inherit from another to have a super.", ex.Message);
        Assert.Equal("super", ex.Token!.Text);
    }

    [Fact]
    public void TestParseSuper_OutsideClass_ThrowsException()
    {
        string source = """
print super.name();
""";

        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("Can't use 'super' outside of class.", ex.Message);
        Assert.Equal("super", ex.Token!.Text);
    }

    [Fact]
    public void TestParseClass_ClassInheritsItself_ThrowsException()
    {
        string source = """
class Oops < Oops {}
""";

        var ex = Assert.Throws<ParserException>(() => Utility.Parse(source));
        Assert.Contains("Class Token { Line = 0, Column = 6, Type = Identifier, Lexeme = Oops } can't inherit from itself.", ex.Message);
        Assert.Equal("Oops", ex.Token!.Text);
    }
}