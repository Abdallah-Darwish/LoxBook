using Lox.Expressions;
using Lox.Statements;
using Expression = Lox.Expressions.Expression;

namespace Lox;

public class Parser
{
    private readonly Scanner _scanner;

    public Parser(Scanner scanner)
    {
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        _scanner.MoveNext();
    }
    public Statement Parse() => ParseStatement();
    public Statement ParseStatement()
    {
        Statement statement = _scanner.Current.Type switch
        {
            TokenType.Print => ParsePrint(),
            _ => ParseExpressionStatement()
        };
        if (_scanner.Current.Type != TokenType.Semicolon)
        {
            throw new ParserException(
                $"Expected a token of type {TokenType.Semicolon} instead found token of type {_scanner.Current.Type}",
                _scanner.Current);
        }
        _scanner.GetAndMoveNext();
        return statement;
    }

    public ExpressionStatement ParseExpressionStatement() => new(ParseExpression());
    public Print ParsePrint()
    {
        _scanner.GetAndMoveNext();
        return new Print(ParseExpression());
    }

    private Expression ParseExpression() => ParseComma();

    private Expression ParseComma()
    {
        var expr = ParseTernary();
        while (_scanner.Current.Type == TokenType.Comma)
        {
            var com = _scanner.GetAndMoveNext();
            expr = new Binary(expr, com, ParseTernary());
        }

        return expr;
    }

    private Expression ParseTernary()
    {
        var condition = ParseEquality();
        if (_scanner.Current.Type != TokenType.QuestionMark)
        {
            return condition;
        }

        var questionMark = _scanner.GetAndMoveNext();
        var left = ParseEquality();
        if (_scanner.Current.Type != TokenType.Colon)
        {
            throw new ParserException(
                $"Expected a token of type {TokenType.Colon} instead found token of type {_scanner.Current.Type}",
                _scanner.Current);
        }

        var colon = _scanner.GetAndMoveNext();

        var right = ParseEquality();
        return new Ternary(condition, questionMark, left, colon, right);
    }


    private Expression ParseEquality()
    {
        var expr = ParseComparison();
        while (_scanner.Current.Type is TokenType.BangEqual or TokenType.EqualEqual)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new Binary(expr, op, ParseComparison());
        }

        return expr;
    }

    private Expression ParseComparison()
    {
        var expr = ParseTerm();
        while (_scanner.Current.Type is TokenType.Greater or TokenType.GreaterEqual or TokenType.Less
               or TokenType.LessEqual)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new Binary(expr, op, ParseTerm());
        }

        return expr;
    }

    private Expression ParseTerm()
    {
        var expr = ParseFactor();
        while (_scanner.Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new Binary(expr, op, ParseFactor());
        }

        return expr;
    }

    private Expression ParseFactor()
    {
        var expr = ParseUnary();
        while (_scanner.Current.Type is TokenType.Slash or TokenType.Star)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new Binary(expr, op, ParseUnary());
        }

        return expr;
    }

    private Expression ParseUnary()
    {
        if (_scanner.Current.Type is TokenType.Bang or TokenType.Minus)
        {
            var op = _scanner.GetAndMoveNext();
            return new Unary(op, ParseUnary());
        }

        return ParsePrimary();
    }

    private Expression ParsePrimary()
    {
        if (_scanner.Current.Type is TokenType.Number or TokenType.String or TokenType.True or TokenType.False
            or TokenType.Nil)
        {
            return new Literal(_scanner.GetAndMoveNext());
        }

        if (_scanner.Current.Type != TokenType.LeftParentheses)
        {
            throw new ParserException(
                $"Expected a token of type {TokenType.LeftParentheses} instead found token of type {_scanner.Current.Type}",
                _scanner.Current);
        }

        _scanner.MoveNext();
        var expr = ParseExpression();
        if (_scanner.Current.Type != TokenType.RightParentheses)
        {
            throw new ParserException(
                $"Expected a token of type {TokenType.RightParentheses} instead found token of type {_scanner.Current.Type}",
                _scanner.Current);
        }

        _scanner.MoveNext();
        return new Grouping(expr);
    }

    private void Synchronize()
    {
        while (_scanner.MoveNext())
        {
            if (_scanner.Current.Type == TokenType.Semicolon)
            {
                //We want to consume the semicolon here but not the rest of the sync points
                _scanner.MoveNext();
            }
            if (_scanner.Current.Type is TokenType.Class or TokenType.Fun or TokenType.Var or TokenType.For
                or TokenType.If or TokenType.While or TokenType.Print or TokenType.Return)
            {
                return;
            }
        }
    }
}