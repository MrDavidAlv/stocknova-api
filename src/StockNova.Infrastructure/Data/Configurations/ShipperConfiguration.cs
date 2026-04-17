using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockNova.Domain.Entities;

namespace StockNova.Infrastructure.Data.Configurations;

public class ShipperConfiguration : IEntityTypeConfiguration<Shipper>
{
    public void Configure(EntityTypeBuilder<Shipper> builder)
    {
        builder.ToTable("Shippers");
        builder.HasKey(s => s.ShipperId);
        builder.Property(s => s.ShipperId).UseIdentityColumn();

        builder.Property(s => s.CompanyName).IsRequired().HasMaxLength(80);
        builder.Property(s => s.Phone).HasMaxLength(50);
    }
}
