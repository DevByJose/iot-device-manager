using IoT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IoT.Infrastructure.Persistence;

/// <summary>
/// DbContext — acceso a la base de datos. Solo en infraestructura (DIP).
/// El dominio NO depende de EF Core.
/// </summary>
public class HogarConectadoDbContext : DbContext
{
    public DbSet<Hogar> Hogares => Set<Hogar>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<Escena> Escenas => Set<Escena>();
    public DbSet<AccionEscena> AccionesEscena => Set<AccionEscena>();
    public DbSet<Disparador> Disparadores => Set<Disparador>();
    public DbSet<EstadoDispositivo> EstadosDispositivo => Set<EstadoDispositivo>();
    public DbSet<LecturaSensor> LecturasSensor => Set<LecturaSensor>();
    public DbSet<AlertaEstado> AlertasEstado => Set<AlertaEstado>();
    public DbSet<ComandoDispositivo> Comandos => Set<ComandoDispositivo>();

    public HogarConectadoDbContext(DbContextOptions<HogarConectadoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Hogar (Aggregate Root)
        modelBuilder.Entity<Hogar>(e =>
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
        });

        modelBuilder.Entity<Habitacion>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Nombre).IsRequired().HasMaxLength(50);
        });

        // Dispositivo
        modelBuilder.Entity<Dispositivo>(e =>
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
        });

        // Escena (Aggregate Root)
        modelBuilder.Entity<Escena>(e =>
        {
            e.HasKey(s => s.Id);
            e.OwnsOne(s => s.Nombre, n => n.Property(x => x.Valor).HasMaxLength(60).HasColumnName("Nombre"));
            e.HasMany(s => s.Acciones).WithOne().HasForeignKey("EscenaId");
            e.HasMany(s => s.Disparadores).WithOne().HasForeignKey("EscenaId");
            e.Navigation(s => s.Acciones).UsePropertyAccessMode(PropertyAccessMode.Field);
            e.Navigation(s => s.Disparadores).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<AccionEscena>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Comando).IsRequired().HasMaxLength(50);
            e.OwnsOne(a => a.Parametro);
        });

        modelBuilder.Entity<Disparador>(e =>
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
                 .HasColumnType("TEXT");
            });
        });

        // EstadoDispositivo (Aggregate Root)
        modelBuilder.Entity<EstadoDispositivo>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.EstadoActual).IsRequired().HasMaxLength(50);
            e.HasMany(s => s.Lecturas).WithOne().HasForeignKey(l => l.DispositivoId);
            e.HasMany(s => s.Alertas).WithOne().HasForeignKey(a => a.DispositivoId);
            e.Navigation(s => s.Lecturas).UsePropertyAccessMode(PropertyAccessMode.Field);
            e.Navigation(s => s.Alertas).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<LecturaSensor>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Unidad).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<AlertaEstado>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Tipo).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<ComandoDispositivo>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Comando).IsRequired().HasMaxLength(50);
            e.Property(c => c.Estado).IsRequired().HasMaxLength(20);
            e.Ignore(c => c.Parametros);
        });
    }
}
