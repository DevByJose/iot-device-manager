# Revisión Arquitectónica — IoT Device Manager
> Revisado el 2026-05-19 · Guías de referencia: `.claude/skills/clean-architecture-csharp` y `.claude/skills/ddd-csharp`

---

## 1. Resumen ejecutivo

El proyecto implementa una plataforma de gestión de dispositivos IoT para hogares inteligentes. La base arquitectónica es sólida: los building blocks de DDD están correctamente definidos (entidades, value objects, aggregate roots, domain events, domain services) y la separación en capas es en su mayoría respetada. Sin embargo, hay **3 bugs que producen pérdida de datos o comportamiento silencioso incorrecto**, **3 use cases declarados pero no implementados**, y **9 incumplimientos** respecto a las guías de referencia del proyecto.

---

## 2. Estado actual de la solución

### 2.1 Estructura de proyectos y dependencias reales

```
IOT.sln
└── src/
    ├── IoT.Domain          ← sin dependencias externas          ✅
    ├── IoT.Application     ← referencia: Domain                 ✅
    ├── IoT.Infrastructure  ← referencia: Domain + Application   ✅
    ├── IoT.API             ← referencia: Application + Infrastructure  ✅
    └── IoT.ConsoleApp      ← referencia: Domain + Application + Infrastructure  ✅
```

### 2.2 Inventario del dominio

#### Aggregate Roots (3)

| Aggregate | Entidades hijas | Responsabilidad |
|---|---|---|
| `Hogar` | `Habitacion`, `Dispositivo` | Inventario de dispositivos por hogar |
| `Escena` | `AccionEscena`, `Disparador` | Automatización y control por escenas |
| `EstadoDispositivo` | `LecturaSensor`, `AlertaEstado` | Monitoreo de telemetría y anomalías |

#### Entidades sin Aggregate Root

| Entidad | Problema |
|---|---|
| `ComandoDispositivo` | Vive en `IoT.Domain.Entities` pero no pertenece a ningún agregado ni tiene repositorio |

#### Value Objects (9)

| Value Object | Validaciones implementadas |
|---|---|
| `DireccionFisica` | Ciudad y país obligatorios |
| `GeoLocalizacion` | Latitud [-90,90], longitud [-180,180], radio > 0 |
| `IdentificadorFisico` | Valor y tipo no vacíos |
| `VersionFirmware` | Componentes no negativos |
| `NombreEscena` | No vacío, 3–60 caracteres |
| `ParametroComando` | Nombre no vacío |
| `UmbralSensor` | Min < Max, unidad no vacía |
| `CondicionDisparador` | Operador válido de conjunto `{>, <, =, !=, >=, <=}` |
| `IntervaloHorario` | HoraInicio < HoraFin, al menos un día |

#### Domain Events (9)

| Evento | Dónde se eleva | Estado |
|---|---|---|
| `HogarRegistrado` | `Hogar` constructor | ✅ Funciona |
| `DispositivoRegistrado` | `Hogar.RegistrarDispositivo()` | ✅ Funciona |
| `DispositivoDesinstalado` | `Hogar.DesinstalarDispositivo()` | ✅ Funciona |
| `EscenaCreada` | `Escena` constructor | ✅ Funciona |
| `EscenaEjecutada` | `Escena.Ejecutar()` | ✅ Funciona |
| `EstadoCambiado` | `EstadoDispositivo.ActualizarLectura()`, `MarcarDesconectado()` | ✅ Funciona |
| `AnomaliaDetectada` | `EstadoDispositivo.DetectarAnomalia()` | ✅ Funciona |
| `DispositivoDesconectado` | `EstadoDispositivo.MarcarDesconectado()` | ✅ Funciona |
| `ComandoEnviado` / `ComandoConfirmado` / `ComandoFallido` | **Declarados en `DomainEvents.cs`** | ❌ **Nunca se elevan** |

#### Domain Services (5)

| Servicio | Responsabilidad |
|---|---|
| `SvcRegistroDispositivo` | Unicidad global de `IdentificadorFisico` antes de delegar a `Hogar` |
| `SvcEjecucionEscena` | Traduce `AccionEscena` en `ComandoDispositivo` secuenciales |
| `SvcValidacionComando` | Valida comandos según tipo de dispositivo (hardcodeado) |
| `SvcConsolidacionEstado` | Wrapper de `EstadoDispositivo.ActualizarLectura()` |
| `SvcDeteccionAnomalia` | Wrapper de `EstadoDispositivo.DetectarAnomalia()` |

### 2.3 Interfaces de dominio

| Interfaz | Ubicación actual | Ubicación correcta (según skill) |
|---|---|---|
| `IHogarRepository` | `IoT.Domain` | ✅ Correcto |
| `IDispositivoRepository` | `IoT.Domain` | ✅ Correcto |
| `IEscenaRepository` | `IoT.Domain` | ✅ Correcto |
| `IEstadoRepository` | `IoT.Domain` | ✅ Correcto |
| `IEventPublisher` | `IoT.Domain` | ✅ Correcto |
| `IUnitOfWork` | `IoT.Domain` | ⚠️ Debería estar en `IoT.Application` |
| `ICacheService` | `IoT.Domain` | ❌ Debe estar en `IoT.Application` |

### 2.4 Endpoints de la API

| Método | Ruta | Handler | Estado |
|---|---|---|---|
| `POST` | `/api/hogar` | `RegistrarHogarHandler` | ✅ Implementado |
| `GET` | `/api/hogar/cliente/{clienteId}` | `ObtenerHogaresHandler` | ✅ Implementado |
| `POST` | `/api/dispositivo` | `RegistrarDispositivoHandler` | ✅ Implementado |
| `GET` | `/api/dispositivo/hogar/{hogarId}` | `ObtenerDispositivosHandler` | ✅ Implementado |
| `POST` | `/api/escena/{escenaId}/ejecutar` | `EjecutarEscenaHandler` | ✅ Implementado |
| `GET` | `/api/estado/{dispositivoId}` | `ConsultarEstadoHandler` (con caché 30s) | ✅ Implementado |

### 2.5 Use cases declarados sin implementar

| Command / Query | Handler | Endpoint API |
|---|---|---|
| `CrearEscenaCommand` | ❌ Sin handler | ❌ Sin endpoint |
| `EnviarComandoCommand` | ❌ Sin handler | ❌ Sin endpoint |
| `ObtenerTelemetriaQuery` | ❌ Sin handler | ❌ Sin endpoint |

### 2.6 Infraestructura

| Componente | Implementación actual | Observación |
|---|---|---|
| Base de datos | SQLite (`hogarconectado.db`) | `EnsureCreated()` al inicio, sin migraciones |
| Event bus | `EventBusPublisher` → `Console.WriteLine` | Stub; diseñado para reemplazar con RabbitMQ/Kafka |
| Caché | `IMemoryCache` de ASP.NET Core, TTL 30s | Solo para `ConsultarEstadoHandler` |
| Unit of Work | Wrapper sobre `DbContext` + `IDbContextTransaction` | ✅ Funciona |

---

## 3. Cumplimiento — Clean Architecture

### ✅ Cumple

- Regla de dependencias respetada en código (Domain sin NuGet externos)
- `IoT.API` actúa como Composition Root
- Controllers sin lógica de negocio
- Repositorios definidos en Domain, implementados en Infrastructure
- `ErrorHandlingMiddleware` como capa más externa (`DomainException` → 400, genérica → 500)
- Infrastructure referencia Application (explícitamente permitido por la guía)

### ❌ No cumple

**CA-1 — `ICacheService` en Domain (debería estar en Application)**
> Guía: *"Application define interfaces para servicios externos que Infrastructure implementa"*

`ICacheService` es una optimización de rendimiento, no un contrato de negocio. Solo la usa `ConsultarEstadoHandler` (Application). Debe moverse a `IoT.Application/Common/Interfaces/`.

**CA-2 — Sin `DependencyInjection.cs` por capa**
> Guía: `AddApplication()` / `AddInfrastructure()` como métodos de extensión en cada proyecto

Todo el registro de DI está concentrado en `Program.cs`. Esto acopla la API al conocimiento interno de cada capa.

**CA-3 — `CommandValidators` lanza `DomainException` para validación de inputs**
> Guía: `FluentValidation` + `ValidationBehavior<TRequest>` + `ValidationException` para validación de forma

`DomainException` debe reservarse para violaciones de invariantes de negocio (p.ej. "la habitación no pertenece al hogar"). Validar que un campo string no esté vacío es validación de entrada, no regla de dominio. La guía usa `FluentValidation` con un pipeline behavior.

**CA-4 — Sin MediatR ni `IRequest<T>` / `ISender`**
> Guía: CQRS con MediatR como estándar para dispatch de Commands y Queries

Los handlers se inyectan directamente en los controllers. Agregar un nuevo handler requiere modificar el controller. Con MediatR, el controller solo necesita `ISender`.

**CA-5 — Sin proyectos de tests**
> Guía: `tests/Domain.Tests`, `Application.Tests`, `Integration.Tests`

La arquitectura está completamente preparada para testing (interfaces en todo lado, dominio sin frameworks), pero no hay ni un test implementado.

---

## 4. Cumplimiento — DDD

### ✅ Cumple

- Tres Aggregate Roots bien definidos con invariantes protegidas
- Value Objects inmutables con igualdad por valor (`GetEqualityComponents`)
- Domain Events con `EventId` y `OccurredOn`
- Domain Services para lógica cross-aggregate
- Acceso a entidades hijas exclusivamente a través del AR
- Lenguaje ubicuo en español, consistente en todo el código
- `AggregateRoot` acumula eventos; handlers los publican y limpian post-transacción
- `Entity` con igualdad por identidad y operadores `==` / `!=`

### ❌ No cumple

**DDD-1 — `ComandoDispositivo` es una entidad huérfana**
> Guía: *"Los repositorios solo guardan y recuperan Aggregate Roots"*

`ComandoDispositivo` existe en `IoT.Domain.Entities`, tiene estado (`Pendiente → Enviado → Confirmado | Fallido`), pero no pertenece a ningún Aggregate Root y no tiene repositorio. `SvcEjecucionEscena` la crea en memoria y la descarta; nunca se persiste.

**DDD-2 — `ComandoEnviado`, `ComandoConfirmado`, `ComandoFallido` nunca se elevan**
> Guía: *"Los Domain Events se disparan dentro del aggregate"*

Los tres eventos están declarados en `DomainEvents.cs` pero ninguna entidad o servicio llama `AddDomainEvent()` con ellos. Son eventos muertos que no producen ningún efecto.

**DDD-3 — Bounded Contexts sin separación ni comunicación**
> Guía: cada BC tiene su propia carpeta; comunicación via Integration Events (no Domain Events)

Los tres contextos (Inventario, Monitoreo, Automatización) conviven en el mismo namespace `IoT.Domain.Entities` sin separación. No existe ningún mecanismo de comunicación entre ellos:
- Si se desinstala un `Dispositivo`, su `EstadoDispositivo` asociado no se limpia
- Si un `Dispositivo` se desconecta, las `Escena` que lo referencian no se notifican

**DDD-4 — Domain sin organización por contexto**
> Guía: `Domain/Orders/`, `Domain/Customers/` — cada contexto agrupa sus propios archivos

La estructura actual es plana (`Entities/`, `ValueObjects/`). Mezcla los tres contextos en las mismas carpetas.

**DDD-5 — `Entity` usa `int Id` sin typed IDs**
> Guía: `Entity<TId>` con `OrderId`, `CustomerId` — previene pasar `hogarId` donde va `dispositivoId`

```csharp
// Actual — el compilador no detecta esto:
hogar.RegistrarDispositivo(hogarId, nombre, ...);  // error semántico silencioso

// Con typed IDs:
hogar.RegistrarDispositivo(new DispositivoId(0), nombre, ...);  // error de compilación
```

**DDD-6 — `Dispositivo.Estado` es `string` primitivo**
> Guía: `OrderStatus` como tipo fuerte (enum o Value Object)

El estado admite cualquier cadena, incluyendo `$"Error: {motivo}"` que rompe el concepto de estado finito. Debe ser un enum o Value Object con transiciones explícitas.

**DDD-7 — `CondicionDisparador` nunca se evalúa**

El Value Object almacena `(operandoIzq, operador, operandoDer)` con validación correcta, pero no tiene método `Evaluar(contexto)`. Ninguna parte del sistema evalúa la condición de un `Disparador` para activar una `Escena` automáticamente. La automatización por sensor/horario/geolocalización es infraestructura no implementada.

**DDD-8 — `SvcValidacionComando` con tipos de dispositivo hardcodeados**

```csharp
private static readonly Dictionary<string, HashSet<string>> ComandosPorTipo = new()
{
    ["Smartlight"] = new() { "TurnOn", "TurnOff", "SetColor", "SetSchedule" },
    ["Camera"]     = new() { "StartRecording", "CaptureSnapshot", "TurnOn", "TurnOff" },
    ["Alarm"]      = new() { "Trigger", "Stop", "TurnOn", "TurnOff" }
};
```

Agregar un nuevo tipo de dispositivo requiere modificar el servicio de dominio (viola OCP). Esta información debería ser parte del modelo del `Dispositivo`.

---

## 5. Bugs

### BUG-1 — `IntervaloHorario.DiasSemana` nunca se persiste (pérdida de datos)

**Severidad: Alta**

En `HogarConectadoDbContext`:
```csharp
e.OwnsOne(d => d.Horario, h => h.Ignore(x => x.DiasSemana));
```
`DiasSemana` está explícitamente ignorado por EF Core. Un `Disparador` con tipo `"Horario"` guardado y recargado desde la BD tendrá `DiasSemana = []`. La automatización horaria nunca funcionará correctamente.

**Causa:** `IReadOnlyList<DayOfWeek>` no tiene mapeo directo en SQLite sin una tabla de unión o serialización a JSON.

### BUG-2 — `DispositivoDto.Habitacion` siempre retorna `string.Empty`

**Severidad: Media**

En `DomainToDtoMapper.ToDto(Dispositivo)`:
```csharp
public static DispositivoDto ToDto(Dispositivo dispositivo) => new(
    ..., dispositivo.Firmware.ToString(), string.Empty);  // ← Habitacion siempre vacía
```
El DTO declara el campo `Habitacion` pero el mapper nunca lo resuelve. Todo cliente de la API que consuma `GET /api/dispositivo/hogar/{id}` recibirá el nombre de habitación vacío.

### BUG-3 — `ComandoDispositivo` se crea pero nunca se persiste

**Severidad: Media**

`SvcEjecucionEscena.EjecutarAsync()` crea instancias de `ComandoDispositivo`, las retorna al handler, y el handler cuenta cuántas fallaron. Pero nunca se llama a ningún repositorio para guardarlos. El historial de comandos enviados a dispositivos no existe en la BD.

---

## 6. Plan de mejoras priorizado

### Prioridad 1 — Bugs críticos (afectan funcionalidad y datos)

| # | Acción | Archivo |
|---|---|---|
| P1-A | Persistir `DiasSemana` en `IntervaloHorario` (columna JSON o tabla de unión) | `HogarConectadoDbContext.cs` |
| P1-B | Resolver `Habitacion` en `DomainToDtoMapper.ToDto(Dispositivo)` | `DomainToDtoMapper.cs` |
| P1-C | Crear `IComandoRepository` y persistir `ComandoDispositivo` en `EjecutarEscenaHandler` | Nuevo archivo + `Handlers.cs` |

### Prioridad 2 — Incumplimientos de DDD con impacto en modelo

| # | Acción | Archivo |
|---|---|---|
| P2-A | Definir a qué Aggregate Root pertenece `ComandoDispositivo` (propuesta: nuevo AR `HistorialComandos` o hijo de `EstadoDispositivo`) | Nuevo modelo |
| P2-B | Elevar `ComandoEnviado` / `ComandoConfirmado` / `ComandoFallido` desde el agregado correspondiente | `DomainEvents.cs` + entidad dueña |
| P2-C | Convertir `Dispositivo.Estado` en enum `EstadoDispositivo` (renombrar para no colisionar con la entidad) | `Hogar.cs` (donde vive `Dispositivo`) |

### Prioridad 3 — Incumplimientos de Clean Architecture

| # | Acción | Archivo |
|---|---|---|
| P3-A | Mover `ICacheService` de `IoT.Domain` a `IoT.Application/Common/Interfaces/` | `DomainInterfaces.cs` → nuevo archivo |
| P3-B | Mover `IUnitOfWork` de `IoT.Domain` a `IoT.Application/Common/Interfaces/` | `DomainInterfaces.cs` → nuevo archivo |
| P3-C | Crear `DependencyInjection.cs` en Application e Infrastructure | Nuevos archivos |
| P3-D | Reemplazar `CommandValidators` + `DomainException` por `FluentValidation` + `ValidationException` | `CommandValidators.cs` |

### Prioridad 4 — Use cases faltantes

| # | Acción |
|---|---|
| P4-A | Implementar `CrearEscenaHandler` + `POST /api/escena` |
| P4-B | Implementar `EnviarComandoHandler` + `POST /api/dispositivo/{id}/comando` |
| P4-C | Implementar `ObtenerTelemetriaHandler` + `GET /api/estado/{id}/telemetria?desde=&hasta=` |

### Prioridad 5 — Mejoras de diseño DDD

| # | Acción |
|---|---|
| P5-A | Organizar `IoT.Domain` en subcarpetas por Bounded Context (`Inventario/`, `Monitoreo/`, `Automatizacion/`) |
| P5-B | Migrar `Entity` a `Entity<TId>` con typed IDs (`HogarId`, `DispositivoId`, etc.) |
| P5-C | Añadir Integration Events para comunicación cross-context (`DispositivoDesinstalado` → limpia `EstadoDispositivo`) |
| P5-D | Reemplazar tipos de dispositivo hardcodeados en `SvcValidacionComando` con una abstracción del modelo |
| P5-E | Agregar proyectos de tests (`IoT.Domain.Tests`, `IoT.Application.Tests`) |
| P5-F | Introducir MediatR para dispatch de Commands/Queries |

---

## 7. Matriz de estado final

| Criterio | Estado |
|---|---|
| Regla de dependencias (código) | ✅ |
| Aggregate Roots con invariantes | ✅ |
| Value Objects inmutables | ✅ |
| Domain Events bien estructurados | ✅ |
| Domain Services | ✅ |
| Controllers delgados | ✅ |
| `ICacheService` / `IUnitOfWork` en capa correcta | ❌ |
| DI por capa (`AddApplication` / `AddInfrastructure`) | ❌ |
| Validación de inputs separada de Domain Exceptions | ❌ |
| MediatR / `IRequest<T>` | ❌ |
| Bounded Contexts separados | ❌ |
| Typed IDs | ❌ |
| `Dispositivo.Estado` tipado | ❌ |
| `ComandoDispositivo` con Aggregate Root | ❌ |
| Todos los Domain Events se elevan | ❌ |
| `DiasSemana` persistido | ❌ (bug) |
| `DispositivoDto.Habitacion` resuelto | ❌ (bug) |
| `ComandoDispositivo` persistido | ❌ (bug) |
| Tests | ❌ |
| Use cases completos | ❌ (3 faltantes) |
