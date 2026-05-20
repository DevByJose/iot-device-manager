using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Versión semántica del firmware del dispositivo. Inmutable.
/// </summary>
public sealed class VersionFirmware : ValueObject
{
    public int Major { get; init; }
    public int Minor { get; init; }
    public int Patch { get; init; }

    private VersionFirmware() { }

    public VersionFirmware(int major, int minor, int patch)
    {
        if (major < 0 || minor < 0 || patch < 0)
            throw new DomainException("Los componentes de versión no pueden ser negativos.");

        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Major;
        yield return Minor;
        yield return Patch;
    }
}
