namespace SmartPark.Application.Abstractions;

/// <summary>Confirma de forma atómica los cambios pendientes del repositorio.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
