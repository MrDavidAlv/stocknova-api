using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.CategoryId);
        builder.Property(c => c.CategoryId).UseIdentityColumn();

        builder.Property(c => c.CategoryName).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Description).HasColumnType("text");
        builder.Property(c => c.Picture).HasColumnType("bytea");

        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.CategoryName).IsUnique();
    }
}
