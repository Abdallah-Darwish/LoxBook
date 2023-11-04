using Lox.Statements;

namespace Lox.Visitors;

public interface IStatementVisitor<T>
{
	T Visit(ExpressionStatement e);
	T Visit(Print s);
}
