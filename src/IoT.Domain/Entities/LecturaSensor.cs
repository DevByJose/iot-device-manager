using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.Entities;

/// <summary>
/// Dato puntual reportado por un dispositivo en un momento dado.
/// </summary>
public class LecturaSensor : Entity
{
    public int DispositivoId { get; private set; }
    public double Valor { get; private set; }
    public string Unidad { get; private set; }
    public DateTime Timestamp { get; private set; }

    private LecturaSensor() { Unidad = string.Empty; }

    public LecturaSensor(int id, int dispositivoId, double valor, string unidad, DateTime timestamp)
    {
        Id = id;
        DispositivoId = dispositivoId;
        Valor = valor;
        Unidad = unidad ?? throw new DomainException("La unidad de lectura es obligatoria.");
        Timestamp = timestamp;
    }
}
