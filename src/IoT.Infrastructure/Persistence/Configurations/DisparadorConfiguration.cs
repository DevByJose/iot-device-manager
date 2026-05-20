using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoT.Infrastructure.Persistence.Configurations;

public class DisparadorConfiguration : IEntityTypeConfiguration<Disparador>
{
    public void Configure(EntityTypeBuilder<Disparador> e)
    {
        e.HasKey(d => d.Id);
        e.Property(d => d.Tipo).IsRequired().HasMaxLength(30);
        e.OwnsOne(d => d.Condicion);
        e.OwnsOne(d => d.Horario, h =>
        {
            h.Property(x => x.DiasSemana)
             .HasConversion(
                 v => string.Join(',', v.Select(d => (int)d)),
                 v => v == null || v.Length == 0
                     ? (IReadOnlyList<DayOfWeek>)Array.Empty<DayOfWeek>()
                     : (IReadOnlyList<DayOfWeek>)v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => (DayOfWeek)int.Parse(s))
                          .ToList()
                          .AsReadOnly()
             )
             .HasColumnType("TEXT")
             .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<DayOfWeek>>(
                 (a, b) => a != null && b != null && a.SequenceEqual(b),
                 v => v.Aggregate(0, (h, d) => HashCode.Combine(h, d.GetHashCode())),
                 v => (IReadOnlyList<DayOfWeek>)v.ToList().AsReadOnly()));
        });
    }
}
