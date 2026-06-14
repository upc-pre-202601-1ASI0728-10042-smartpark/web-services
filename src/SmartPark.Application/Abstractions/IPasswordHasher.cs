namespace SmartPark.Application.Abstractions;

/// <summary>Puerto de hashing de contraseñas (implementado en Infraestructura).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
