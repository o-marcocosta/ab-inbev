using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        // ---------------------------------------------------------------
        // Table
        // ---------------------------------------------------------------
        builder.ToTable("SaleItems", t =>
            t.HasCheckConstraint("CK_SaleItems_UnitPrice", "\"UnitPrice\" > 0"));

        // ---------------------------------------------------------------
        // Own columns
        // ---------------------------------------------------------------
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("Id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(i => i.Quantity)
            .HasColumnName("Quantity")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnName("UnitPrice")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.DiscountRate)
            .HasColumnName("DiscountRate")
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(i => i.DiscountAmount)
            .HasColumnName("DiscountAmount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.TotalAmount)
            .HasColumnName("TotalAmount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.IsCancelled)
            .HasColumnName("IsCancelled")
            .IsRequired();

        builder.Property(i => i.CancelledAt)
            .HasColumnName("CancelledAt");

        // Computed in the domain (Quantity * UnitPrice); not persisted.
        builder.Ignore(i => i.GrossAmount);

        // ---------------------------------------------------------------
        // External references (owned, stored as columns of this table)
        // ---------------------------------------------------------------
        builder.OwnsOne(i => i.Product, product =>
        {
            product.Property(p => p.Id)
                .HasColumnName("ProductId")
                .IsRequired();

            product.Property(p => p.Name)
                .HasColumnName("ProductName")
                .HasMaxLength(200)
                .IsRequired();
        });
        builder.Navigation(i => i.Product).IsRequired();

        // ---------------------------------------------------------------
        // Relationships
        // ---------------------------------------------------------------
        // FK to Sales is configured on the aggregate root (SaleConfiguration).
        builder.Property(i => i.SaleId)
            .HasColumnName("SaleId")
            .IsRequired();

        // ---------------------------------------------------------------
        // Indexes
        // ---------------------------------------------------------------
        builder.OwnsOne(i => i.Product).HasIndex(p => p.Id);
    }
}
