using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class HogarConfiguration : IEntityTypeConfiguration<Hogar>
{
    public void Configure(EntityTypeBuilder<Hogar> e)
    {
        e.HasKey(h => h.Id);
        e.Property(h => h.Nombre).IsRequired().HasMaxLength(100);
        e.Property(h => h.ClienteId).IsRequired();
        e.OwnsOne(h => h.Direccion, d =>
        {
            d.Property(x => x.Calle).HasMaxLength(200);
            d.Property(x => x.Ciudad).IsRequired().HasMaxLength(100);
            d.Property(x => x.Pais).IsRequired().HasMaxLength(50);
        });
        e.OwnsOne(h => h.Ubicacion);
        e.HasMany(h => h.Habitaciones).WithOne().HasForeignKey(ha => ha.HogarId);
        e.HasMany(h => h.Dispositivos).WithOne().HasForeignKey(di => di.HogarId);
        e.Navigation(h => h.Habitaciones).UsePropertyAccessMode(PropertyAccessMode.Field);
        e.Navigation(h => h.Dispositivos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
