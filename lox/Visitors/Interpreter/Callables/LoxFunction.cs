using Lox.Core;
using Lox.Visitors.Interpreters.Environments;
using Lox.Visitors.Interpreters.Exceptions;



namespace Lox.Visitors.Interpreters.Callables;

public class LoxFunction(FunctionStatement func) : ILoxCallable
{
    private readonly FunctionStatement _func = func;
    public int Arity => _func.Parameters.Count;
    public object? Call(Interpreter interpreter, object?[] arguments)
    {
        LoxEnvironment env = new(interpreter._globals as LoxEnvironment);
        foreach (var (name, val) in _func.Parameters.Zip(arguments))
        {
            env.Define(name, val);
        }
        try
        {
            interpreter.ExecuteBlock(_func.Body, env);
        }
        catch (ReturnException ex)
        {
            return ex.Value;
        }
        return null;
    }
}