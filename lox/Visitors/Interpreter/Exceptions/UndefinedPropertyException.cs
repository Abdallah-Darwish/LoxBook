using Lox.Core;
using Lox.Visitors.Interpreters.Callables;
namespace Lox.Visitors.Interpreters.Exceptions;


public class UndefinedPropertyException(LoxInstance sourceInstance, string propertyName, GetExpression? source = null) :
    RuntimeException($"Undefined property '{propertyName}' on an instance of type {sourceInstance.Klass.Name}")
{
    public LoxInstance Instance { get; } = sourceInstance;
    public string PropertyName { get; } = propertyName;
    public Expression SourceExpression { get; } = source;
}