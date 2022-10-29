using Lox.Expressions;
namespace Lox.Visitors;
public interface IVisitor<out T>
{
	T Visit(Binary e);
	T Visit(Grouping e);
	T Visit(Literal e);
	T Visit(Unary e);
}
