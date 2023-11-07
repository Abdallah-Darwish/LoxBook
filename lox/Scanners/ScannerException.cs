namespace Lox.Scanners;

public class ScannerException : LoxException
{
    public int Line { get; }
    public int Column { get; }
    public ScannerException(string message, int line, int column) : base(message)
    {
        Line = line;
        Column = column;
    }
}