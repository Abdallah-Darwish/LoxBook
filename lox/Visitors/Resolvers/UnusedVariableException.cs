using Lox.Core;

namespace Lox.Visitors.Resolvers.Exceptions;

public class UnusedVariableException(Token variable) : ResolverException($"Unused variable {variable.Text} defined at {variable.Line}, {variable.Column}", variable)
{ }