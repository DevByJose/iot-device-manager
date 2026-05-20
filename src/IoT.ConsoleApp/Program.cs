using IoT.Application.Commands;
using IoT.Application.Handlers;
using IoT.Application.Interfaces;
using IoT.Application.Queries;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;
using IoT.Domain.Services;
using IoT.Infrastructure.Messaging;
using IoT.Infrastructure.Persistence;
using IoT.Infrastructure.Repositories;
using IoT.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IoT.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("============================================");
        Console.WriteLine("    PLATAFORMA IoT - CONSOLA DE PRUEBAS     ");
        Console.WriteLine("============================================");
        Console.WriteLine("Inicializando sistema...");

        var serviceProvider = ConfigureServices();

        // Asegurar que la base de datos existe
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HogarConectadoDbContext>();
            db.Database.EnsureCreated();
        }

        Console.WriteLine("Sistema listo. Presione Enter para comenzar...");
        Console.ReadLine();

        bool exit = false;
        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("============================================");
            Console.WriteLine("               MENU PRINCIPAL               ");
            Console.WriteLine("============================================");
            Console.WriteLine("1. Registrar nuevo Hogar");
            Console.WriteLine("2. Listar Hogares del Cliente 1");
            Console.WriteLine("3. Registrar Dispositivo en Hogar");
            Console.WriteLine("4. Listar Dispositivos de un Hogar");
            Console.WriteLine("5. Salir");
            Console.WriteLine("============================================");
            Console.Write("Seleccione una opción: ");

            var option = Console.ReadLine();

            try
            {
                switch (option)
                {
                    case "1":
                        await RegistrarHogar(serviceProvider);
                        break;
                    case "2":
                        await ListarHogares(serviceProvider);
                        break;
                    case "3":
                        await RegistrarDispositivo(serviceProvider);
                        break;
                    case "4":
                        await ListarDispositivos(serviceProvider);
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
            catch (DomainException ex)
            {
                Console.WriteLine($"\n[ERROR DE NEGOCIO] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR INESPERADO] {ex.Message}");
            }

            if (!exit)
            {
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Infraestructura: Base de datos SQLite
        // Usamos una ruta absoluta o relativa al directorio de ejecución para compartir la misma BD
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "IoT.API", "hogarconectado.db");
        services.AddDbContext<HogarConectadoDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Infraestructura: Repositorios
        services.AddScoped<IHogarRepository, HogarRepository>();
        services.AddScoped<IDispositivoRepository, DispositivoRepository>();
        services.AddScoped<IEscenaRepository, EscenaRepository>();
        services.AddScoped<IEstadoRepository, EstadoRepository>();

        // Infraestructura: Servicios transversales
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IEventPublisher, EventBusPublisher>();
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        // Dominio: Servicios
        services.AddScoped<SvcRegistroDispositivo>();
        services.AddScoped<SvcEjecucionEscena>();
        services.AddScoped<SvcValidacionComando>();
        services.AddScoped<SvcConsolidacionEstado>();
        services.AddScoped<SvcDeteccionAnomalia>();

        // Aplicación: Handlers
        services.AddScoped<RegistrarHogarHandler>();
        services.AddScoped<RegistrarDispositivoHandler>();
        services.AddScoped<EjecutarEscenaHandler>();
        services.AddScoped<ConsultarEstadoHandler>();
        services.AddScoped<ObtenerDispositivosHandler>();
        services.AddScoped<ObtenerHogaresHandler>();

        return services.BuildServiceProvider();
    }

    private static async Task RegistrarHogar(IServiceProvider provider)
    {
        Console.WriteLine("\n--- REGISTRAR HOGAR ---");
        Console.Write("Nombre del Hogar: ");
        string nombre = Console.ReadLine() ?? "";
        Console.Write("Ciudad: ");
        string ciudad = Console.ReadLine() ?? "";
        Console.Write("País: ");
        string pais = Console.ReadLine() ?? "";

        var handler = provider.GetRequiredService<RegistrarHogarHandler>();
        var command = new RegistrarHogarCommand(nombre, "Calle Falsa", "123", ciudad, pais, "00000", 1);
        
        var result = await handler.HandleAsync(command);
        Console.WriteLine($"\n¡Éxito! Hogar '{result.Nombre}' registrado con ID: {result.Id}");
    }

    private static async Task ListarHogares(IServiceProvider provider)
    {
        Console.WriteLine("\n--- HOGARES DEL CLIENTE 1 ---");
        var handler = provider.GetRequiredService<ObtenerHogaresHandler>();
        var hogares = await handler.HandleAsync(new ObtenerHogaresQuery(1));

        if (hogares.Count == 0)
        {
            Console.WriteLine("No se encontraron hogares.");
            return;
        }

        foreach (var h in hogares)
        {
            Console.WriteLine($"- ID: {h.Id} | Nombre: {h.Nombre} | Ubicación: {h.Ciudad}, {h.Pais}");
        }
    }

    private static async Task RegistrarDispositivo(IServiceProvider provider)
    {
        Console.WriteLine("\n--- REGISTRAR DISPOSITIVO ---");
        Console.Write("ID del Hogar: ");
        if (!int.TryParse(Console.ReadLine(), out int hogarId)) return;

        Console.Write("Nombre del Dispositivo (ej. Luz Sala): ");
        string nombre = Console.ReadLine() ?? "";
        Console.Write("Tipo (Smartlight/Camera/Alarm): ");
        string tipo = Console.ReadLine() ?? "";
        Console.Write("MAC Address o Serial: ");
        string mac = Console.ReadLine() ?? "";

        var handler = provider.GetRequiredService<RegistrarDispositivoHandler>();
        // Usamos HabitacionId = 1 por defecto para el demo (asumiendo que existe o no lo valida estrictamente el repo si no está en memoria)
        // Nota: Para que no falle, el hogar debería tener al menos una habitación.
        // Simularemos que el handler lo permite, o podemos atrapar el error.
        
        // Vamos a pedir el ID de habitación
        Console.Write("ID de la Habitación (debe existir en el hogar): ");
        if (!int.TryParse(Console.ReadLine(), out int habitacionId)) return;

        var command = new RegistrarDispositivoCommand(hogarId, nombre, tipo, mac, "MAC", 1, 0, 0, habitacionId);
        var result = await handler.HandleAsync(command);

        Console.WriteLine($"\n¡Éxito! Dispositivo registrado con ID: {result.DispositivoId}");
    }

    private static async Task ListarDispositivos(IServiceProvider provider)
    {
        Console.WriteLine("\n--- DISPOSITIVOS DEL HOGAR ---");
        Console.Write("ID del Hogar: ");
        if (!int.TryParse(Console.ReadLine(), out int hogarId)) return;

        var handler = provider.GetRequiredService<ObtenerDispositivosHandler>();
        var dispositivos = await handler.HandleAsync(new ObtenerDispositivosQuery(hogarId));

        if (dispositivos.Count == 0)
        {
            Console.WriteLine("No se encontraron dispositivos.");
            return;
        }

        foreach (var d in dispositivos)
        {
            Console.WriteLine($"- ID: {d.Id} | {d.Nombre} ({d.TipoDispositivo}) | Estado: {d.Estado} | MAC: {d.Identificador}");
        }
    }
}
