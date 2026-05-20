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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HogarConectadoDbContext).Assembly);
    }
}
