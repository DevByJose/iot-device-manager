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
            Console.WriteLine("  --- HOGARES ---");
            Console.WriteLine("  1.  Registrar nuevo Hogar");
            Console.WriteLine("  2.  Listar Hogares del Cliente");
            Console.WriteLine("  3.  Agregar Habitación a un Hogar");
            Console.WriteLine("  4.  Listar Habitaciones de un Hogar");
            Console.WriteLine("  --- DISPOSITIVOS ---");
            Console.WriteLine("  5.  Registrar Dispositivo");
            Console.WriteLine("  6.  Listar Dispositivos de un Hogar");
            Console.WriteLine("  7.  Conectar Dispositivo");
            Console.WriteLine("  8.  Desconectar Dispositivo");
            Console.WriteLine("  9.  Enviar Comando a Dispositivo");
            Console.WriteLine("  10. Consultar Estado de Dispositivo");
            Console.WriteLine("  --- ESCENAS ---");
            Console.WriteLine("  11. Crear Escena");
            Console.WriteLine("  12. Ejecutar Escena");
            Console.WriteLine("============================================");
            Console.WriteLine("  0.  Salir");
            Console.WriteLine("============================================");
            Console.Write("Seleccione una opción: ");

            var option = Console.ReadLine();

            try
            {
                switch (option)
                {
                    case "1":  await RegistrarHogar(serviceProvider); break;
                    case "2":  await ListarHogares(serviceProvider); break;
                    case "3":  await AgregarHabitacion(serviceProvider); break;
                    case "4":  await ListarHabitaciones(serviceProvider); break;
                    case "5":  await RegistrarDispositivo(serviceProvider); break;
                    case "6":  await ListarDispositivos(serviceProvider); break;
                    case "7":  await ConectarDispositivo(serviceProvider); break;
                    case "8":  await DesconectarDispositivo(serviceProvider); break;
                    case "9":  await EnviarComando(serviceProvider); break;
                    case "10": await ConsultarEstado(serviceProvider); break;
                    case "11": await CrearEscena(serviceProvider); break;
                    case "12": await EjecutarEscena(serviceProvider); break;
                    case "0":  exit = true; break;
                    default:   Console.WriteLine("Opción no válida."); break;
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

    // ─── DI ───────────────────────────────────────────────────────────────────

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "IoT.API", "hogarconectado.db");
        services.AddDbContext<HogarConectadoDbContext>(opt =>
            opt.UseSqlite($"Data Source={dbPath}"));

        // Repositorios
        services.AddScoped<IHogarRepository, HogarRepository>();
        services.AddScoped<IDispositivoRepository, DispositivoRepository>();
        services.AddScoped<IEscenaRepository, EscenaRepository>();
        services.AddScoped<IEstadoRepository, EstadoRepository>();
        services.AddScoped<IComandoRepository, ComandoRepository>();

        // Servicios transversales
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IEventPublisher, EventBusPublisher>();
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        // Servicios de dominio
        services.AddScoped<SvcRegistroDispositivo>();
        services.AddScoped<SvcEjecucionEscena>();
        services.AddScoped<SvcValidacionComando>();
        services.AddScoped<SvcConsolidacionEstado>();
        services.AddScoped<SvcDeteccionAnomalia>();

        // Handlers
        services.AddScoped<RegistrarHogarHandler>();
        services.AddScoped<ObtenerHogaresHandler>();
        services.AddScoped<AgregarHabitacionHandler>();
        services.AddScoped<ObtenerHabitacionesHandler>();
        services.AddScoped<RegistrarDispositivoHandler>();
        services.AddScoped<ObtenerDispositivosHandler>();
        services.AddScoped<ConectarDispositivoHandler>();
        services.AddScoped<DesconectarDispositivoHandler>();
        services.AddScoped<EnviarComandoHandler>();
        services.AddScoped<ConsultarEstadoHandler>();
        services.AddScoped<CrearEscenaHandler>();
        services.AddScoped<EjecutarEscenaHandler>();
        services.AddScoped<ObtenerTelemetriaHandler>();

        return services.BuildServiceProvider();
    }

    // ─── HOGARES ──────────────────────────────────────────────────────────────

    private static async Task RegistrarHogar(IServiceProvider provider)
    {
        Console.WriteLine("\n--- REGISTRAR HOGAR ---");
        Console.Write("Nombre del Hogar: ");
        string nombre = Console.ReadLine() ?? "";
        Console.Write("Ciudad: ");
        string ciudad = Console.ReadLine() ?? "";
        Console.Write("País: ");
        string pais = Console.ReadLine() ?? "";

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<RegistrarHogarHandler>();
        var result = await handler.HandleAsync(
            new RegistrarHogarCommand(nombre, "Sin calle", "S/N", ciudad, pais, "00000", 1));

        Console.WriteLine($"\n¡Éxito! Hogar '{result.Nombre}' registrado con ID: {result.Id}");
    }

    private static async Task ListarHogares(IServiceProvider provider)
    {
        Console.Write("\nID del Cliente (default 1): ");
        var input = Console.ReadLine();
        int clienteId = string.IsNullOrWhiteSpace(input) ? 1 : int.Parse(input);

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ObtenerHogaresHandler>();
        var hogares = await handler.HandleAsync(new ObtenerHogaresQuery(clienteId));

        Console.WriteLine($"\n--- HOGARES DEL CLIENTE {clienteId} ---");
        if (hogares.Count == 0) { Console.WriteLine("No se encontraron hogares."); return; }

        foreach (var h in hogares)
            Console.WriteLine($"  ID: {h.Id} | {h.Nombre} | {h.Ciudad}, {h.Pais} | Hab: {h.TotalHabitaciones} | Disp: {h.TotalDispositivos}");
    }

    private static async Task AgregarHabitacion(IServiceProvider provider)
    {
        Console.WriteLine("\n--- AGREGAR HABITACIÓN ---");
        Console.Write("ID del Hogar: ");
        if (!int.TryParse(Console.ReadLine(), out int hogarId)) return;
        Console.Write("Nombre de la Habitación (ej. Sala, Dormitorio): ");
        string nombre = Console.ReadLine() ?? "";

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<AgregarHabitacionHandler>();
        var result = await handler.HandleAsync(new AgregarHabitacionCommand(hogarId, nombre));

        Console.WriteLine($"\n¡Éxito! Habitación '{result.Nombre}' agregada con ID: {result.Id}");
    }

    private static async Task ListarHabitaciones(IServiceProvider provider)
    {
        Console.Write("\nID del Hogar: ");
        if (!int.TryParse(Console.ReadLine(), out int hogarId)) return;

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ObtenerHabitacionesHandler>();
        var habitaciones = await handler.HandleAsync(new ObtenerHabitacionesQuery(hogarId));

        Console.WriteLine($"\n--- HABITACIONES DEL HOGAR {hogarId} ---");
        if (habitaciones.Count == 0) { Console.WriteLine("No se encontraron habitaciones."); return; }

        foreach (var h in habitaciones)
            Console.WriteLine($"  ID: {h.Id} | {h.Nombre}");
    }

    // ─── DISPOSITIVOS ─────────────────────────────────────────────────────────

    private static async Task RegistrarDispositivo(IServiceProvider provider)
    {
        Console.WriteLine("\n--- REGISTRAR DISPOSITIVO ---");
        Console.Write("ID del Hogar: ");
        if (!int.TryParse(Console.ReadLine(), out int hogarId)) return;
        Console.Write("ID de la Habitación: ");
        if (!int.TryParse(Console.ReadLine(), out int habitacionId)) return;
        Console.Write("Nombre del Dispositivo (ej. Luz Sala): ");
        string nombre = Console.ReadLine() ?? "";
        Console.Write("Tipo (Smartlight / Camera / Alarm): ");
        string tipo = Console.ReadLine() ?? "";
        Console.Write("Identificador físico (MAC/Serial único): ");
        string mac = Console.ReadLine() ?? "";

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<RegistrarDispositivoHandler>();
        var result = await handler.HandleAsync(
            new RegistrarDispositivoCommand(hogarId, nombre, tipo, mac, "MAC", 1, 0, 0, habitacionId));

        Console.WriteLine($"\n¡Éxito! '{result.Nombre}' registrado con ID: {result.DispositivoId}");
    }

    private static async Task ListarDispositivos(IServiceProvider provider)
    {
        Console.Write("\nID del Hogar: ");
        if (!int.TryParse(Console.ReadLine(), out int hogarId)) return;

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ObtenerDispositivosHandler>();
        var dispositivos = await handler.HandleAsync(new ObtenerDispositivosQuery(hogarId));

        Console.WriteLine($"\n--- DISPOSITIVOS DEL HOGAR {hogarId} ---");
        if (dispositivos.Count == 0) { Console.WriteLine("No se encontraron dispositivos."); return; }

        foreach (var d in dispositivos)
            Console.WriteLine($"  ID: {d.Id} | {d.Nombre} ({d.TipoDispositivo}) | Estado: {d.Estado} | Hab: {d.Habitacion}");
    }

    private static async Task ConectarDispositivo(IServiceProvider provider)
    {
        Console.Write("\nID del Dispositivo a conectar: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ConectarDispositivoHandler>();
        await handler.HandleAsync(id);

        Console.WriteLine($"\n¡Éxito! Dispositivo {id} conectado (Online).");
    }

    private static async Task DesconectarDispositivo(IServiceProvider provider)
    {
        Console.Write("\nID del Dispositivo a desconectar: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<DesconectarDispositivoHandler>();
        await handler.HandleAsync(id);

        Console.WriteLine($"\n¡Éxito! Dispositivo {id} desconectado (Offline).");
    }

    private static async Task EnviarComando(IServiceProvider provider)
    {
        Console.WriteLine("\n--- ENVIAR COMANDO ---");
        Console.Write("ID del Dispositivo: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;
        Console.Write("Comando (TurnOn/TurnOff/SetColor/StartRecording/Trigger/Stop): ");
        string comando = Console.ReadLine() ?? "";

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<EnviarComandoHandler>();
        var result = await handler.HandleAsync(new EnviarComandoCommand(id, comando));

        Console.WriteLine($"\n¡Éxito! Comando '{result.Comando}' → Estado: {result.Estado} | ID Comando: {result.Id}");
    }

    private static async Task ConsultarEstado(IServiceProvider provider)
    {
        Console.Write("\nID del Dispositivo: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ConsultarEstadoHandler>();
        var estado = await handler.HandleAsync(new ConsultarEstadoQuery(id));

        if (estado == null) { Console.WriteLine("No se encontró estado para ese dispositivo."); return; }

        Console.WriteLine($"\n--- ESTADO DEL DISPOSITIVO {id} ---");
        Console.WriteLine($"  Estado:       {estado.Estado}");
        Console.WriteLine($"  Conectado:    {estado.Conectado}");
        Console.WriteLine($"  Último valor: {estado.UltimoValor?.ToString() ?? "N/A"}");
        Console.WriteLine($"  Actualizado:  {estado.UltimaActualizacion:G}");
        Console.WriteLine($"  Alertas:      {estado.TotalAlertas}");
    }

    // ─── ESCENAS ──────────────────────────────────────────────────────────────

    private static async Task CrearEscena(IServiceProvider provider)
    {
        Console.WriteLine("\n--- CREAR ESCENA ---");
        Console.Write("ID del Hogar: ");
        if (!int.TryParse(Console.ReadLine(), out int hogarId)) return;
        Console.Write("Nombre de la Escena (3-60 caracteres): ");
        string nombre = Console.ReadLine() ?? "";

        var acciones = new List<AccionEscenaInput>();
        Console.WriteLine("Agrega acciones (Enter en ID de dispositivo para terminar):");
        int orden = 1;
        while (true)
        {
            Console.Write($"  Acción {orden} - ID del Dispositivo (Enter para terminar): ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;
            if (!int.TryParse(input, out int dispId)) break;

            Console.Write($"  Acción {orden} - Comando: ");
            string cmd = Console.ReadLine() ?? "";

            acciones.Add(new AccionEscenaInput(orden, dispId, cmd));
            orden++;
        }

        if (acciones.Count == 0) { Console.WriteLine("La escena debe tener al menos una acción."); return; }

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CrearEscenaHandler>();
        var result = await handler.HandleAsync(new CrearEscenaCommand(hogarId, nombre, acciones));

        Console.WriteLine($"\n¡Éxito! Escena '{result.Nombre}' creada con ID: {result.Id} | Acciones: {result.TotalAcciones}");
    }

    private static async Task EjecutarEscena(IServiceProvider provider)
    {
        Console.Write("\nID de la Escena a ejecutar: ");
        if (!int.TryParse(Console.ReadLine(), out int escenaId)) return;

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<EjecutarEscenaHandler>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await uow.BeginTransactionAsync();
        try
        {
            var result = await handler.HandleAsync(new EjecutarEscenaCommand(escenaId, "manual"));
            await uow.CommitAsync();

            Console.WriteLine($"\n¡Éxito! Escena {result.EscenaId} ejecutada.");
            Console.WriteLine($"  Enviados: {result.ComandosEnviados} | Fallidos: {result.ComandosFallidos}");
            foreach (var detalle in result.Detalles)
                Console.WriteLine($"  {detalle}");
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}
