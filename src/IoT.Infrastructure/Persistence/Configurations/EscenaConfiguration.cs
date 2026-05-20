using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class EscenaConfiguration : IEntityTypeConfiguration<Escena>
{
    public void Configure(EntityTypeBuilder<Escena> e)
    {
        e.HasKey(s => s.Id);
        e.OwnsOne(s => s.Nombre, n => n.Property(x => x.Valor).HasMaxLength(60).HasColumnName("Nombre"));
        e.HasMany(s => s.Acciones).WithOne().HasForeignKey("EscenaId");
        e.HasMany(s => s.Disparadores).WithOne().HasForeignKey("EscenaId");
        e.Navigation(s => s.Acciones).UsePropertyAccessMode(PropertyAccessMode.Field);
        e.Navigation(s => s.Disparadores).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
