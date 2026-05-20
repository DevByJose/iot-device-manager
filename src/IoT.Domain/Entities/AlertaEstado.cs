using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Entities;

/// <summary>
/// Materialización de una condición que cruza un umbral relevante.
/// </summary>
public class AlertaEstado : Entity
{
    public int DispositivoId { get; private set; }
    public string Tipo { get; private set; }
    public string Descripcion { get; private set; }
    public double ValorLeido { get; private set; }
    public DateTime Generada { get; private set; }
    public bool Resuelta { get; private set; }

    private AlertaEstado() { Tipo = string.Empty; Descripcion = string.Empty; }

    public AlertaEstado(int id, int dispositivoId, string tipo, string descripcion, double valorLeido)
    {
        Id = id;
        DispositivoId = dispositivoId;
        Tipo = tipo;
        Descripcion = descripcion;
        ValorLeido = valorLeido;
        Generada = DateTime.UtcNow;
        Resuelta = false;
    }

    public void Resolver() => Resuelta = true;
}
