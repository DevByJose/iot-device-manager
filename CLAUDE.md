# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build the full solution
dotnet build IOT.sln

# Run the REST API (desarrollo)
dotnet run --project src/IoT.API

# Run the Console App
dotnet run --project src/IoT.ConsoleApp

# Publish (release)
dotnet publish src/IoT.API -c Release
```

No hay proyecto de tests en el repositorio actualmente.

La base de datos SQLite (`hogarconectado.db`) se crea automáticamente al iniciar `IoT.API` via `db.Database.EnsureCreated()`. El archivo se genera en el directorio de trabajo del proceso.

Swagger UI disponible en `https://localhost:{port}/swagger` cuando `ASPNETCORE_ENVIRONMENT=Development`.

---

## Arquitectura

El proyecto sigue **Clean Architecture** estricta con **DDD** (Domain-Driven Design). La regla de dependencias es unidireccional:

```
IoT.API → IoT.Application → IoT.Domain
IoT.Infrastructure → IoT.Domain  (implementa interfaces del dominio)
IoT.API → IoT.Infrastructure     (registra las implementaciones en DI)
```

`IoT.Domain` no tiene referencias a ningún otro proyecto ni a frameworks externos.

### Proyectos

| Proyecto | Responsabilidad |
|---|---|
| `IoT.Domain` | Entidades, Value Objects, Eventos de dominio, Servicios de dominio, contratos (interfaces) |
| `IoT.Application` | Commands, Queries, Handlers (orquestadores), DTOs, Mappings, Validators |
| `IoT.Infrastructure` | EF Core + SQLite, Repositorios, EventBusPublisher, CacheService, UnitOfWork |
| `IoT.API` | Controllers ASP.NET Core, Middleware de errores, `Program.cs` (composición root) |
| `IoT.ConsoleApp` | Punto de entrada alternativo de consola |

---

## Agregados del Dominio

Hay **tres Aggregate Roots**, cada uno dueño de sus entidades hijas:

### `Hogar` (inventario de dispositivos)
- Hijos: `Habitacion`, `Dispositivo`
- Invariante clave: un `Dispositivo` solo puede registrarse en una `Habitacion` que pertenezca al mismo `Hogar`
- Eventos que eleva: `HogarRegistrado`, `DispositivoRegistrado`, `DispositivoDesinstalado`

### `Escena` (control y automatización)
- Hijos: `AccionEscena`, `Disparador`
- Invariante clave: para ejecutarse debe estar activa y tener al menos una acción
- Eventos que eleva: `EscenaCreada`, `EscenaEjecutada`

### `EstadoDispositivo` (monitoreo)
- Hijos: `LecturaSensor`, `AlertaEstado`
- Solo acepta lecturas con timestamp más reciente que la última consolidada (descarta duplicados)
- Eventos que eleva: `EstadoCambiado`, `AnomaliaDetectada`, `DispositivoDesconectado`

---

## Servicios de Dominio

Los servicios de dominio están en `IoT.Domain/Services/DomainServices.cs` y resuelven lógica que involucra más de un agregado:

- `SvcRegistroDispositivo` — verifica unicidad global del `IdentificadorFisico` antes de delegar al agregado `Hogar`
- `SvcEjecucionEscena` — traduce las `AccionEscena` de una escena en `ComandoDispositivo` secuenciales; si un dispositivo no está disponible, marca el comando como `Fallido` y continúa (política de tolerancia)
- `SvcValidacionComando` — valida que un comando sea permitido para el tipo de dispositivo (`Smartlight`, `Camera`, `Alarm`)
- `SvcConsolidacionEstado` — wrapper sobre `EstadoDispositivo.ActualizarLectura`
- `SvcDeteccionAnomalia` — wrapper sobre `EstadoDispositivo.DetectarAnomalia`

---

## Value Objects

Todos inmutables, en `IoT.Domain/ValueObjects/`:
`DireccionFisica`, `GeoLocalizacion`, `IdentificadorFisico`, `VersionFirmware`, `NombreEscena`, `ParametroComando`, `UmbralSensor`, `CondicionDisparador`, `IntervaloHorario`

EF Core los persiste con `OwnsOne` en el `DbContext`.

---

## Flujo de un comando de escritura

1. **Controller** recibe el request y despacha al **Handler** correspondiente (no tiene lógica propia)
2. **Handler** (`IoT.Application/Handlers/Handlers.cs`): valida el command, carga el agregado desde el repositorio, llama al servicio de dominio o directamente al agregado, llama `IUnitOfWork.SaveChangesAsync()`, publica los `DomainEvents` acumulados via `IEventPublisher`, limpia los eventos
3. **Repositorio** (`IoT.Infrastructure`) implementa las interfaces definidas en `IoT.Domain`
4. **EventBusPublisher** actualmente es un stub que imprime a consola; está diseñado para reemplazarse con RabbitMQ/Kafka sin modificar el dominio

---

## Manejo de errores

`ErrorHandlingMiddleware` intercepta toda la pipeline:
- `DomainException` → HTTP 400 con `{ error, type: "DomainException" }`
- Cualquier otra excepción → HTTP 500

Toda validación de reglas de negocio debe lanzar `DomainException` (nunca excepciones de infraestructura que lleguen al dominio).

---

## Caché

`ConsultarEstadoHandler` almacena el estado de dispositivos en memoria con TTL de 30 segundos. La clave es `estado:dispositivo:{dispositivoId}`. `ICacheService` / `CacheService` usan `IMemoryCache` de ASP.NET Core.

---

## Convenciones del proyecto

- El código y nombres de dominio están en **español** (entidades, métodos, propiedades, eventos)
- Cada archivo de una capa generalmente agrupa todas las clases relacionadas en un solo `.cs` (ej. `Commands.cs`, `Handlers.cs`, `Repositories.cs`)
- Los IDs de entidad se asignan como `0` al crear y EF Core los genera; el dominio no genera IDs
- `ComandoDispositivo.Estado` sigue la máquina de estados: `Pendiente → Enviado → Confirmado | Fallido`
