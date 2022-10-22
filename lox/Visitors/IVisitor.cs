using Lox.Expressions;

namespace Lox.Visitors;
public interface IVisitor<T>
{
    T Visit(Expression e);
}