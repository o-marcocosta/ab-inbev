namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

public class InvalidQueryException : Exception
{
    public InvalidQueryException(string message) : base(message)
    {
    }
}
