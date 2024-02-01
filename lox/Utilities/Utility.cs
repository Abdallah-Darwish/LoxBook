using Lox.Core;

namespace Lox.Utilities;

public static class Utility
{
    public static Token SuperToThis(Token super) => super with { Type = Token.This.Type, Lexeme = Token.This.Lexeme };
    public static Token GetClassSuperToken(ClassStatement klass) => Token.Super with { Line = klass.Super.Line, Column = klass.Super.Column };
}