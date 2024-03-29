namespace Lox.Core;

public enum TokenType
{
    // Single-character tokens.
    LeftParentheses, RightParentheses, LeftBrace, RightBrace,
    Comma, Dot, Minus, Plus, Semicolon, Slash, Star, QuestionMark, Colon,

    // One or two character tokens.
    Bang, BangEqual,
    Equal, EqualEqual,
    Greater, GreaterEqual,
    Less, LessEqual,

    // Literals.
    Identifier, String, Number,

    // Keywords.
    And, Class, Else, False, Fun, For, If, Nil, Or,
    Print, Return, Super, This, True, Var, While, Break,

    Eof
}