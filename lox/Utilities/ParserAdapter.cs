using Lox.Scanners;
using Lox.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        try {
            while (true)
            {
                var exp = _parser.Parse();
                foreach (var visitor in _visitors)
                {
                    exp.Accept(visitor);
                }
            }
        }
        catch (ScannerException ex) when (ex.Message.Contains("disposed or not started") && ex is { Line : 0, Column : 0})
        {}
    }
}

