using Lox.Core;
namespace Lox.Visitors.Interpreters.Exceptions;

public class BreakException(BreakStatement sourceStatement) : RuntimeException
{
    public BreakStatement SourceStatement { get; } = sourceStatement;
}