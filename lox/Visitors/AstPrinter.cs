using Lox.Expressions;

namespace Lox.Visitors;

public class AstPrinter : IVisitor<string>
{
    public string Visit(Binary e)
    {
        throw new NotImplementedException();
    }

    public string Visit(Grouping e)
    {
        throw new NotImplementedException();
    }

    public string Visit(Literal e)
    {
        throw new NotImplementedException();
    }

    public string Visit(Unary e)
    {
        throw new NotImplementedException();
    }
}