using Lox.Core;

namespace Lox.Visitors;

public interface IStatementVisitor
{
	void Visit(ExpressionStatement s);
	void Visit(Print e);
	void Visit(VariableStatement s);
}
public interface IStatementVisitor<T>
{
	T Visit(ExpressionStatement s);
	T Visit(Print e);
	T Visit(VariableStatement s);
}