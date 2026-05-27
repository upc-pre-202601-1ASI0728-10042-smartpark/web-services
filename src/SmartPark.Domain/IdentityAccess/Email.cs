using System.Text.RegularExpressions;
using SmartPark.Domain.Common;

namespace SmartPark.Domain.IdentityAccess;

/// <summary>Objeto de valor Email: normaliza a minúsculas y valida el formato.</summary>
public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException("El correo no puede estar vacío.");
        var normalized = raw.Trim().ToLowerInvariant();
        if (!EmailRegex().IsMatch(normalized))
            throw new DomainException($"Correo inválido: '{raw}'.");
        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
