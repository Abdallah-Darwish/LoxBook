using Lox.Utilities;
using Lox.Visitors;
using Lox.Parsers;
using Lox.Scanners;
using Lox.Visitors.Interpreters;
using Lox.Visitors.Interpreters.Environemnts;
string x = """"
var x = 1 == 10 or 10 == 10;
"""";
Scanner sc = new(new StringReader(x));
Parser p = new(sc);
Interpreter interpreter = new(new LoxEnvironment(), Console.Out);
StatementAstPrinter printer = new(new ExpressionAstPrinter(), Console.Out);
ParserAdapter ad = new(p, new IStatementVisitor[] { printer/*, interpreter*/ });
ad.Visit();


Console.WriteLine();
