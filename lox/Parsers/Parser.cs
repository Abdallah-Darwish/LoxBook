using Lox.Core;
using Lox.Scanners;


namespace Lox.Parsers;
public class Parser : IParser
{
    private IScanner _scanner;

    private abstract record class State(int Depth, Token Id);
    private record class FunctionState(int Depth, Token Id, Token? FunctionName) : State(Depth, Id);
    private record class ClassState(int Depth, Token Id, Token? ClassName, Token? SuperName) : State(Depth, Id);
    private record class LoopState(int Depth, Token Id) : State(Depth, Id);
    private readonly Dictionary<Type, Stack<State>> _cactusState = [];
    private readonly Stack<State> _state = [];

    private T? GetLastStateOf<T>() where T : State
    {
        if (!_cactusState.TryGetValue(typeof(T), out var tStateStack) || tStateStack.Count == 0)
        {
            return null;
        }
        return tStateStack.Peek() as T;
    }
    private bool IsInLoopRule => (GetLastStateOf<LoopState>()?.Depth ?? -2) > Math.Max(GetLastStateOf<ClassState>()?.Depth ?? -1, GetLastStateOf<FunctionState>()?.Depth ?? -1);
    private bool IsInFunctionBody => (GetLastStateOf<FunctionState>()?.Depth ?? -2) > (GetLastStateOf<ClassState>()?.Depth ?? -1);
    private Token? CurrentFunctionName => GetLastStateOf<FunctionState>()?.FunctionName;
    private bool IsInClassBody => GetLastStateOf<ClassState>() is not null;
    private Token? CurrentClassName => GetLastStateOf<ClassState>()?.ClassName;
    private Token? CurrentClassSuperName => GetLastStateOf<ClassState>()?.SuperName;

    private void PushState<T>(T state) where T : State
    {
        var stateType = typeof(T);
        if (!_cactusState.TryGetValue(stateType, out var tStateStack))
        {
            _cactusState.Add(stateType, tStateStack = new());
        }
        tStateStack.Push(state);

        _state.Push(state);
    }

    private void PopState<T>(T state) where T : State
    {
        if (!_state.TryPeek(out var lastState))
        {
            throw new InvalidOperationException($"Can't pop state when state stack is empty, state: {state}");
        }
        if (lastState != state)
        {
            throw new InvalidOperationException($"Last pushed state {lastState} != to be popped state {state}");
        }
        _cactusState[typeof(T)].Pop();
        _state.Pop();
    }
    private bool _disposed;

    private void CheckDisposed()
    {
        if (!_disposed)
        {
            return;
        }
        throw new ObjectDisposedException(GetType().FullName);
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
            TokenType.Fun => ParseFunctionDeclaration(false),
            TokenType.Class => ParseClassDeclaration(),
            _ => ParseStatement()
        };
    }
    private ClassStatement ParseClassDeclaration()
    {
        var klass = _scanner.GetAndMoveNext(TokenType.Class);
        var name = _scanner.GetAndMoveNext(TokenType.Identifier);
        Token? super = null;
        if (_scanner.Current.Type == TokenType.Less)
        {
            _scanner.GetAndMoveNext();
            super = _scanner.GetAndMoveNext(TokenType.Identifier, "inherit operator");
            if (name.Lexeme == super.Lexeme)
            {
                throw new ParserException($"Class {name} can't inherit from itself.", super);
            }
        }
        _scanner.GetAndMoveNext(TokenType.LeftBrace, "class name");

        ClassState state = new(_state.Count, klass, name, super);
        PushState(state);
        try
        {
            List<FunctionStatement> methods = [];
            while (_scanner.Current.Type != TokenType.RightBrace)
            {
                methods.Add(ParseFunctionDeclaration(true));
            }
            _scanner.GetAndMoveNext(TokenType.RightBrace);
            return new(name, super, methods);
        }
        finally
        {
            PopState(state);
        }
    }
    private VariableStatement ParseVariableDeclaration()
    {
        _scanner.GetAndMoveNext(TokenType.Var);

        var id = _scanner.GetAndMoveNext(TokenType.Identifier, "variable declaration");
        Expression? init = null;
        if (_scanner.Current.Type == TokenType.Equal)
        {
            _scanner.GetAndMoveNext();
            init = ParseExpression();
        }
        _scanner.GetAndMoveNext(TokenType.Semicolon, "variable declaration");
        return new(id, init);
    }
    private (IReadOnlyList<Token> Parameters, IReadOnlyList<Statement> Body) ParseLambdaParametersAndBody(FunctionType type, Token? name)
    {
        List<Token> parameters = [];
        if (type != FunctionType.Property)
        {
            _scanner.GetAndMoveNext(TokenType.LeftParentheses, $"{type} name");


            bool isFirstParam = true;
            while (_scanner.Current.Type != TokenType.RightParentheses)
            {
                if (!isFirstParam)
                {
                    _scanner.GetAndMoveNext(TokenType.Comma, $"{type} parameter");
                    isFirstParam = false;
                }
                parameters.Add(_scanner.GetAndMoveNext(TokenType.Identifier, $"{type} declaration"));
                isFirstParam = false;
            }
            _scanner.GetAndMoveNext(TokenType.RightParentheses, $"{type} parameter list");
        }

        FunctionState state = new(_state.Count, _scanner.Current, name);
        PushState(state);
        try
        {
            return (parameters, ParseBlock());
        }
        finally
        {
            PopState(state);
        }
    }
    /// <param name="isTopClassFunction">
    /// Used to ignore <see cref="TokenType.Fun"/> for class methods and properties, and to allow properties in classes.
    /// </param>
    private FunctionStatement ParseFunctionDeclaration(bool isTopClassFunction)
    {
        if (!isTopClassFunction)
        {
            _scanner.GetAndMoveNext(TokenType.Fun, $"{FunctionType.Function} declaration");
        }
        var name = _scanner.GetAndMoveNext(TokenType.Identifier, $@"{FunctionType.Function}\{FunctionType.Property}\{FunctionType.Method} declaration");

        var type = FunctionType.Function;
        if (isTopClassFunction)
        {
            type = _scanner.Current.Type == TokenType.LeftParentheses ? FunctionType.Method : FunctionType.Property;
        }
        var (parameters, body) = ParseLambdaParametersAndBody(type, name);
        return new(name, parameters, type, body);
    }
    private Statement ParseStatement() => _scanner.Current.Type switch
    {
        TokenType.Print => ParsePrint(),
        TokenType.LeftBrace => new BlockStatement(ParseBlock()),
        TokenType.If => ParseIf(),
        TokenType.While => ParseWhile(),
        TokenType.For => ParseFor(),
        TokenType.Break => ParseBreak(),
        TokenType.Return => ParseReturn(),
        _ => ParseExpressionStatement()
    };
    private ReturnStatement ParseReturn()
    {
        if (!IsInFunctionBody)
        {
            throw new ParserException("No enclosing function or method out of which to return.", _scanner.Current);
        }
        var ret = _scanner.GetAndMoveNext(TokenType.Return);
        Expression? val = null;
        if (_scanner.Current.Type != TokenType.Semicolon)
        {
            val = ParseExpression();
        }
        _scanner.GetAndMoveNext(TokenType.Semicolon, "return keyword");

        if (IsInClassBody && CurrentFunctionName.Text == "init" && val is not null)
        {
            //This would catch valid cases where a class->func->init->return 1; but I wouldn't worry about it now
            throw new ParserException("Can't return a value from an initializer.", ret);
        }
        return new(ret, val);
    }
    private BreakStatement ParseBreak()
    {
        if (!IsInLoopRule)
        {
            throw new ParserException("No enclosing loop out of which to break.", _scanner.Current);
        }
        _scanner.GetAndMoveNext(TokenType.Break);
        _scanner.GetAndMoveNext(TokenType.Semicolon, "break statement");
        return new BreakStatement();
    }
    private IfStatement ParseIf()
    {
        _scanner.GetAndMoveNext(TokenType.If);
        _scanner.GetAndMoveNext(TokenType.LeftParentheses, "if statement");
        var condition = ParseExpression();
        _scanner.GetAndMoveNext(TokenType.RightParentheses, "if statement condition");
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
        var whilee = _scanner.GetAndMoveNext(TokenType.While);
        _scanner.GetAndMoveNext(TokenType.LeftParentheses, "while statement");
        var condition = ParseExpression();
        _scanner.GetAndMoveNext(TokenType.RightParentheses, "while statement condition");

        var body = ParseLoopBody(whilee);
        return new WhileStatement(condition, body);
    }
    private Statement ParseFor()
    {
        var forr = _scanner.GetAndMoveNext(TokenType.For);
        _scanner.GetAndMoveNext(TokenType.LeftParentheses, "for statement");

        Statement? init = null;
        if (_scanner.Current.Type == TokenType.Var)
        {
            init = ParseVariableDeclaration();
        }
        else if (_scanner.Current.Type != TokenType.Semicolon)
        {
            init = ParseExpressionStatement();
        }
        else
        {
            _scanner.GetAndMoveNext(TokenType.Semicolon, "for statement or a varaible declaration or an expression");
        }

        Expression cond;
        if (_scanner.Current.Type != TokenType.Semicolon)
        {
            cond = ParseExpressionStatement().Expression;
        }
        else
        {
            cond = new LiteralExpression(Token.FromBool(true));
            _scanner.GetAndMoveNext(TokenType.Semicolon, "for statement condition or an expression");
        }

        ExpressionStatement? iter = null;
        if (_scanner.Current.Type != TokenType.RightParentheses)
        {
            iter = new(ParseExpression());
        }
        _scanner.GetAndMoveNext(TokenType.RightParentheses, "for statement iterator");

        var body = ParseLoopBody(forr);
        Statement whileBody = iter is null ? body : new BlockStatement(new Statement[] { body, iter });
        Statement whileStmt = new WhileStatement(cond, whileBody);

        return init is null ? whileStmt : new BlockStatement(new Statement[] { init, whileStmt });
    }
    private Statement ParseLoopBody(Token id)
    {
        LoopState state = new(_state.Count, id);
        PushState(state);
        try { return ParseStatement(); }
        finally { PopState(state); }
    }
    private IReadOnlyList<Statement> ParseBlock()
    {
        _scanner.GetAndMoveNext(TokenType.LeftBrace);
        List<Statement> body = [];
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
        _scanner.GetAndMoveNext(TokenType.Semicolon, "print statement");
        return print;
    }

    private ExpressionStatement ParseExpressionStatement()
    {
        ExpressionStatement expr = new(ParseExpression());
        if (expr.Expression is LambdaExpression lambdaExpression)
        {
            throw new ParserException("Lambdas can't be used as a standalone statements.", lambdaExpression.Fun);
        }
        _scanner.GetAndMoveNext(TokenType.Semicolon, "expression");
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
        if (name is not VariableExpression and not GetExpression)
        {
            throw new ParserException($"Expected a expression of type {nameof(VariableExpression)} or {nameof(GetExpression)} instead found {name.GetType().Name}", _scanner.Current);
        }
        _scanner.GetAndMoveNext();

        var val = ParseAssignment();
        return name is GetExpression getExpr ? new SetExpression(getExpr.Instance, getExpr.Name, val) : new AssignmentExpression((name as VariableExpression)!.Name, val);
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
        _scanner.GetAndMoveNext(TokenType.Colon, "ternary operator left hand");

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

        return ParseCall();
    }
    private Expression ParseCall()
    {
        var lhs = ParseLambda();
        while (true)
        {
            // Its done this way to handle syntaxes like Func(1, 2)(3, 4)(5, 6)
            if (_scanner.Current.Type == TokenType.LeftParentheses)
            {
                if (lhs is LiteralExpression literal)
                {
                    throw new ParserException("Literals are not callable.", literal.Value);
                }
                var (rightParentheses, arguemnts) = ParseArguments();
                lhs = new CallExpression(lhs, rightParentheses, arguemnts);
            }
            else if (_scanner.Current.Type == TokenType.Dot)
            {
                if (lhs is LiteralExpression literal)
                {
                    throw new ParserException("Literals can't appear before '.'.", literal.Value);
                }
                _scanner.GetAndMoveNext();
                var name = _scanner.GetAndMoveNext(TokenType.Identifier, "access operator '.'");
                lhs = new GetExpression(lhs, name);
            }
            else
            {
                break;
            }
        }
        return lhs;
    }
    private (Token RightParentheses, Expression[] Arguemnts) ParseArguments()
    {
        _scanner.GetAndMoveNext(TokenType.LeftParentheses, "callee");
        Expression[] args;
        if (_scanner.Current.Type != TokenType.RightParentheses)
        {
            Stack<Expression> argsStack = [];
            var commaArgs = ParseComma();
            while (commaArgs is BinaryExpression binary && binary.Operator.Type == TokenType.Comma)
            {
                argsStack.Push(binary.Right);
                commaArgs = binary.Left;
            }
            argsStack.Push(commaArgs);
            args = new Expression[argsStack.Count];
            for (int i = 0; argsStack.Count != 0; args[i++] = argsStack.Pop()) ;
        }
        else
        {
            args = [];
        }

        return (_scanner.GetAndMoveNext(TokenType.RightParentheses, "call arguments"), args);
    }
    private Expression ParseLambda()
    {
        if (_scanner.Current.Type == TokenType.Fun)
        {
            var fun = _scanner.GetAndMoveNext();
            var (parameters, body) = ParseLambdaParametersAndBody(FunctionType.Lambda, null);
            return new LambdaExpression(fun, parameters, body);
        }
        return ParsePrimary();
    }
    private Expression ParsePrimary()
    {
        if (_scanner.Current.Type == TokenType.This)
        {
            if (!IsInClassBody)
            {
                throw new ParserException("Can't use 'this' outside class.", _scanner.Current);
            }
            return new ThisExpression(_scanner.GetAndMoveNext());
        }
        if (_scanner.Current.Type is TokenType.Number or TokenType.String or TokenType.True or TokenType.False
            or TokenType.Nil)
        {
            return new LiteralExpression(_scanner.GetAndMoveNext());
        }
        if (_scanner.Current.Type == TokenType.Identifier)
        {
            return new VariableExpression(_scanner.GetAndMoveNext());
        }
        if (_scanner.Current.Type == TokenType.Super)
        {
            if (!IsInClassBody)
            {
                throw new ParserException("Can't use 'super' outside of class.", _scanner.Current);
            }
            if (CurrentClassSuperName is null)
            {
                throw new ParserException($"Class {CurrentClassName.Text} doesn't inherit from another to have a super.", _scanner.Current);
            }
            var super = _scanner.GetAndMoveNext();
            _scanner.GetAndMoveNext(TokenType.Dot, "super keyword");
            var name = _scanner.GetAndMoveNext(TokenType.Identifier, "super accessor");
            return new SuperExpression(super, name);
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