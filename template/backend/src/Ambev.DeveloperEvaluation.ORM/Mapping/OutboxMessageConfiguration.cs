using Ambev.DeveloperEvaluation.ORM.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("Id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(m => m.Type)
            .HasColumnName("Type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.Payload)
            .HasColumnName("Payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(m => m.OccurredAt)
            .HasColumnName("OccurredAt")
            .IsRequired();

        builder.Property(m => m.ProcessedAt)
            .HasColumnName("ProcessedAt");

        builder.Property(m => m.Attempts)
            .HasColumnName("Attempts")
            .IsRequired();

        builder.Property(m => m.LastError)
            .HasColumnName("LastError");

        builder.HasIndex(m => m.OccurredAt)
            .HasFilter("\"ProcessedAt\" IS NULL");
    }
}
