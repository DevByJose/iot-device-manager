using IoT.Application.Interfaces;
using IoT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace IoT.Infrastructure.Services;

/// <summary>
/// Implementa IUnitOfWork para manejar transacciones (SRP + DIP).
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly HogarConectadoDbContext _db;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(HogarConectadoDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _db.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync()
        => _transaction = await _db.Database.BeginTransactionAsync();

    public async Task CommitAsync()
    {
        await _db.SaveChangesAsync();
        if (_transaction != null) await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null) await _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _db.Dispose();
    }
}
