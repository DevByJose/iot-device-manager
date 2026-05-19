using IoT.API.Middleware;
using IoT.Application.Handlers;
using IoT.Domain.Interfaces;
using IoT.Domain.Services;
using IoT.Infrastructure.Messaging;
using IoT.Infrastructure.Persistence;
using IoT.Infrastructure.Repositories;
using IoT.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === Capa de Infraestructura: DbContext con SQLite ===
builder.Services.AddDbContext<HogarConectadoDbContext>(options =>
    options.UseSqlite("Data Source=hogarconectado.db"));

// === Capa de Infraestructura: Repositorios implementan interfaces de dominio (DIP) ===
builder.Services.AddScoped<IHogarRepository, HogarRepository>();
builder.Services.AddScoped<IDispositivoRepository, DispositivoRepository>();
builder.Services.AddScoped<IEscenaRepository, EscenaRepository>();
builder.Services.AddScoped<IEstadoRepository, EstadoRepository>();

// === Capa de Infraestructura: UnitOfWork, EventPublisher, Cache ===
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IEventPublisher, EventBusPublisher>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

// === Capa de Dominio: Servicios de dominio ===
builder.Services.AddScoped<SvcRegistroDispositivo>();
builder.Services.AddScoped<SvcEjecucionEscena>();
builder.Services.AddScoped<SvcValidacionComando>();
builder.Services.AddScoped<SvcConsolidacionEstado>();
builder.Services.AddScoped<SvcDeteccionAnomalia>();

// === Capa de Aplicación: Handlers (orquestadores) ===
builder.Services.AddScoped<RegistrarHogarHandler>();
builder.Services.AddScoped<RegistrarDispositivoHandler>();
builder.Services.AddScoped<EjecutarEscenaHandler>();
builder.Services.AddScoped<ConsultarEstadoHandler>();
builder.Services.AddScoped<ObtenerDispositivosHandler>();
builder.Services.AddScoped<ObtenerHogaresHandler>();

// === Capa de API: Controllers ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Crear la base de datos automáticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HogarConectadoDbContext>();
    db.Database.EnsureCreated();
}

// Middleware de manejo de errores (capa más externa)
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
