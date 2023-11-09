using lox.Utilities;
using lox.Visitors;
using Lox.Parsers;
using Lox.Scanners;
using Lox.Visitors;
using Lox.Visitors.Interpreters;
using Lox.Visitors.Interpreters.Environemnts;
string x = """"
var y;
print y;
y = "x";
print "Welcome mr " + y;
"""";
Scanner sc = new(new StringReader(x));
Parser p = new(sc);
Interpreter interpreter = new(new LoxEnvironemnt(), Console.Out);
StatementAstPrinter printer = new(new ExpressionAstPrinter(), Console.Out);
ParserAdapter ad = new(p, new IStatementVisitor[] { printer, interpreter });
ad.Visit();


Console.WriteLine();
