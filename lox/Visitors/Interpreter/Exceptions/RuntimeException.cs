namespace Lox.Visitors.Interpreters.Exceptions;
public class RuntimeException : LoxException
{
    public RuntimeException() { }
    public RuntimeException(string message) : base(message) { }
    public RuntimeException(string message, Exception inner) : base(message, inner) { }
}