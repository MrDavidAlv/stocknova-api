using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.EmployeeId);
        builder.Property(e => e.EmployeeId).UseIdentityColumn();

        builder.Property(e => e.LastName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Title).HasMaxLength(50);
        builder.Property(e => e.TitleOfCourtesy).HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(120);
        builder.Property(e => e.City).HasMaxLength(50);
        builder.Property(e => e.Region).HasMaxLength(50);
        builder.Property(e => e.PostalCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(50);
        builder.Property(e => e.HomePhone).HasMaxLength(50);
        builder.Property(e => e.Extension).HasMaxLength(10);
        builder.Property(e => e.Photo).HasColumnType("bytea");
        builder.Property(e => e.Notes).HasColumnType("text");

        builder.HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ReportsTo)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.LastName, e.FirstName });
        builder.HasIndex(e => e.PostalCode);
    }
}
