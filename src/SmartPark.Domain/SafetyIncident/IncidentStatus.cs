namespace SmartPark.Domain.SafetyIncident;

/// <summary>Estado del ciclo de vida de un incidente de seguridad.</summary>
public enum IncidentStatus
{
    Alert,      // detectado, sin atender
    Confirmed,  // el operador tomó conocimiento
    Resolved    // incidente cerrado
}
