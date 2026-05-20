namespace IoT.Domain.Services;

/// <summary>
/// Contrato para validar comandos por tipo de dispositivo (OCP).
/// Agregar un nuevo tipo = crear una nueva clase que implemente esta interfaz.
/// No se modifica ninguna clase existente.
/// </summary>
public interface IValidadorTipoDispositivo
{
    string Tipo { get; }
    bool EsComandoValido(string comando);
}
