using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

// Reference to an entity owned by another bounded context (External Identities pattern). The denormalized
// description keeps the sale readable without querying the owning context.
public abstract record ExternalReference
{
    public Guid Id { get; }
    public string Name { get; }

    protected ExternalReference(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException($"{GetType().Name} id must be provided.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException($"{GetType().Name} name must be provided.");

        Id = id;
        Name = name.Trim();
    }
}

public sealed record CustomerRef : ExternalReference
{
    public CustomerRef(Guid id, string name) : base(id, name) { }
}

public sealed record BranchRef : ExternalReference
{
    public BranchRef(Guid id, string name) : base(id, name) { }
}

public sealed record ProductRef : ExternalReference
{
    public ProductRef(Guid id, string name) : base(id, name) { }
}
