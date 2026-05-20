using IoT.Domain.Entities;
using IoT.Domain.Interfaces;
using IoT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoT.Infrastructure.Repositories;

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
