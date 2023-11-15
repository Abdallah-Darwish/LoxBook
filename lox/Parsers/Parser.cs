using Lox.Core;
using Lox.Scanners;

namespace Lox.Parsers;

public class Parser : IParser
{
    private IScanner _scanner;
    private bool _disposed;
    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    public bool IsExhausted
    {
        get
        {
            CheckDisposed();
            return _scanner.IsExhausted;
        }
    }

    public Parser(IScanner scanner)
    {
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        _scanner.MoveNext();
    }
    /// <inheritdoc/>
    public Statement? Parse()
    {
        CheckDisposed();
        if (IsExhausted) { return null; }
        try
        {
            return ParseDeclaration();
        }
        catch (ParserException)
        {
            Synchronize();
            throw;
        }
    }

    private Statement ParseDeclaration()
    {
        return _scanner.Current.Type switch
        {
            TokenType.Var => ParseVariableDeclaration(),
            _ => ParseStatement()
        };
    }
    private VariableStatement ParseVariableDeclaration()
    {
        _scanner.GetAndMoveNext(TokenType.Var);

        var id = _scanner.GetAndMoveNext(TokenType.Identifier);
        Expression? init = null;
        if (_scanner.Current.Type == TokenType.Equal)
        {
            _scanner.GetAndMoveNext();
            init = ParseExpression();
        }
        _scanner.GetAndMoveNext(TokenType.Semicolon);
        return new(id, init);
    }
    private Statement ParseStatement() => _scanner.Current.Type switch
    {
        TokenType.Print => ParsePrint(),
        TokenType.LeftBrace => new BlockStatement(ParseBlock()),
        TokenType.If => ParseIf(),
        TokenType.While => ParseWhile(),
        _ => ParseExpressionStatement()
    };
    private IfStatement ParseIf()
    {
        _scanner.GetAndMoveNext(TokenType.If);
        _scanner.GetAndMoveNext(TokenType.LeftParentheses);
        var condition = ParseExpression();
        _scanner.GetAndMoveNext(TokenType.RightParentheses);
        var then = ParseStatement();
        Statement? els = null;
        if (_scanner.Current.Type == TokenType.Else)
        {
            els = ParseStatement();
        }
        return new IfStatement(condition, then, els);
    }
    private WhileStatement ParseWhile()
    {
        _scanner.GetAndMoveNext(TokenType.While);
        _scanner.GetAndMoveNext(TokenType.LeftParentheses);
        var condition = ParseExpression();
        _scanner.GetAndMoveNext(TokenType.RightParentheses);
        var body = ParseStatement();
        return new WhileStatement(condition, body);
    }
    private IReadOnlyList<Statement> ParseBlock()
    {
        _scanner.GetAndMoveNext(TokenType.LeftBrace);
        List<Statement> body = new();
        while (_scanner.Current.Type != TokenType.RightBrace)
        {
            body.Add(ParseDeclaration());
        }
        _scanner.GetAndMoveNext();
        return body;
    }
    private PrintStatement ParsePrint()
    {
        _scanner.GetAndMoveNext();
        PrintStatement print = new(ParseExpression());
        _scanner.GetAndMoveNext(TokenType.Semicolon);
        return print;
    }

    private ExpressionStatement ParseExpressionStatement()
    {
        ExpressionStatement expr = new(ParseExpression());
        _scanner.GetAndMoveNext(TokenType.Semicolon);
        return expr;
    }

    private Expression ParseExpression() => ParseComma();

    private Expression ParseComma()
    {
        var expr = ParseAssignment();
        while (_scanner.Current.Type == TokenType.Comma)
        {
            var com = _scanner.GetAndMoveNext();
            expr = new BinaryExpression(expr, com, ParseAssignment());
        }

        return expr;
    }

    private Expression ParseAssignment()
    {
        var name = ParseTernary();
        if (_scanner.Current.Type != TokenType.Equal)
        {
            return name;
        }
        if (name is not VariableExpression varExpr)
        {
            throw new ParserException($"Expected a expression of type {nameof(VariableExpression)} instead found {name.GetType().Name}", _scanner.Current);
        }
        _scanner.GetAndMoveNext();

        var val = ParseAssignment();
        return new AssignmentExpression(varExpr.Name, val);
    }

    private Expression ParseTernary()
    {
        var condition = ParseOr();
        if (_scanner.Current.Type != TokenType.QuestionMark)
        {
            return condition;
        }

        _scanner.GetAndMoveNext();
        var left = ParseOr();
        _scanner.GetAndMoveNext(TokenType.Colon);

        var right = ParseOr();
        return new TernaryExpression(condition, left, right);
    }


    private Expression ParseOr()
    {
        var expr = ParseAnd();
        while (_scanner.Current.Type == TokenType.Or)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new BinaryExpression(expr, op, ParseAnd());
        }
        return expr;
    }
    private Expression ParseAnd()
    {
        var expr = ParseEquality();
        while (_scanner.Current.Type == TokenType.And)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new BinaryExpression(expr, op, ParseEquality());
        }
        return expr;
    }

    private Expression ParseEquality()
    {
        var expr = ParseComparison();
        while (_scanner.Current.Type is TokenType.BangEqual or TokenType.EqualEqual)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new BinaryExpression(expr, op, ParseComparison());
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
            expr = new BinaryExpression(expr, op, ParseTerm());
        }

        return expr;
    }

    private Expression ParseTerm()
    {
        var expr = ParseFactor();
        while (_scanner.Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new BinaryExpression(expr, op, ParseFactor());
        }

        return expr;
    }

    private Expression ParseFactor()
    {
        var expr = ParseUnary();
        while (_scanner.Current.Type is TokenType.Slash or TokenType.Star)
        {
            var op = _scanner.GetAndMoveNext();
            expr = new BinaryExpression(expr, op, ParseUnary());
        }

        return expr;
    }

    private Expression ParseUnary()
    {
        if (_scanner.Current.Type is TokenType.Bang or TokenType.Minus)
        {
            var op = _scanner.GetAndMoveNext();
            return new UnaryExpression(op, ParseUnary());
        }

        return ParsePrimary();
    }

    private Expression ParsePrimary()
    {
        if (_scanner.Current.Type is TokenType.Number or TokenType.String or TokenType.True or TokenType.False
            or TokenType.Nil)
        {
            return new LiteralExpression(_scanner.GetAndMoveNext());
        }
        if (_scanner.Current.Type == TokenType.Identifier)
        {
            return new VariableExpression(_scanner.GetAndMoveNext());
        }

        _scanner.GetAndMoveNext(TokenType.LeftParentheses);
        var expr = ParseExpression();
        _scanner.GetAndMoveNext(TokenType.RightParentheses);

        return new GroupingExpression(expr);
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

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _scanner.Dispose();
            }
            _scanner = null;
            _disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}