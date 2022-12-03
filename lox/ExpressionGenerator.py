from pathlib import Path
from typing import List, Tuple


class Expression:
    name: str
    types: List[Tuple[str, str]]

    def __init__(self, definition: str):
        self.name, self.types = definition.strip().split(":")
        self.name = self.name.strip()
        self.types = [
            tuple(j.strip() for j in i.split()) for i in self.types.split(",")
        ]

    def definition(self, base_name: str):
        types_str = ", ".join(f"{t[0]} {t[1]}" for t in self.types)
        return f"""
public record class {self.name}({types_str}) : {base_name}
{{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
}}
"""
    @property
    def visitor_method(self):
        return f"\tT Visit({self.name} e);"


def generate_ast(expressions: List[Expression], namespace: str, base_name: str) -> str:
    ast = "".join(e.definition(base_name) for e in expressions)
    return f"""using Lox.Visitors;
namespace {namespace};
public abstract record class {base_name}()
{{
    public abstract T Accept<T>(IVisitor<T> visitor);
}}
{ast}
""".strip()


def save_ast(p: Path, expressions: List[Expression], namespace: str, base_name: str):
    with open(p, "w") as fs:
        fs.write(generate_ast(expressions, namespace, base_name))
        fs.flush()


def generate_visitor_interface(expressions: List[Expression], namespace: str) -> str:
    visiting_methods = "\n".join(e.visitor_method for e in expressions)
    return f"""using Lox.Expressions;
namespace {namespace};
public interface IVisitor<T>
{{
{visiting_methods}
}}
"""


def save_visitor_interface(p: Path, expressions: List[Expression], namespace: str):
    with open(p, "w") as fs:
        fs.write(generate_visitor_interface(expressions, namespace))
        fs.flush()


def parse_ast_txt(definition: str) -> List[Expression]:
    return [Expression(l) for l in definition.splitlines() if l.strip()]


ast_txt = """
Ternary  : Expression Condition, Token QuestionMark, Expression Left, Token Colon, Expression Right
Binary   : Expression Left, Token Operator, Expression Right
Grouping : Expression Expression
Literal  : Token Value
Unary    : Token Operator, Expression Right
"""
ast_base_name = "Expression"
cd = Path(__file__).parent

expressions = parse_ast_txt(ast_txt)

save_ast(
    cd.joinpath(f"{ast_base_name}.cs"), expressions, "Lox.Expressions", ast_base_name
)
save_visitor_interface(
    cd.joinpath("Visitors").joinpath("IVisitor.cs"), expressions, "Lox.Visitors"
)
