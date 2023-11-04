from pathlib import Path
from typing import List, Tuple

BASE_NAMESPACE = 'Lox'
BASE_PLACEHOLDER = '$base$'

def make_namespace(s: str) -> str:
    return f'{BASE_NAMESPACE}.{s}'

class SyntaxNode:
    name: str
    types: List[Tuple[str, str]]

    def __init__(self, definition: str):
        self.name, self.types = definition.strip().split(":")
        self.name = self.name.strip()
        self.types = [
            tuple(j.strip() for j in i.split()) for i in self.types.split(",")
        ]

    def generate_definition(self, base_name: str):
        types_str = ", ".join(f"{t[0]} {t[1]}" for t in self.types)
        return f"""
public record class {self.name}({types_str}) : {base_name}
{{
    public override T Accept<T>(I{base_name}Visitor<T> visitor) => visitor.Visit(this);
}}
"""
    def generate_visitor_method(self):
        var_name = 'e' if 'expr' in self.name.lower() else 's'
        return f"\tT Visit({self.name} {var_name});"

class Ast:
    namespace: str
    base_name: str
    nodes: List[SyntaxNode]
    definition_additional_namespaces: List[str] | None

    def __init__(self, namespace: str, base_name: str, definition: str, definition_additional_namespaces: List[str] | None = None):
        self.namespace, self.base_name = namespace, base_name
        self.nodes = [SyntaxNode(l) for l in definition.replace(BASE_PLACEHOLDER, base_name).splitlines() if l.strip()]
        self.definition_additional_namespaces = definition_additional_namespaces

    def _generate_base_definition(self):
        return f"""public abstract record class {self.base_name}()
{{
    public abstract T Accept<T>(I{self.base_name}Visitor<T> visitor);
}}"""

    def generate_definition(self):
        ast = "".join(e.generate_definition(self.base_name) for e in self.nodes)
        additional_usings = '\n'.join([f'using {ns};' for ns in self.definition_additional_namespaces]) + '\n' if self.definition_additional_namespaces else ''
        return f"""{additional_usings}using {make_namespace('Visitors')};

namespace {make_namespace(self.namespace)};

{self._generate_base_definition()}
{ast}
""".strip()

    def save_definition(self, p: Path):
        with open(p, "w") as fs:
            fs.write(self.generate_definition())
            fs.flush()

    def generate_visitor_interface(self) -> str:
        visiting_methods = "\n".join(e.generate_visitor_method() for e in self.nodes)
        return f"""using {make_namespace(self.base_name + 's')};

namespace {make_namespace('Visitors')};

public interface I{self.base_name}Visitor<T>
{{
{visiting_methods}
}}
"""
    
    def save_visitor_interface(self, p: Path):
        with open(p, "w") as fs:
            fs.write(self.generate_visitor_interface())
            fs.flush()

    def save_definition_and_visitor(self, d: Path | None = None):
        d = d or Path(__file__).parent
        self.save_definition(d.joinpath(f"{self.base_name}.cs"))
        self.save_visitor_interface(d.joinpath("Visitors").joinpath(f"I{self.base_name}Visitor.cs"))

expressions_ast_txt = """
Ternary  : $base$ Condition, Token QuestionMark, $base$ Left, Token Colon, $base$ Right
Binary   : $base$ Left, Token Operator, $base$ Right
Grouping : $base$ Expression
Literal  : Token Value
Unary    : Token Operator, $base$ Right
"""
expressions = Ast('Expressions', 'Expression', expressions_ast_txt)

statements_ast_txt = """
ExpressionStatement : Expression Expression
Print               : Expression Expression
"""
statements = Ast('Statements', 'Statement', statements_ast_txt, [make_namespace('Expressions')])

expressions.save_definition_and_visitor()
statements.save_definition_and_visitor()