using IoT.Domain.Entities;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Services;

/// <summary>
/// Evalúa si una lectura cruza un UmbralSensor y genera AlertaEstado (SRP).
/// </summary>
public class SvcDeteccionAnomalia
{
    public AlertaEstado? Evaluar(EstadoDispositivo estado, UmbralSensor umbral, int alertaId)
    {
        return estado.DetectarAnomalia(umbral, alertaId);
    }
}
