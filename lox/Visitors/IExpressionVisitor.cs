using Lox.Expressions;

namespace Lox.Visitors;

public interface IVisitor<T>
{
	T Visit(Ternary s);
	T Visit(Binary s);
	T Visit(Grouping s);
	T Visit(Literal s);
	T Visit(Unary s);
}
