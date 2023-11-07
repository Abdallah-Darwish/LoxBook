[Serializable]
public class LoxException : Exception
{
    public LoxException() { }
    public LoxException(string message) : base(message) { }
    public LoxException(string message, Exception inner) : base(message, inner) { }
    protected LoxException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}