using Lox;
using Lox.Expressions;
using Lox.Visitors.Interpreter;
string x = """ "nil" - "nil" """;
Scanner sc = new(new StringReader(x));
Parser p = new(sc);
var exp = p.Parse();
InterpreterVisitor interpreter = new();
Console.WriteLine(exp.Accept(interpreter));
