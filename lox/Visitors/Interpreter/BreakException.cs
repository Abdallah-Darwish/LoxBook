using Lox.Core;
namespace Lox.Visitors.Interpreters;

public class BreakException : RuntimeException
{
    public BreakStatement SourceStatement { get; }
    public BreakException(BreakStatement sourceStatement) => SourceStatement = sourceStatement;
}