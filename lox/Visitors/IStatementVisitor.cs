using Lox.Statements;

namespace Lox.Visitors;

public interface IVisitor
{
	void Visit(ExpressionStatement e);
	void Visit(Print s);
}
