# IoT Device Manager

Sistema de gestión de dispositivos IoT para hogares inteligentes (cámaras, alarmas, luces inteligentes), construido con **.NET 8**, **Clean Architecture** y **Domain-Driven Design (DDD)**.

---

## Arquitectura

El proyecto sigue Clean Architecture estricta con DDD. Las dependencias apuntan **siempre hacia adentro**:

```
IoT.API  ──►  IoT.Application  ──►  IoT.Domain
              IoT.Infrastructure  ──►  IoT.Domain + IoT.Application
```

| Proyecto | Rol |
|---|---|
| `IoT.Domain` | Núcleo: entidades, value objects, eventos de dominio, servicios de dominio, interfaces |
| `IoT.Application` | Orquestación: commands, queries, handlers (CQRS), DTOs, validadores, interfaces de servicios |
| `IoT.Infrastructure` | Persistencia (EF Core + SQLite), repositorios, caché en memoria, event bus (stub) |
| `IoT.API` | API REST (ASP.NET Core), controllers, middleware de errores, Swagger |
| `IoT.ConsoleApp` | Punto de entrada alternativo con menú CLI completo |
| `IoT.Domain.Tests` | Tests unitarios del dominio (xUnit) |

`IoT.Domain` no tiene ningún `PackageReference` externo — cero dependencias de frameworks.

---

## Principios de diseño

### SOLID

| Principio | Implementación |
|---|---|
| **S** Single Responsibility | Un handler por caso de uso, un controller por área de dominio, un servicio de dominio por responsabilidad |
| **O** Open/Closed | `IValidadorTipoDispositivo` + estrategias por tipo (`ValidadorSmartlight`, `ValidadorCamera`, `ValidadorAlarm`) — agregar un tipo nuevo = solo una clase nueva |
| **L** Liskov Substitution | Todas las implementaciones respetan sus contratos sin restricciones |
| **I** Interface Segregation | `ISaveChanges` (solo persistencia) y `IUnitOfWork : ISaveChanges` (transacciones) — los handlers usan la interfaz mínima |
| **D** Dependency Inversion | Domain define contratos, Infrastructure los implementa; la regla de dependencias nunca se invierte |

---

## Dominio

### Aggregate Roots

| Aggregate | Responsabilidad | Hijos |
|---|---|---|
| `Hogar` | Inventario del hogar | `Habitacion`, `Dispositivo` |
| `Escena` | Automatización y control | `AccionEscena`, `Disparador` |
| `EstadoDispositivo` | Monitoreo de telemetría | `LecturaSensor`, `AlertaEstado` |
| `ComandoDispositivo` | Ciclo de vida de un comando | — |

### Value Objects (todos inmutables, validan en constructor)

`DireccionFisica` · `GeoLocalizacion` · `IdentificadorFisico` · `VersionFirmware` · `NombreEscena` · `ParametroComando` · `UmbralSensor` · `CondicionDisparador` · `IntervaloHorario`

### Eventos de Dominio (nombrados en pasado, elevados desde agregados)

`HogarRegistrado` · `DispositivoRegistrado` · `DispositivoDesinstalado` · `EscenaCreada` · `EscenaEjecutada` · `EstadoCambiado` · `AnomaliaDetectada` · `DispositivoDesconectado` · `ComandoEnviado` · `ComandoConfirmado` · `ComandoFallido`

### Servicios de Dominio

| Servicio | Responsabilidad |
|---|---|
| `SvcRegistroDispositivo` | Garantiza unicidad global de `IdentificadorFisico` antes de delegar al AR `Hogar` |
| `SvcEjecucionEscena` | Traduce `AccionEscena` en `ComandoDispositivo` secuenciales con política de tolerancia a fallos |
| `SvcValidacionComando` | Delega la validación al `IValidadorTipoDispositivo` correspondiente (OCP) |
| `SvcConsolidacionEstado` | Wrapper sobre `EstadoDispositivo.ActualizarLectura` |
| `SvcDeteccionAnomalia` | Wrapper sobre `EstadoDispositivo.DetectarAnomalia` |

---

## API REST

### Hogares

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/hogar` | Registrar nuevo hogar |
| `GET` | `/api/hogar/cliente/{clienteId}` | Listar hogares de un cliente |
| `POST` | `/api/hogar/{hogarId}/habitacion` | Agregar habitación |
| `GET` | `/api/hogar/{hogarId}/habitacion` | Listar habitaciones |

### Dispositivos

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/dispositivo` | Registrar dispositivo (Smartlight / Camera / Alarm) |
| `GET` | `/api/dispositivo/hogar/{hogarId}` | Listar dispositivos de un hogar |
| `PUT` | `/api/dispositivo/{id}/conectar` | Marcar dispositivo como Online |
| `PUT` | `/api/dispositivo/{id}/desconectar` | Marcar dispositivo como Offline |
| `POST` | `/api/dispositivo/{id}/comando` | Enviar comando al dispositivo |

### Escenas

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/escena` | Crear escena con acciones |
| `POST` | `/api/escena/{escenaId}/ejecutar` | Ejecutar escena (transacción explícita) |

### Estado y Telemetría

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/estado/{dispositivoId}` | Consultar estado actual (caché 30 s) |
| `GET` | `/api/estado/{dispositivoId}/telemetria?desde=&hasta=` | Historial de lecturas de sensor |

Documentación interactiva disponible en `/swagger` en modo Development.

#### Comandos válidos por tipo

| Tipo | Comandos |
|---|---|
| `Smartlight` | `TurnOn`, `TurnOff`, `SetColor`, `SetSchedule` |
| `Camera` | `StartRecording`, `CaptureSnapshot`, `TurnOn`, `TurnOff` |
| `Alarm` | `Trigger`, `Stop`, `TurnOn`, `TurnOff` |

---

## Consola CLI

`IoT.ConsoleApp` ofrece un menú interactivo con las mismas 13 funciones que la API:

```
1.  Registrar Hogar          7.  Conectar Dispositivo
2.  Listar Hogares           8.  Desconectar Dispositivo
3.  Agregar Habitación       9.  Enviar Comando
4.  Listar Habitaciones      10. Consultar Estado
5.  Registrar Dispositivo    11. Crear Escena
6.  Listar Dispositivos      12. Ejecutar Escena
```

---

## Tests

El proyecto incluye **69 tests unitarios** de dominio (`IoT.Domain.Tests`) que cubren:

| Archivo | Qué valida |
|---|---|
| `ValueObjectTests.cs` | Inmutabilidad y validaciones de `NombreEscena`, `DireccionFisica`, `VersionFirmware`, `IdentificadorFisico` |
| `HogarTests.cs` | Invariantes del AR: habitaciones duplicadas, habitación ajena, eventos elevados |
| `EscenaYComandoTests.cs` | Escena inactiva/sin acciones, orden de acciones, máquina de estados de `ComandoDispositivo` |
| `EstadoDispositivoTests.cs` | Lecturas antiguas descartadas, `EstadoCambiado` solo cuando hay cambio real, detección de anomalías |
| `SvcValidacionComandoTests.cs` | Comandos válidos/inválidos por tipo, dispositivo desconectado |

```bash
dotnet test tests/IoT.Domain.Tests
```

---

## Cómo ejecutar

**Requisito:** .NET 8 SDK

```bash
# Compilar toda la solución
dotnet build IOT.sln

# API REST (la base de datos SQLite se crea automáticamente)
dotnet run --project src/IoT.API
# → Swagger: https://localhost:{puerto}/swagger

# Consola interactiva
dotnet run --project src/IoT.ConsoleApp

# Tests
dotnet test tests/IoT.Domain.Tests

# Publicar en Release
dotnet publish src/IoT.API -c Release
```

---

## Estructura del proyecto

```
iot-device-manager/
├── src/
│   ├── IoT.Domain/
│   │   ├── BuildingBlocks/     # AggregateRoot, Entity, ValueObject, IDomainEvent
│   │   ├── Entities/           # 8 entidades de dominio
│   │   ├── ValueObjects/       # 9 value objects inmutables
│   │   ├── Events/             # 11 eventos de dominio
│   │   ├── Services/           # 5 servicios de dominio + IValidadorTipoDispositivo
│   │   ├── Interfaces/         # IHogarRepository, IDispositivoRepository, IEscenaRepository,
│   │   │                       # IEstadoRepository, IComandoRepository, IEventPublisher
│   │   └── Exceptions/         # DomainException
│   ├── IoT.Application/
│   │   ├── Commands/           # 6 command records
│   │   ├── Queries/            # 5 query records
│   │   ├── Handlers/           # 13 handlers (un use case cada uno)
│   │   ├── DTOs/               # 9 sealed records de respuesta
│   │   ├── Mappings/           # DomainToDtoMapper (mapeo explícito)
│   │   ├── Validators/         # CommandValidators (validación en boundaries)
│   │   └── Interfaces/         # ISaveChanges, IUnitOfWork, ICacheService
│   ├── IoT.Infrastructure/
│   │   ├── Persistence/        # HogarConectadoDbContext (EF Core + SQLite)
│   │   ├── Repositories/       # 5 repositorios
│   │   ├── Services/           # UnitOfWork, CacheService
│   │   └── Messaging/          # EventBusPublisher (stub → reemplazable por RabbitMQ/Kafka)
│   ├── IoT.API/
│   │   ├── Controllers/        # HogarController, DispositivoController,
│   │   │                       # EscenaController, EstadoController
│   │   ├── Middleware/         # ErrorHandlingMiddleware
│   │   └── Program.cs          # Composition root
│   └── IoT.ConsoleApp/
│       └── Program.cs          # Menú CLI interactivo (12 opciones)
└── tests/
    └── IoT.Domain.Tests/       # 69 tests unitarios (xUnit)
```

---

## Stack tecnológico

| Área | Tecnología |
|---|---|
| Lenguaje / Runtime | C# 12 · .NET 8 |
| ORM / Base de datos | Entity Framework Core 8.0.15 · SQLite |
| API | ASP.NET Core Web API · Swashbuckle (Swagger) 6.9.0 |
| Caché | `Microsoft.Extensions.Caching.Memory` 8.0.1 |
| Tests | xUnit 2.5.3 |
| Mensajería | Stub en consola (diseñado para RabbitMQ/Kafka sin cambiar el dominio) |

---

## Notas de implementación

- La base de datos SQLite se crea automáticamente con `EnsureCreated()` al iniciar; no hay migraciones de EF.
- Los domain events `HogarRegistrado` y `EscenaCreada` se publican después de `SaveChangesAsync()` para garantizar que el ID generado por la base de datos sea el correcto.
- La ejecución de escenas usa una transacción explícita (`BeginTransactionAsync / CommitAsync / RollbackAsync`) gestionada por `EscenaController`.
- `ISaveChanges` e `IUnitOfWork` resuelven la misma instancia de `UnitOfWork` por scope de DI.
