// See https://aka.ms/new-console-template for more information

using Lox;
using Lox.Expressions;
using Lox.Visitors;

Expression expression = new Binary(
    new Unary(
        new Token(0, 0, TokenType.Minus, null),
        new Literal(123)),
    new Token(0, 0, TokenType.Star, null),
    new Grouping(new Literal(45.67)));

Console.WriteLine(expression.Accept(new AstPrinter()));
expression = new Binary(new Grouping(new Binary(new Literal(1), new Token(0, 0, TokenType.Plus, null), new Literal(2))), new Token(0, 0, TokenType.Star, null), new Grouping(new Binary(new Literal(4), new Token(0, 0, TokenType.Minus, null), new Literal(3))));
Console.WriteLine(expression.Accept(new ReversePlishNotationPrinter()));