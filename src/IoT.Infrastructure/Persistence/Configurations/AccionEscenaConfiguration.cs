using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class AccionEscenaConfiguration : IEntityTypeConfiguration<AccionEscena>
{
    public void Configure(EntityTypeBuilder<AccionEscena> e)
    {
        e.HasKey(a => a.Id);
        e.Property(a => a.Comando).IsRequired().HasMaxLength(50);
        e.OwnsOne(a => a.Parametro);
    }
}
