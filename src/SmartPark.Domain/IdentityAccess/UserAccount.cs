using SmartPark.Domain.Common;
using SmartPark.Domain.IdentityAccess.Events;

namespace SmartPark.Domain.IdentityAccess;

/// <summary>
/// Agregado raíz del bounded context Identity &amp; Access. Encapsula la cuenta de un
/// operador o conductor. El hash de contraseña se calcula fuera del dominio (puerto
/// IPasswordHasher en la capa de aplicación) y se inyecta ya calculado.
/// </summary>
public sealed class UserAccount : AggregateRoot<Guid>
{
    public Email Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private UserAccount() { } // EF Core

    private UserAccount(Guid id, Email email, string passwordHash, string fullName, UserRole role) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Factory: registra una nueva cuenta y emite el evento de dominio.</summary>
    public static UserAccount Register(Email email, string passwordHash, string fullName, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("El hash de contraseña es obligatorio.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("El nombre completo es obligatorio.");

        var user = new UserAccount(Guid.NewGuid(), email, passwordHash, fullName, role);
        user.Raise(new UserRegistered(user.Id, email.Value, role));
        return user;
    }
}
