using Lox;
using Lox.Expressions;

namespace lox;

public class Parser
{
    private readonly Scanner _scanner;

    private Expression ParseExpression() => ParseEquality();

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
        var expr = ParseExpression(); // Saif: Isn't calling expression here wrong since we're matching a lower precedence rule ?
        if (_scanner.Current.Type != TokenType.RightParentheses)
        {
            throw new ParserException(
                $"Expected a token of type {TokenType.RightParentheses} instead found token of type {_scanner.Current.Type}",
                _scanner.Current);
        }

        _scanner.MoveNext();
        return new Grouping(expr);
    }
}