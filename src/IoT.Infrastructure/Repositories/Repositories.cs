using IoT.Domain.Entities;
using IoT.Domain.Interfaces;
using IoT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoT.Infrastructure.Repositories;

/// <summary>
/// Implementa IHogarRepository usando EF Core. Retorna entidades del dominio (LSP).
/// </summary>
public class HogarRepository : IHogarRepository
{
    private readonly HogarConectadoDbContext _db;

    public HogarRepository(HogarConectadoDbContext db) => _db = db;

    public async Task<Hogar?> GetByIdAsync(int id)
        => await _db.Hogares.Include(h => h.Habitaciones).Include(h => h.Dispositivos).FirstOrDefaultAsync(h => h.Id == id);

    public async Task<IReadOnlyList<Hogar>> GetAllAsync()
        => await _db.Hogares.Include(h => h.Habitaciones).Include(h => h.Dispositivos).ToListAsync();

    public async Task<IReadOnlyList<Hogar>> GetByClienteIdAsync(int clienteId)
        => await _db.Hogares.Where(h => h.ClienteId == clienteId).Include(h => h.Habitaciones).Include(h => h.Dispositivos).ToListAsync();

    public async Task SaveAsync(Hogar hogar)
    {
        var exists = await _db.Hogares.AnyAsync(h => h.Id == hogar.Id);
        if (exists) _db.Hogares.Update(hogar);
        else await _db.Hogares.AddAsync(hogar);
    }

    public async Task DeleteAsync(int id)
    {
        var hogar = await GetByIdAsync(id);
        if (hogar != null) _db.Hogares.Remove(hogar);
    }
}

/// <summary>
/// Implementa IDispositivoRepository (LSP).
/// </summary>
public class DispositivoRepository : IDispositivoRepository
{
    private readonly HogarConectadoDbContext _db;

    public DispositivoRepository(HogarConectadoDbContext db) => _db = db;

    public async Task<Dispositivo?> GetByIdAsync(int id)
        => await _db.Dispositivos.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IReadOnlyList<Dispositivo>> GetByHogarIdAsync(int hogarId)
        => await _db.Dispositivos.Where(d => d.HogarId == hogarId).ToListAsync();

    public async Task<bool> ExisteIdentificadorAsync(string identificadorFisico)
        => await _db.Dispositivos.AnyAsync(d => EF.Property<string>(d.Identificador, "Valor") == identificadorFisico);

    public async Task SaveAsync(Dispositivo dispositivo)
    {
        var exists = await _db.Dispositivos.AnyAsync(d => d.Id == dispositivo.Id);
        if (exists) _db.Dispositivos.Update(dispositivo);
        else await _db.Dispositivos.AddAsync(dispositivo);
    }

    public async Task DeleteAsync(int id)
    {
        var d = await GetByIdAsync(id);
        if (d != null) _db.Dispositivos.Remove(d);
    }
}

/// <summary>
/// Implementa IEscenaRepository (LSP).
/// </summary>
public class EscenaRepository : IEscenaRepository
{
    private readonly HogarConectadoDbContext _db;

    public EscenaRepository(HogarConectadoDbContext db) => _db = db;

    public async Task<Escena?> GetByIdAsync(int id)
        => await _db.Escenas.Include(e => e.Acciones).Include(e => e.Disparadores).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<Escena>> GetByHogarIdAsync(int hogarId)
        => await _db.Escenas.Where(e => e.HogarId == hogarId).Include(e => e.Acciones).ToListAsync();

    public async Task SaveAsync(Escena escena)
    {
        var exists = await _db.Escenas.AnyAsync(e => e.Id == escena.Id);
        if (exists) _db.Escenas.Update(escena);
        else await _db.Escenas.AddAsync(escena);
    }

    public async Task DeleteAsync(int id)
    {
        var e = await GetByIdAsync(id);
        if (e != null) _db.Escenas.Remove(e);
    }
}

/// <summary>
/// Implementa IComandoRepository (LSP).
/// </summary>
public class ComandoRepository : IComandoRepository
{
    private readonly HogarConectadoDbContext _db;

    public ComandoRepository(HogarConectadoDbContext db) => _db = db;

    public async Task SaveAllAsync(IEnumerable<ComandoDispositivo> comandos)
        => await _db.Comandos.AddRangeAsync(comandos);

    public async Task<IReadOnlyList<ComandoDispositivo>> GetByDispositivoIdAsync(int dispositivoId)
        => await _db.Comandos.Where(c => c.DispositivoId == dispositivoId).ToListAsync();
}

/// <summary>
/// Implementa IEstadoRepository (LSP).
/// </summary>
public class EstadoRepository : IEstadoRepository
{
    private readonly HogarConectadoDbContext _db;

    public EstadoRepository(HogarConectadoDbContext db) => _db = db;

    public async Task<EstadoDispositivo?> GetByDispositivoIdAsync(int dispositivoId)
        => await _db.EstadosDispositivo.Include(e => e.Lecturas).Include(e => e.Alertas)
            .FirstOrDefaultAsync(e => e.DispositivoId == dispositivoId);

    public async Task<IReadOnlyList<LecturaSensor>> GetLecturasAsync(int dispositivoId, DateTime desde, DateTime hasta)
        => await _db.LecturasSensor.Where(l => l.DispositivoId == dispositivoId && l.Timestamp >= desde && l.Timestamp <= hasta)
            .OrderBy(l => l.Timestamp).ToListAsync();

    public async Task SaveAsync(EstadoDispositivo estado)
    {
        var exists = await _db.EstadosDispositivo.AnyAsync(e => e.Id == estado.Id);
        if (exists) _db.EstadosDispositivo.Update(estado);
        else await _db.EstadosDispositivo.AddAsync(estado);
    }
}
