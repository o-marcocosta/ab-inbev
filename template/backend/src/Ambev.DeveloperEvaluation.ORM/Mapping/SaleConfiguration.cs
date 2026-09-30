using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : AuditableEntityConfiguration<Sale>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Sale> builder)
    {
        // ---------------------------------------------------------------
        // Table
        // ---------------------------------------------------------------
        builder.ToTable("Sales", t =>
            t.HasCheckConstraint("CK_Sales_TotalAmount", "\"TotalAmount\" >= 0"));

        // ---------------------------------------------------------------
        // Own columns (Id, CreatedAt and UpdatedAt come from AuditableEntityConfiguration)
        // ---------------------------------------------------------------
        builder.Property(s => s.Id).ValueGeneratedNever();

        // Identity column (backed by an implicit Postgres sequence); never written by the application.
        builder.Property(s => s.SaleNumber)
            .HasColumnName("SaleNumber")
            .UseIdentityAlwaysColumn()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(s => s.SaleDate)
            .HasColumnName("SaleDate")
            .IsRequired();

        builder.Property(s => s.TotalAmount)
            .HasColumnName("TotalAmount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.IsCancelled)
            .HasColumnName("IsCancelled")
            .IsRequired();

        builder.Property(s => s.CancelledAt)
            .HasColumnName("CancelledAt");

        // Computed in the domain from the active items; not persisted.
        builder.Ignore(s => s.GrossAmount);
        builder.Ignore(s => s.DiscountAmount);

        // Optimistic concurrency mapped to Postgres' xmin system column,.
        builder.Property<uint>("Version")
            .HasColumnName("xmin")
            .IsRowVersion();

        // ---------------------------------------------------------------
        // External references (owned, stored as columns of this table)
        // ---------------------------------------------------------------
        builder.OwnsOne(s => s.Customer, customer =>
        {
            customer.Property(c => c.Id)
                .HasColumnName("CustomerId")
                .IsRequired();

            customer.Property(c => c.Name)
                .HasColumnName("CustomerName")
                .HasMaxLength(150)
                .IsRequired();
        });
        builder.Navigation(s => s.Customer).IsRequired();

        builder.OwnsOne(s => s.Branch, branch =>
        {
            branch.Property(b => b.Id)
                .HasColumnName("BranchId")
                .IsRequired();

            branch.Property(b => b.Name)
                .HasColumnName("BranchName")
                .HasMaxLength(150)
                .IsRequired();
        });
        builder.Navigation(s => s.Branch).IsRequired();

        // ---------------------------------------------------------------
        // Relationships
        // ---------------------------------------------------------------
        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---------------------------------------------------------------
        // Indexes
        // ---------------------------------------------------------------
        builder.HasIndex(s => s.SaleNumber).IsUnique();
        builder.HasIndex(s => s.SaleDate);
        builder.OwnsOne(s => s.Customer).HasIndex(c => c.Id);
        builder.OwnsOne(s => s.Branch).HasIndex(b => b.Id);
    }
}
