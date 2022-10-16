// See https://aka.ms/new-console-template for more information

using lox;
string src = "var $ = \"whhheooo\";";

Scanner sc = new(new StringReader(src));
foreach (var token in sc.GetTokens())
{
    Console.WriteLine(token?.ToString() ?? "Comment");
}
