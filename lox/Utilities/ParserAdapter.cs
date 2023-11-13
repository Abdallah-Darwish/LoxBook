﻿using Lox.Visitors;

namespace lox.Utilities;

public class ParserAdapter
{
    private readonly IParser _parser;
    private readonly ICollection<IStatementVisitor> _visitors;
    public ParserAdapter(IParser parser, ICollection<IStatementVisitor> visitors)
    {
        _parser = parser;
        _visitors = visitors;
    }
    public void Visit()
    {
        for (var expr = _parser.Parse(); expr is not null; expr = _parser.Parse())
        {
            foreach (var visitor in _visitors)
            {
                expr.Accept(visitor);
            }
        }
    }
}

