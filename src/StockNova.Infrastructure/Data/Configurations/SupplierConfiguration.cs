using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.SupplierId);
        builder.Property(s => s.SupplierId).UseIdentityColumn();

        builder.Property(s => s.CompanyName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.ContactName).HasMaxLength(50);
        builder.Property(s => s.ContactTitle).HasMaxLength(50);
        builder.Property(s => s.Address).HasMaxLength(120);
        builder.Property(s => s.City).HasMaxLength(50);
        builder.Property(s => s.Region).HasMaxLength(50);
        builder.Property(s => s.PostalCode).HasMaxLength(20);
        builder.Property(s => s.Country).HasMaxLength(50);
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.Fax).HasMaxLength(50);
        builder.Property(s => s.HomePage).HasColumnType("text");

        builder.HasIndex(s => s.CompanyName);
        builder.HasIndex(s => s.PostalCode);
    }
}
