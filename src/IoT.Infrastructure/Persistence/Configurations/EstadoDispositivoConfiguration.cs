using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class EstadoDispositivoConfiguration : IEntityTypeConfiguration<EstadoDispositivo>
{
    public void Configure(EntityTypeBuilder<EstadoDispositivo> e)
    {
        e.HasKey(s => s.Id);
        e.Property(s => s.EstadoActual).IsRequired().HasMaxLength(50);
        e.HasMany(s => s.Lecturas).WithOne().HasForeignKey(l => l.DispositivoId);
        e.HasMany(s => s.Alertas).WithOne().HasForeignKey(a => a.DispositivoId);
        e.Navigation(s => s.Lecturas).UsePropertyAccessMode(PropertyAccessMode.Field);
        e.Navigation(s => s.Alertas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
