namespace Lox.Visitors.Interpreters.Environemnts;

public class Uninitialized
{
    public static Uninitialized Instance { get; } = new();
    private Uninitialized() { }
}