using IoT.Domain.Entities;
using IoT.Domain.Interfaces;
using IoT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoT.Infrastructure.Repositories;

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
        => await _db.Dispositivos.AnyAsync(d => d.Identificador.Valor == identificadorFisico);

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
