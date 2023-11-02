using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace Lox;

public class Scanner : IEnumerator<Token>
{
    private TextReader _source;

    public Scanner(TextReader source)
    {
        _source = source;
    }

    private int _line = -1, _col = -1;

    private bool _isNewLine = true;

    private char Peek()
    {
        int c = _source.Peek();
        return c == -1 ? '\0' : (char)c;
    }

    private char Read()
    {
        int c = _source.Read();
        if (c == -1)
        {
            return '\0';
        }

        if (_isNewLine)
        {
            _line++;
            _col = -1;
            _isNewLine = false;
        }

        if (c == '\n')
        {
            _isNewLine = true;
        }

        _col++;
        return (char)c;
    }

    private static readonly ImmutableDictionary<string, TokenType> s_keywords =
        new Dictionary<string, TokenType>
        {
            ["and"] = TokenType.And,
            ["class"] = TokenType.Class,
            ["else"] = TokenType.Else,
            ["false"] = TokenType.False,
            ["fun"] = TokenType.Fun,
            ["for"] = TokenType.For,
            ["if"] = TokenType.If,
            ["nil"] = TokenType.Nil,
            ["or"] = TokenType.Or,
            ["print"] = TokenType.Print,
            ["return"] = TokenType.Return,
            ["super"] = TokenType.Super,
            ["this"] = TokenType.This,
            ["true"] = TokenType.True,
            ["var"] = TokenType.Var,
            ["while"] = TokenType.While
        }.ToImmutableDictionary();
    private Token BuildToken(TokenType type, string? lexeme, int col = -1, int line = -1) => new(line != -1 ? line : _line, col != -1 ? col : _col, type, lexeme);
    private Token BuildToken(TokenType type, int col = -1, int line = -1) => BuildToken(type, null, col, line);

    private Token BuildToken(char expectedChar, TokenType matchedToken, TokenType unmatchedToken)
    {
        if (Peek() != expectedChar) { return BuildToken(unmatchedToken); }
        Read();
        return BuildToken(matchedToken, _col - 1);
    }
    /// <summary>
    /// Returns null for comments and spaces.
    /// </summary>
    private Token? ScanSingleToken()
    {
        var c = Read();
        switch (c)
        {
            case '\0':
                return BuildToken(TokenType.Eof);
            case '(':
                return BuildToken(TokenType.LeftParentheses);
            case ')':
                return BuildToken(TokenType.RightParentheses);
            case '{':
                return BuildToken(TokenType.LeftBrace);
            case '}':
                return BuildToken(TokenType.RightBrace);
            case ',':
                return BuildToken(TokenType.Comma);
            case '.':
                return BuildToken(TokenType.Dot);
            case '-':
                return BuildToken(TokenType.Minus);
            case '+':
                return BuildToken(TokenType.Plus);
            case ';':
                return BuildToken(TokenType.Semicolon);
            case '*':
                return BuildToken(TokenType.Star);
            case '?':
                return BuildToken(TokenType.QuestionMark);
            case ':':
                return BuildToken(TokenType.Colon);
            case '!':
                return BuildToken('=', TokenType.BangEqual, TokenType.Bang);
            case '=':
                return BuildToken('=', TokenType.EqualEqual, TokenType.Equal);
            case '>':
                return BuildToken('=', TokenType.GreaterEqual, TokenType.Greater);
            case '<':
                return BuildToken('=', TokenType.LessEqual, TokenType.Less);
            case '/':
                if (Peek() == '*')
                {
                    Read();
                    c = Read();
                    while (c != '\0' && c != '*' || Peek() != '/')
                    {
                        c = Read();
                    }

                    Read(); // Read the last /
                    return null;
                }
                if (Peek() != '/')
                {
                    return BuildToken(TokenType.Slash);
                }

                for (c = Read(); c is not '\n' and not '\0'; c = Read())
                {
                }

                return null;
            case '"':
                {
                    StringBuilder lexeme = new();
                    lexeme.Append(c);
                    c = Read();
                    int line = _line;
                    int col = _col;
                    while (c != '\0' && c != '"')
                    {
                        lexeme.Append(c);
                        c = Read();
                    }

                    lexeme.Append(c);
                    if (lexeme[^1] != '"')
                    {
                        throw new ScannerException("Unterminated string.", line, col);
                    }

                    return BuildToken(TokenType.String, lexeme.ToString(), col, line);
                }
            case >= '0' and <= '9':
                {
                    StringBuilder lexeme = new();
                    lexeme.Append(c);
                    c = Peek();
                    int col = _col;
                    bool seenDecimal = false;
                    while (c is > '0' and < '9' || (c == '.' && !seenDecimal))
                    {
                        c = Read();
                        seenDecimal |= c == '.';
                        lexeme.Append(c);
                        c = Peek();
                    }

                    if (seenDecimal && c == '.')
                    {
                        throw new ScannerException("Number contains multiple decimal points.", _line, col);
                    }

                    if (lexeme[^1] == '.')
                    {
                        throw new ScannerException("Number ends with a decimal point.", _line, col);
                    }

                    return BuildToken(TokenType.Number, lexeme.ToString(), col);
                }
            //Identifier naming rules
            case '_':
            case >= 'A' and <= 'Z':
            case >= 'a' and <= 'z':
                {
                    StringBuilder lexeme = new();
                    lexeme.Append(c);
                    var col = _col;
                    c = Peek();
                    while (c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')
                    {
                        c = Read();
                        lexeme.Append(c);
                        c = Peek();
                    }

                    return s_keywords.TryGetValue(lexeme.ToString(), out var keywordType)
                        ? BuildToken(keywordType, col)
                        : BuildToken(TokenType.Identifier, lexeme.ToString(), col);
                }
            case ' ':
            case '\r':
            case '\t':
            case '\n':
                return null;
            default:
                throw new ScannerException($"Unexpected char '{c}'.", _line, _col);
        }
    }

    private Token ScanNonNullToken()
    {
        var token = ScanSingleToken();
        for (; token is null; token = ScanSingleToken())
        {
        }

        return token;
    }

    public bool IsExhausted { get; private set; }
    private Token? _current;
    public bool MoveNext()
    {
        if (IsExhausted)
        {
            return false;
        }
        _current = ScanNonNullToken();
        if (_current.Type == TokenType.Eof)
        {
            IsExhausted = true;
            _source.Dispose();
        }

        return true;
    }

    public Token GetAndMoveNext()
    {
        var current = Current;
        MoveNext();
        return current;
    }

    void IEnumerator.Reset()
    {
        if (IsExhausted)
        {
            throw new NotSupportedException("This scanner is already exhausted, please create a new one.");
        }
    }

    public Token Current =>
        _current ?? throw new ScannerException("This scanner is disposed or not started yet.", 0, 0);

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        IsExhausted = true;
        _source.Dispose();
        _current = null;
        GC.SuppressFinalize(this);
    }
}