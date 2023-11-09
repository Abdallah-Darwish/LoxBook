using Lox.Parsers;
using Lox.Scanners;
using Lox.Visitors;
using Lox.Visitors.Interpreters;
using Lox.Visitors.Interpreters.Environemnts;
string x = """"
var y = 1;
y = "x";
"""";
Scanner sc = new(new StringReader(x));
Parser p = new(sc);
Interpreter interpreter = new(new LoxEnvironemnt(), null);
while (!sc.IsExhausted)
{
    var exp = p.Parse();
    exp.Accept(interpreter);
}

Console.WriteLine();
