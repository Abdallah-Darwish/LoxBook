// See https://aka.ms/new-console-template for more information

using Lox;
using Lox.Expressions;
using Lox.Visitors;

Expression expression = new Binary(
    new Unary(
        new Token(0, 0, TokenType.Minus, null),
        new Literal(new Token(0, 0, TokenType.Number, "123"))),
    new Token(0, 0, TokenType.Star, null),
    new Grouping(new Literal(new Token(0, 0, TokenType.Number, "45.67"))));

Console.WriteLine(expression.Accept(new AstPrinter()));
expression = new Binary(
    new Grouping(new Binary(new Literal(new Token(0, 0, TokenType.Number, "1")), new Token(0, 0, TokenType.Plus, null),
        new Literal(new Token(0, 0, TokenType.Number, "2")))),
    new Token(0, 0, TokenType.Star, null),
    new Grouping(new Binary(new Literal(new Token(0, 0, TokenType.Number, "4")), new Token(0, 0, TokenType.Minus, null),
        new Literal(new Token(0, 0, TokenType.Number, "3")))));
Console.WriteLine(expression.Accept(new ReversePolishNotationPrinter()));