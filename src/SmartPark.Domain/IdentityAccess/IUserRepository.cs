namespace SmartPark.Domain.IdentityAccess;

/// <summary>Repositorio del agregado UserAccount (puerto del dominio; se implementa en Infraestructura).</summary>
public interface IUserRepository
{
    Task<UserAccount?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default);
    void Add(UserAccount user);
}
