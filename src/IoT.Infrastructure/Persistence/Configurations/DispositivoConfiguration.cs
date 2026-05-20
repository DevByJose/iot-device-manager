using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class DispositivoConfiguration : IEntityTypeConfiguration<Dispositivo>
{
    public void Configure(EntityTypeBuilder<Dispositivo> e)
    {
        e.HasKey(d => d.Id);
        e.Property(d => d.Nombre).IsRequired().HasMaxLength(100);
        e.Property(d => d.TipoDispositivo).IsRequired().HasMaxLength(50);
        e.Property(d => d.Estado).IsRequired().HasMaxLength(50);
        e.OwnsOne(d => d.Identificador, i =>
        {
            i.Property(x => x.Valor).HasMaxLength(100);
            i.Property(x => x.TipoIdentificador).HasMaxLength(50);
        });
        e.OwnsOne(d => d.Firmware);
    }
}
