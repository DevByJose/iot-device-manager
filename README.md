# IoT Device Manager

Sistema de gestión de dispositivos IoT para hogares inteligentes, construido con **.NET 8**, **Clean Architecture** y **Domain-Driven Design (DDD)**.

---

## Arquitectura

El proyecto sigue Clean Architecture con DDD y CQRS, organizado en cuatro capas con dependencias estrictamente hacia adentro:

```
IoT.API  ──►  IoT.Application  ──►  IoT.Domain
IoT.Infrastructure  ──────────────────────────►  IoT.Domain
```

### Capas

| Proyecto | Rol |
|---|---|
| `IoT.Domain` | Núcleo del dominio: entidades, value objects, eventos, servicios de dominio, interfaces |
| `IoT.Application` | Orquestación: comandos, queries, handlers (CQRS), DTOs, validadores |
| `IoT.Infrastructure` | Persistencia (EF Core + SQLite), repositorios, caché en memoria, event bus (stub) |
| `IoT.API` | API REST (ASP.NET Core), controladores, middleware de errores, Swagger |
| `IoT.ConsoleApp` | Punto de entrada alternativo para pruebas sin HTTP |

---

## Dominio

### Aggregate Roots

| Aggregate | Descripción |
|---|---|
| `Hogar` | Gestiona el inventario del hogar: habitaciones y dispositivos registrados |
| `Escena` | Automatización: acciones y disparadores (sensor, horario, geolocalización) |
| `EstadoDispositivo` | Monitoreo de telemetría: lecturas de sensor y alertas de anomalía |

### Value Objects

`DireccionFisica` · `GeoLocalizacion` · `IdentificadorFisico` · `VersionFirmware` · `NombreEscena` · `ParametroComando` · `UmbralSensor` · `CondicionDisparador` · `IntervaloHorario`

### Eventos de Dominio

`HogarRegistrado` · `DispositivoRegistrado` · `DispositivoDesinstalado` · `EscenaCreada` · `EscenaEjecutada` · `EstadoCambiado` · `AnomaliaDetectada` · `DispositivoDesconectado`

### Servicios de Dominio

| Servicio | Responsabilidad |
|---|---|
| `SvcRegistroDispositivo` | Garantiza unicidad global de `IdentificadorFisico` |
| `SvcEjecucionEscena` | Traduce acciones de escena en `ComandoDispositivo` secuenciales |
| `SvcValidacionComando` | Valida comandos por tipo de dispositivo (Smartlight, Camera, Alarm) |
| `SvcConsolidacionEstado` | Actualiza lecturas en el aggregate `EstadoDispositivo` |
| `SvcDeteccionAnomalia` | Detecta anomalías contra umbrales configurados |

---

## API REST

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/hogar` | Registrar un nuevo hogar |
| `GET` | `/api/hogar/cliente/{clienteId}` | Obtener hogares de un cliente |
| `POST` | `/api/hogar/{hogarId}/habitacion` | Agregar habitación a un hogar |
| `GET` | `/api/hogar/{hogarId}/habitacion` | Listar habitaciones de un hogar |
| `POST` | `/api/dispositivo` | Registrar dispositivo |
| `GET` | `/api/dispositivo/hogar/{hogarId}` | Listar dispositivos de un hogar |
| `POST` | `/api/escena/{escenaId}/ejecutar` | Ejecutar una escena |
| `GET` | `/api/estado/{dispositivoId}` | Consultar estado de dispositivo (caché 30 s) |

La documentación interactiva está disponible en `/swagger` al ejecutar en modo desarrollo.

---

## Stack tecnológico

- **Lenguaje / Runtime:** C# 12 · .NET 8
- **ORM / BD:** Entity Framework Core 8.0.15 · SQLite
- **API:** ASP.NET Core Web API · Swashbuckle (Swagger) 6.9.0
- **Caché:** `Microsoft.Extensions.Caching.Memory` 8.0.1
- **Mensajería:** Stub en consola (diseñado para reemplazar con RabbitMQ/Kafka)

---

## Estructura de directorios

```
iot-device-manager/
├── src/
│   ├── IoT.Domain/
│   │   ├── BuildingBlocks/     # AggregateRoot, Entity, ValueObject, IDomainEvent
│   │   ├── Entities/           # Hogar, Escena, EstadoDispositivo y entidades hijas
│   │   ├── ValueObjects/       # 9 value objects inmutables
│   │   ├── Events/             # 9 eventos de dominio
│   │   ├── Services/           # 5 servicios de dominio
│   │   ├── Interfaces/         # Contratos de repositorios, UoW, caché, event bus
│   │   └── Exceptions/         # DomainException
│   ├── IoT.Application/
│   │   ├── Commands/           # Command records (escritura)
│   │   ├── Queries/            # Query records (lectura)
│   │   ├── Handlers/           # Handlers CQRS
│   │   ├── DTOs/               # Objetos de respuesta
│   │   ├── Mappings/           # Mapper dominio → DTO
│   │   └── Validators/         # Validación de comandos
│   ├── IoT.Infrastructure/
│   │   ├── Persistence/        # DbContext con EF Core
│   │   ├── Repositories/       # Implementaciones de repositorios
│   │   ├── Services/           # UnitOfWork, CacheService
│   │   └── Messaging/          # EventBusPublisher (stub)
│   ├── IoT.API/
│   │   ├── Controllers/        # HogarController, DispositivoController, EscenaController
│   │   ├── Middleware/         # ErrorHandlingMiddleware
│   │   └── Program.cs          # Composition root / DI
│   └── IoT.ConsoleApp/
│       └── Program.cs
└── IOT.sln
```

---

## Cómo ejecutar

**Requisitos:** .NET 8 SDK

```bash
# Compilar toda la solución
dotnet build IOT.sln

# Ejecutar la API (SQLite se crea automáticamente)
dotnet run --project src/IoT.API

# Ejecutar la consola de pruebas
dotnet run --project src/IoT.ConsoleApp

# Publicar en Release
dotnet publish src/IoT.API -c Release
```

---

## Estado actual

- **Tests:** No hay proyecto de pruebas en el repositorio.
- **Migraciones:** La base de datos se crea con `EnsureCreated()` al iniciar; no hay migraciones de EF.
- **Mensajería:** El event bus es un stub que imprime en consola; pendiente integrar RabbitMQ o Kafka.
- **Casos de uso pendientes:** `CrearEscena`, `EnviarComando` y `ObtenerTelemetria` no tienen handler ni endpoint implementados.
