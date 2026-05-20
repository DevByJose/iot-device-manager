using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class ComandoDispositivoConfiguration : IEntityTypeConfiguration<ComandoDispositivo>
{
    public void Configure(EntityTypeBuilder<ComandoDispositivo> e)
    {
        e.HasKey(c => c.Id);
        e.Property(c => c.Id).ValueGeneratedOnAdd();
        e.Property(c => c.Comando).IsRequired().HasMaxLength(50);
        e.Property(c => c.Estado).IsRequired().HasMaxLength(20);
        e.Ignore(c => c.Parametros);
    }
}
