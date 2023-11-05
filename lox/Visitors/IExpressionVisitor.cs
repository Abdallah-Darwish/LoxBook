using Lox.Core;

namespace Lox.Visitors;

public interface IExpressionVisitor<T>
{
	T Visit(Ternary e);
	T Visit(Binary e);
	T Visit(Grouping e);
	T Visit(Literal e);
	T Visit(Unary e);
	T Visit(Variable e);
}