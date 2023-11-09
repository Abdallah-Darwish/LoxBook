using Lox.Core;
using Lox.Visitors;


namespace lox.Visitors
{
    public class StatementAstPrinter : IStatementVisitor
    {
        private readonly TextWriter _output;
        private readonly IExpressionVisitor<string> _expressionVisitor;
        private void Parenthesize(string? name, Expression?[]? expressions = null, Token?[]? tokens = null)
        {
            _output.Write('{');
            if (name is not null && !string.IsNullOrWhiteSpace(name))
            {
                _output.Write(name);
            }
            if (expressions is not null && expressions.Length > 0)
            {
                foreach (var e in expressions)
                {
                    if (e is null) { continue; }
                    _output.Write(' ');
                    _output.Write(e.Accept(_expressionVisitor));
                }
            }
            if (tokens is not null && tokens.Length > 0)
            {
                foreach (var t in tokens)
                {
                    if (t is null) { continue; }
                    _output.Write(' ');
                    _output.Write(t.Text);
                }
            }
            _output.WriteLine(" }");
        }

        public StatementAstPrinter(IExpressionVisitor<string> expressionVisitor, TextWriter output)
        {
            _output = output;
            _expressionVisitor = expressionVisitor;
        }

        public void Visit(ExpressionStatement s) => Parenthesize(null, expressions: new [] { s.Expression });

        public void Visit(PrintStatement s) => Parenthesize("print", expressions: new [] { s.Expression });

        public void Visit(VariableStatement s) => Parenthesize("=", expressions: new[] { s.Initializer }, tokens: new[] {s.Name});
    }
}
