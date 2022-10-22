from pathlib import Path
from typing import List


def define_expression(name: str, types: List[str], base_name: str) -> str:
    name = name.strip()
    types = ", ".join(i.strip() for i in types)
    return f"""
public record class {name}({types}) : {base_name}
{{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}}
"""


def generate_ast(definition: str, namespace: str, base_name: str) -> str:
    if not definition:
        return None

    definition = [l.strip().split(":") for l in definition.splitlines() if l.strip()]
    ast = [define_expression(i[0], i[1].split(","), base_name) for i in definition]
    ast = "".join(ast)
    return f"""using Lox.Visitors;
namespace {namespace};
public abstract record class {base_name}()
{{
    public abstract T Accept<T>(IVisitor<T> visitor);
}}
{ast}
""".strip()


def save_ast(p: Path, definition: str, namespace: str, base_name: str):
    with open(p, "w") as fs:
        fs.write(generate_ast(definition, namespace, base_name))
        fs.flush()


ast_definition = """
Binary   : Expression Left, Token Operator, Expression Right
Grouping : Expression Expression
Literal  : object Value
Unary    : Token Operator, Expression Right
"""
ast_base_name = "Expression"
cd = Path(__file__).parent

save_ast(
    cd.joinpath(f"{ast_base_name}.cs"), ast_definition, "Lox.Expressions", ast_base_name
)
