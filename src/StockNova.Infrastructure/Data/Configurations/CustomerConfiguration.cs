using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.CustomerId);

        builder.Property(c => c.CustomerId).HasMaxLength(5).IsFixedLength();
        builder.Property(c => c.CompanyName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.ContactName).HasMaxLength(50);
        builder.Property(c => c.ContactTitle).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(120);
        builder.Property(c => c.City).HasMaxLength(50);
        builder.Property(c => c.Region).HasMaxLength(50);
        builder.Property(c => c.PostalCode).HasMaxLength(20);
        builder.Property(c => c.Country).HasMaxLength(50);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Fax).HasMaxLength(50);

        builder.HasIndex(c => c.CompanyName);
        builder.HasIndex(c => c.City);
        builder.HasIndex(c => c.Region);
        builder.HasIndex(c => c.PostalCode);
    }
}
