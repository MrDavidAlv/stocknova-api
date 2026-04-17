using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.OrderId);
        builder.Property(o => o.OrderId).UseIdentityColumn();

        builder.Property(o => o.CustomerId).HasMaxLength(5).IsFixedLength();
        builder.Property(o => o.Freight).HasColumnType("decimal(18,2)");
        builder.Property(o => o.ShipName).HasMaxLength(80);
        builder.Property(o => o.ShipAddress).HasMaxLength(120);
        builder.Property(o => o.ShipCity).HasMaxLength(50);
        builder.Property(o => o.ShipRegion).HasMaxLength(50);
        builder.Property(o => o.ShipPostalCode).HasMaxLength(20);
        builder.Property(o => o.ShipCountry).HasMaxLength(50);

        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Employee)
            .WithMany(e => e.Orders)
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.ShipViaNavigation)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.ShipVia)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.EmployeeId);
        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.ShippedDate);
        builder.HasIndex(o => o.ShipPostalCode);
    }
}
