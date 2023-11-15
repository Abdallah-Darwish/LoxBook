using Lox.Core;
using Lox.Visitors;


namespace lox.Visitors
{
    public class StatementAstPrinter : IStatementVisitor
    {
        private readonly TextWriter _output;
        private readonly IExpressionVisitor<string> _expressionVisitor;
        private void Parenthesize(string? name, IEnumerable<Statement?>? statements = null, IEnumerable<Expression?>? expressions = null, IEnumerable<Token?>? tokens = null)
        {
            _output.Write('{');
            if (name is not null && !string.IsNullOrWhiteSpace(name))
            {
                _output.Write(name);
            }
            if (statements is not null)
            {
                foreach (var s in statements)
                {
                    if (s is null) { continue; }
                    _output.Write(' ');
                    s.Accept(this);
                }
            }
            if (expressions is not null)
            {
                foreach (var e in expressions)
                {
                    if (e is null) { continue; }
                    _output.Write(' ');
                    _output.Write(e.Accept(_expressionVisitor));
                }
            }
            if (tokens is not null)
            {
                foreach (var t in tokens)
                {
                    if (t is null) { continue; }
                    _output.Write(' ');
                    _output.Write(t.Text);
                }
            }
            if (name is not null && !char.IsWhiteSpace(name[^1]))
            {
                _output.Write(' ');
            }
            _output.WriteLine('}');
        }

        public StatementAstPrinter(IExpressionVisitor<string> expressionVisitor, TextWriter output)
        {
            _output = output;
            _expressionVisitor = expressionVisitor;
        }

        public void Visit(ExpressionStatement s) => Parenthesize(null, expressions: new[] { s.Expression });

        public void Visit(PrintStatement s) => Parenthesize("print", expressions: new[] { s.Expression });

        public void Visit(VariableStatement s) => Parenthesize("var", expressions: new[] { s.Initializer }, tokens: new[] { s.Name });

        public void Visit(BlockStatement s) => Parenthesize("block\n", statements: s.Statements);

        public void Visit(IfStatement s) => Parenthesize("if\n", statements: new Statement?[] { s.Then, s.Else });

        public void Visit(WhileStatement s) => Parenthesize("while\n", statements: new Statement?[] { s.Body });
    }
}
