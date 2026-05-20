using IoT.Domain.BuildingBlocks;
using IoT.Domain.Events;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Entities;

/// <summary>
/// Orden concreta y auditable que se envía a un dispositivo físico.
/// </summary>
public class ComandoDispositivo : AggregateRoot
{
    public int DispositivoId { get; private set; }
    public string Comando { get; private set; }
    public List<ParametroComando> Parametros { get; private set; }
    public string Estado { get; private set; } // Pendiente, Enviado, Confirmado, Fallido
    public DateTime CreadoEn { get; private set; }
    public string? MotivoFallo { get; private set; }

    private ComandoDispositivo() { Comando = string.Empty; Estado = string.Empty; Parametros = new(); }

    public ComandoDispositivo(int id, int dispositivoId, string comando, List<ParametroComando>? parametros = null)
    {
        if (string.IsNullOrWhiteSpace(comando))
            throw new DomainException("El comando no puede estar vacío.");

        Id = id;
        DispositivoId = dispositivoId;
        Comando = comando;
        Parametros = parametros ?? new List<ParametroComando>();
        Estado = "Pendiente";
        CreadoEn = DateTime.UtcNow;
    }

    public void MarcarEnviado()
    {
        if (Estado != "Pendiente")
            throw new DomainException($"Solo se puede enviar un comando en estado Pendiente. Estado actual: {Estado}");
        Estado = "Enviado";
        AddDomainEvent(new ComandoEnviado(Id, DispositivoId, Comando));
    }

    public void Confirmar()
    {
        if (Estado != "Enviado")
            throw new DomainException($"Solo se puede confirmar un comando enviado. Estado actual: {Estado}");
        Estado = "Confirmado";
        AddDomainEvent(new ComandoConfirmado(Id));
    }

    public void MarcarFallido(string motivo)
    {
        Estado = "Fallido";
        MotivoFallo = motivo;
        AddDomainEvent(new ComandoFallido(Id, motivo));
    }
}
