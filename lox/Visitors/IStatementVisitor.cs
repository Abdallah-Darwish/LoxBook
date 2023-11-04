using Lox.Core;

namespace Lox.Visitors;

public interface IVisitor
{
	void Visit(ExpressionStatement e);
	void Visit(Print s);
	void Visit(VariableStatement s);
}
