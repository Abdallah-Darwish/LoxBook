using Lox.Utilities;
using Lox.Visitors;
using Lox.Parsers;
using Lox.Scanners;
using Lox.Visitors.Interpreters;
using Lox.Visitors.Interpreters.Environemnts;
string x = """"
for(var i = 0; i < 5; i = i + 1)
{
    var j = i;
    while(true)
    {
        if(j <= 0)
        {
            break;
        }
        print j;
        j = j - 1;
    }
}
"""";
Scanner sc = new(new StringReader(x));
Parser p = new(sc);
TextOutputSync sync = new(Console.Out);
Interpreter interpreter = new(new LoxEnvironment(), sync);
ConsoleStatementAstPrinter printer = new(new StatementAstPrinter(new ExpressionAstPrinter()));
ParserAdapter ad = new(p, new IStatementVisitor[] { printer, interpreter });
ad.Visit();


Console.WriteLine();
