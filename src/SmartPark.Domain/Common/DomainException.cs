namespace SmartPark.Domain.Common;

/// <summary>Se lanza cuando se viola una invariante o regla de negocio del dominio.</summary>
public sealed class DomainException(string message) : Exception(message);
