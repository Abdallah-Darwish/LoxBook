using Lox.Visitors.Interpreters;

namespace Lox.Utilities;

public class TextOutputSync : IOutputSync<object?>
{
    private readonly TextWriter _output;
    public TextOutputSync(TextWriter output) => _output = output;
    public void Push(object? value)
    {
        if (value is null)
        {
            _output.WriteLine("$nil$");
        }
        else
        {
            _output.WriteLine(value);
        }
    }
}