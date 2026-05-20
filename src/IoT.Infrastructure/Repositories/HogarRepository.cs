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
