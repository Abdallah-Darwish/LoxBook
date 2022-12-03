using Lox;
using Lox.Expressions;
using Lox.Visitors;
string x = @"var bool = 1 == 1;
var d = 2;
";
Scanner sc = new(new StringReader(x));
while (sc.MoveNext())
{
    Console.WriteLine(sc.Current);
}
