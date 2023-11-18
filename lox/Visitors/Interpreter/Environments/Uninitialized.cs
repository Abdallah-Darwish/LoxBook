namespace Lox.Visitors.Interpreters.Environments;

public class Uninitialized
{
    public static Uninitialized Instance { get; } = new();
    private Uninitialized() { }
}