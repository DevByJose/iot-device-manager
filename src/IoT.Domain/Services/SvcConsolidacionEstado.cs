using IoT.Domain.Entities;

namespace IoT.Domain.Services;

/// <summary>
/// Recibe LecturaSensor y consolida EstadoDispositivo (SRP).
/// </summary>
public class SvcConsolidacionEstado
{
    public void Consolidar(EstadoDispositivo estado, LecturaSensor lectura)
    {
        estado.ActualizarLectura(lectura);
    }
}
