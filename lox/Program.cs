using Lox;
using Lox.Expressions;
using Lox.Visitors.Interpreter;
string x = """"
print "hhe";
"""";
Scanner sc = new(new StringReader(x));
Parser p = new(sc);
var exp = p.Parse();
InterpreterVisitor interpreter = new(null);
exp.Accept(interpreter);
Console.WriteLine();
