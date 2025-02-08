namespace PostgrestSharp.Abstract.Exceptions;

public class AlreadyExistsException(string message)  : Exception
{
    
    
    public override string ToString()
    {
        return message;
    }
}