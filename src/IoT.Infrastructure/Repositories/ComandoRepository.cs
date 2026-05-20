using IoT.Domain.Entities;
using IoT.Domain.Interfaces;
using IoT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoT.Infrastructure.Repositories;

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
