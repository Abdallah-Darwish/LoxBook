using Lox.Parsers;
using Lox.Visitors;

namespace Lox.Utilities;

public class ParserAdapter(IParser parser, ICollection<IStatementVisitor> visitors)
{
    private readonly IParser _parser = parser;
    private readonly ICollection<IStatementVisitor> _visitors = visitors;

    public void Visit()
    {
        for (var stmt = _parser.Parse(); stmt is not null; stmt = _parser.Parse())
        {
            foreach (var visitor in _visitors)
            {
                stmt.Accept(visitor);
            }
        }
    }
}

