using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class LecturaSensorConfiguration : IEntityTypeConfiguration<LecturaSensor>
{
    public void Configure(EntityTypeBuilder<LecturaSensor> e)
    {
        e.HasKey(l => l.Id);
        e.Property(l => l.Unidad).IsRequired().HasMaxLength(20);
    }
}
