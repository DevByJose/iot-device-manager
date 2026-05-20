using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Entities;

/// <summary>
/// Entidad hija: instrucción individual dentro de una Escena.
/// </summary>
public class AccionEscena : Entity
{
    public int Orden { get; private set; }
    public int DispositivoId { get; private set; }
    public string Comando { get; private set; }
    public ParametroComando? Parametro { get; private set; }

    private AccionEscena() { Comando = string.Empty; }

    public AccionEscena(int id, int orden, int dispositivoId, string comando, ParametroComando? parametro = null)
    {
        if (string.IsNullOrWhiteSpace(comando))
            throw new DomainException("El comando de la acción no puede estar vacío.");

        Id = id;
        Orden = orden;
        DispositivoId = dispositivoId;
        Comando = comando;
        Parametro = parametro;
    }
}
