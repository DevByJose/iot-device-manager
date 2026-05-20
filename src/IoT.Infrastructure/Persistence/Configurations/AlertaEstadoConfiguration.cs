using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class AlertaEstadoConfiguration : IEntityTypeConfiguration<AlertaEstado>
{
    public void Configure(EntityTypeBuilder<AlertaEstado> e)
    {
        e.HasKey(a => a.Id);
        e.Property(a => a.Tipo).IsRequired().HasMaxLength(50);
    }
}
