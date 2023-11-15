using Lox.Utilities;
using Lox.Visitors;
using Lox.Parsers;
using Lox.Scanners;
using Lox.Visitors.Interpreters;
using Lox.Visitors.Interpreters.Environemnts;
string x = """"
var x = 1;
while(x < 3)
{
    print x;
    x = x + 1;
    while(x > 5)
    {
        if (x == 2)
            print 1;
    }
}
"""";
Scanner sc = new(new StringReader(x));
Parser p = new(sc);
Interpreter interpreter = new(new LoxEnvironment(), Console.Out);
ConsoleStatementAstPrinter printer = new(new StatementAstPrinter(new ExpressionAstPrinter()));
ParserAdapter ad = new(p, new IStatementVisitor[] { printer, interpreter });
ad.Visit();


Console.WriteLine();
