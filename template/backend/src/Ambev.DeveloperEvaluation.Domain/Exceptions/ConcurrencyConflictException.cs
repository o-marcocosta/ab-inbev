namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

// Keeps the persistence technology (e.g. EF Core's DbUpdateConcurrencyException) out of the upper layers.
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
