// See https://aka.ms/new-console-template for more information

using lox;
string src = @"var saif = 22;
var x = 1 + 3;";

Scanner sc = new(new StringReader(src));
foreach (var token in sc.GetTokens())
{
    Console.WriteLine(token?.ToString() ?? "Comment");
}
