using Lox.Visitors.Interpreters;

namespace Lox.Tests.Utilities;

public class BufferOutputSync<T> : IOutputSync<T>
{
    private readonly List<T> _buffer = new();
    public IReadOnlyList<T> Buffer => _buffer;
    public void Push(T value) => _buffer.Add(value);
}