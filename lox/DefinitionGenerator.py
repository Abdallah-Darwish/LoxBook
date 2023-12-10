from pathlib import Path
from typing import List, Tuple, Callable
from enum import Enum, auto

BASE_NAMESPACE = 'Lox'
BASE_PLACEHOLDER = '$base$'
TAB = ' ' * 4
class VisitorVariant(Enum):
    TYPED = auto()
    NOT_TYPED = auto()
    ALL = auto()

    def call_method(self, m: Callable, *args, **kwargs) -> str:
        return "\n".join([m(*args, **{**kwargs, 'visitor_variant':VisitorVariant.NOT_TYPED }), m(*args, **{**kwargs, 'visitor_variant':VisitorVariant.TYPED })]) if self == VisitorVariant.ALL else m(*args, **{**kwargs, 'visitor_variant':self })
    
    def get_return_type(self):
        return 'T' if self == VisitorVariant.TYPED else 'void'
    
    def get_type_argument(self):
        return '<T>' if self == VisitorVariant.TYPED else ''

def make_namespace(s: str) -> str:
    return f'{BASE_NAMESPACE}.{s}'

class SyntaxNode:
    name: str
    base_name: str
    types: List[Tuple[str, str]]
    class_name: str

    def __init__(self, definition: str, base_name: str):
        self.name, self.types = definition.strip().split(":")
        self.name = self.name.strip()
        self.types = [
            tuple(j.strip() for j in i.split()) for i in self.types.split(",")
        ] if self.types else []
        self.base_name = base_name
        self.class_name = f'{self.name}{self.base_name}'

    def _generate_accept_method(self, *, visitor_variant: VisitorVariant) -> str:
        assert visitor_variant != VisitorVariant.ALL
        return f"{TAB}public override {visitor_variant.get_return_type()} Accept{visitor_variant.get_type_argument()}(I{self.base_name}Visitor{visitor_variant.get_type_argument()} visitor) => visitor.Visit(this);"

    def generate_definition(self, visitor_variant: VisitorVariant):
        types_str = ", ".join(f"{t[0]} {t[1]}" for t in self.types)
        accept_str = visitor_variant.call_method(self._generate_accept_method)
        return f"""
public record class {self.class_name}({types_str}) : {self.base_name}
{{
{accept_str}
}}
"""
    
    def _generate_visit_method(self, *, visitor_variant: VisitorVariant) -> str:
        assert visitor_variant != VisitorVariant.ALL
        var_name = 's' if 'statement' in self.class_name.lower() else 'e'
        return f"{TAB}{visitor_variant.get_return_type()} Visit({self.class_name} {var_name});"

    def generate_visit_method(self, visitor_variant: VisitorVariant):
        return visitor_variant.call_method(self._generate_visit_method)
        

class Ast:
    namespace: str
    base_name: str
    nodes: List[SyntaxNode]
    visitor_variant: VisitorVariant
    definition_additional_namespaces: List[str] | None

    def __init__(self, namespace: str, base_name: str, definition: str, visitor_variant: VisitorVariant, definition_additional_namespaces: List[str] | None = None):
        self.namespace, self.base_name = namespace, base_name.strip()
        self.nodes = [SyntaxNode(l, self.base_name) for l in definition.replace(BASE_PLACEHOLDER, self.base_name).splitlines() if l.strip()]
        self.visitor_variant = visitor_variant
        self.definition_additional_namespaces = definition_additional_namespaces

    def _generate_base_visit_method(self, *, visitor_variant: VisitorVariant):
        assert visitor_variant != VisitorVariant.ALL
        return f"{TAB}public abstract {visitor_variant.get_return_type()} Accept{visitor_variant.get_type_argument()}(I{self.base_name}Visitor{visitor_variant.get_type_argument()} visitor);"

    def _generate_base_definition(self):
        accept_str = self.visitor_variant.call_method(self._generate_base_visit_method)
        return f"""public abstract record class {self.base_name}()
{{
{accept_str}
}}"""

    def generate_definition(self):
        ast = "".join(e.generate_definition(self.visitor_variant) for e in self.nodes)
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

    def _generate_visitor_interface(self, *, visitor_variant: VisitorVariant) -> str:
        visiting_methods = "\n".join(e.generate_visit_method(visitor_variant) for e in self.nodes)
        return f"""public interface I{self.base_name}Visitor{visitor_variant.get_type_argument()}
{{
{visiting_methods}
}}"""

    def generate_visitor_interface(self) -> str:
        interfaces_str = self.visitor_variant.call_method(self._generate_visitor_interface)
        
        return f"""using {make_namespace(self.namespace)};

namespace {make_namespace('Visitors')};

{interfaces_str}"""
    
    def save_visitor_interface(self, p: Path):
        with open(p, "w") as fs:
            fs.write(self.generate_visitor_interface())
            fs.flush()

    def save_definition_and_visitor(self, d: Path | None = None):
        d = d or Path(__file__).parent
        self.save_definition(d.joinpath(self.namespace).joinpath(f"{self.base_name}.cs"))
        self.save_visitor_interface(d.joinpath("Visitors").joinpath(f"I{self.base_name}Visitor.cs"))

expressions_ast_txt = """
Ternary     : $base$ Condition, $base$ Left, $base$ Right
Binary      : $base$ Left, Token Operator, $base$ Right
Grouping    : $base$ Expression
Literal     : Token Value
Unary       : Token Operator, $base$ Right
Variable    : Token Name
Assignment  : Token Name, $base$ Value
Call        : $base$ Callee, Token RightParentheses, $base$[] Arguments
Lambda      : Token Fun, IReadOnlyList<Token> Parameters, IReadOnlyList<Statement> Body
Get         : $base$ Instance, Token Name
Set         : $base$ Instance, Token Name, $base$ Value
This        : Token This
"""
expressions = Ast('Core', 'Expression', expressions_ast_txt, VisitorVariant.ALL)

statements_ast_txt = """
Expression  : Expression Expression
Print       : Expression Expression
Variable    : Token Name, Expression? Initializer
Block       : IReadOnlyList<$base$> Statements
If          : Expression Condition, $base$ Then, $base$? Else
While       : Expression Condition, $base$ Body
Break       :
Function    : Token Name, IReadOnlyList<Token> Parameters, FunctionType Type, IReadOnlyList<$base$> Body
Return      : Token Return, Expression? Value
Class       : Token Name, IReadOnlyList<Function$base$> Methods
"""
statements = Ast('Core', 'Statement', statements_ast_txt, VisitorVariant.ALL)

expressions.save_definition_and_visitor()
statements.save_definition_and_visitor()
