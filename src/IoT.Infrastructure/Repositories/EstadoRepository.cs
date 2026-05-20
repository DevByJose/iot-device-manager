using IoT.Domain.Entities;
using IoT.Domain.Interfaces;
using IoT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoT.Infrastructure.Repositories;

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
