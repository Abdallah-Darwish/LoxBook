using Lox.Utilities;
using Lox.Visitors;
using Lox.Parsers;
using Lox.Scanners;
using Lox.Visitors.Interpreters;
using Lox.Visitors.Interpreters.Environments;
using Lox.Core;
using Lox.Visitors.Resolvers;

string x = """"
var fn = fun(name)
{
    return "Hello " + name;
};
print fn("Abdallah");
print fn;
"""";
Scanner sc = new(new StringReader(x));
Parser p = new(sc);
TextOutputSync sync = new(Console.Out);
Dictionary<Token, ResolvedToken> resolverStore = [];
Resolver resolver = new(LoxEnvironment.Globals.Select(g => g.Name.Text), resolverStore);
Interpreter interpreter = new(LoxEnvironment.GlobalEnvironment, sync, resolverStore);
TextWriterAstPrinter printer = new(Console.Out, new AstPrinter(new ExpressionHasStatement()));
ParserAdapter ad = new(p, new IStatementVisitor[] { printer, resolver, interpreter });
ad.Visit();


Console.WriteLine();
