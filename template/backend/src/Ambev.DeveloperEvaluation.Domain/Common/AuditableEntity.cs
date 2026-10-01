namespace Ambev.DeveloperEvaluation.Domain.Common;

// Audit timestamps are filled by the persistence layer on save, so entities never need to update them.
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
}
