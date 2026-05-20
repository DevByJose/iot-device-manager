# Guía de Sustentación — IoT Device Manager

> Proyecto: Plataforma de gestión de dispositivos IoT para hogares inteligentes  
> Stack: .NET 8 · C# 12 · Clean Architecture · DDD · EF Core + SQLite · xUnit  
> Fecha: 2026-05-20

---

## 1. Mapa de la rúbrica (25 puntos)

| Criterio | Puntos | Estado | Referencia en código |
|---|---|---|---|
| Capa de Dominio completa | 10 | ✅ Cubierto | `src/IoT.Domain/` |
| Capa de Aplicación | 5 | ✅ Cubierto | `src/IoT.Application/` |
| Capa de Infraestructura | 5 | ✅ Cubierto | `src/IoT.Infrastructure/` |
| Capa externa (API) | 5 | ✅ Cubierto | `src/IoT.API/` |

---

## 2. Capa de Dominio — 10 puntos

### 2.1 Entidades ricas con comportamiento

Las entidades no son simples bolsas de datos; cada una implementa invariantes y comportamiento propio.

**`Hogar` (Aggregate Root) — `src/IoT.Domain/Entities/Hogar.cs`**
- `AgregarHabitacion()` → valida nombre duplicado (case-insensitive)
- `RegistrarDispositivo()` → valida que la habitación pertenezca al hogar antes de registrar
- `DesinstalarDispositivo()` → eleva evento `DispositivoDesinstalado`
- `ConectarDispositivo()` / `DesconectarDispositivo()` → acceso protegido via AR

**`Escena` (Aggregate Root) — `src/IoT.Domain/Entities/Escena.cs`**
- `Ejecutar()` → valida que esté activa y tenga acciones; eleva `EscenaEjecutada`
- `Activar()` / `Desactivar()` / `CambiarNombre()` → transiciones de estado encapsuladas

**`ComandoDispositivo` (Aggregate Root) — `src/IoT.Domain/Entities/ComandoDispositivo.cs`**
- Máquina de estados explícita: `Pendiente → Enviado → Confirmado | Fallido`
- `MarcarEnviado()` → solo desde `Pendiente`, eleva `ComandoEnviado`
- `Confirmar()` → solo desde `Enviado`, eleva `ComandoConfirmado`
- `MarcarFallido()` → desde cualquier estado, eleva `ComandoFallido`

**`EstadoDispositivo` (Aggregate Root) — `src/IoT.Domain/Entities/EstadoDispositivo.cs`**
- Rechaza lecturas con timestamp anterior a la última consolidada (evita duplicados)
- `DetectarAnomalia()` → compara lectura contra umbrales y eleva `AnomaliaDetectada`

---

### 2.2 Value Objects inmutables

**Todos en `src/IoT.Domain/ValueObjects/`** — 9 value objects, todos con:
- Propiedades solo con `init` (inmutables tras construcción)
- Validaciones en constructor que lanzan `DomainException`
- Igualdad por valor via `GetEqualityComponents()` heredado de `ValueObject`

| Value Object | Validación clave |
|---|---|
| `DireccionFisica` | Ciudad y País obligatorios |
| `GeoLocalizacion` | Latitud ∈ [-90,90], Longitud ∈ [-180,180] |
| `IdentificadorFisico` | Valor y TipoIdentificador no vacíos |
| `VersionFirmware` | Componentes ≥ 0; `ToString()` → `"1.2.3"` |
| `NombreEscena` | No vacío, 3–60 caracteres |
| `ParametroComando` | Nombre no vacío |
| `UmbralSensor` | Min < Max, Unidad no vacía |
| `CondicionDisparador` | Operador ∈ `{>, <, =, !=, >=, <=}` |
| `IntervaloHorario` | HoraInicio < HoraFin, al menos un día |

**Punto de demostración:** Mostrar cómo `NombreEscena` lanza excepción con menos de 3 caracteres o más de 60. Mostrar que dos instancias con el mismo valor son iguales (`==`).

---

### 2.3 Agregados con consistencia

**4 Aggregate Roots identificados:**

| AR | Entidades hijas | Invariante protegida |
|---|---|---|
| `Hogar` | `Habitacion`, `Dispositivo` | Un dispositivo solo puede registrarse en una habitación del mismo hogar |
| `Escena` | `AccionEscena`, `Disparador` | Solo se ejecuta si está activa y tiene acciones |
| `EstadoDispositivo` | `LecturaSensor`, `AlertaEstado` | No acepta lecturas con timestamp duplicado o anterior |
| `ComandoDispositivo` | — | Transiciones de estado solo en orden válido |

**Regla clave:** ningún código accede a `Habitacion`, `AccionEscena`, `LecturaSensor` directamente — solo a través de su AR.  
`Habitaciones`, `Dispositivos`, `Acciones`, `Lecturas` son `IReadOnlyCollection<T>` → la colección interna es privada (`private readonly List<T>`).

---

### 2.4 Domain Events

**11 eventos en `src/IoT.Domain/Events/`** — todos nombrados en pasado, todos implementan `IDomainEvent` con `EventId` (Guid) y `OccurredOn` (DateTime).

| Evento | Se eleva en | Cuándo |
|---|---|---|
| `HogarRegistrado` | `Hogar.ConfirmarRegistro()` | Tras `SaveChangesAsync()` para tener el ID real |
| `DispositivoRegistrado` | `Hogar.RegistrarDispositivo()` | Al agregar el dispositivo al agregado |
| `DispositivoDesinstalado` | `Hogar.DesinstalarDispositivo()` | Al remover el dispositivo |
| `EscenaCreada` | `Escena.ConfirmarCreacion()` | Tras `SaveChangesAsync()` para tener el ID real |
| `EscenaEjecutada` | `Escena.Ejecutar()` | Al ejecutar las acciones |
| `EstadoCambiado` | `EstadoDispositivo.ActualizarLectura()` | Cuando el estado real cambia |
| `AnomaliaDetectada` | `EstadoDispositivo.DetectarAnomalia()` | Cuando una lectura supera umbrales |
| `DispositivoDesconectado` | `EstadoDispositivo.MarcarDesconectado()` | Al marcar offline |
| `ComandoEnviado` | `ComandoDispositivo.MarcarEnviado()` | Transición Pendiente → Enviado |
| `ComandoConfirmado` | `ComandoDispositivo.Confirmar()` | Transición Enviado → Confirmado |
| `ComandoFallido` | `ComandoDispositivo.MarcarFallido()` | Fallo en cualquier estado |

**Flujo de publicación:** Los handlers acumulan eventos del AR, llaman `SaveChangesAsync()`, luego llaman `IEventPublisher.PublishAllAsync(ar.DomainEvents)` y limpian con `ar.ClearDomainEvents()`.  
**Por qué `ConfirmarRegistro()`/`ConfirmarCreacion()`:** Si el evento se elevaba en el constructor, `Id` era `0` (EF no lo asigna hasta persistir). La solución fue elevar el evento **después** de la persistencia, desde el handler.

---

### 2.5 Interfaces de Dominio

**`src/IoT.Domain/Interfaces/DomainInterfaces.cs`** — definidas en Dominio, implementadas en Infraestructura (DIP).

```
IHogarRepository       → HogarRepository      (Infrastructure)
IDispositivoRepository → DispositivoRepository (Infrastructure)
IEscenaRepository      → EscenaRepository      (Infrastructure)
IEstadoRepository      → EstadoRepository      (Infrastructure)
IComandoRepository     → ComandoRepository     (Infrastructure)
IEventPublisher        → EventBusPublisher     (Infrastructure)
```

**`src/IoT.Application/Interfaces/ApplicationInterfaces.cs`** — interfaces de orquestación (no son contratos de negocio):
```
ICacheService   → CacheService  (Infrastructure)
ISaveChanges    → UnitOfWork    (Infrastructure)
IUnitOfWork     → UnitOfWork    (Infrastructure)
```

**Por qué `ICacheService` e `IUnitOfWork` están en Application, no en Domain:**  
La caché y las transacciones son preocupaciones técnicas de orquestación. El dominio puro no sabe que existe una caché ni que hay transacciones de base de datos. Solo `Application` (handlers) los usa.

---

## 3. Capa de Aplicación — 5 puntos

### 3.1 Use cases específicos (1 handler = 1 acción de negocio)

**13 handlers en `src/IoT.Application/Handlers/Handlers.cs`:**

| Handler | Acción única |
|---|---|
| `RegistrarHogarHandler` | Registrar un hogar y emitir `HogarRegistrado` |
| `ObtenerHogaresHandler` | Consultar hogares de un cliente |
| `AgregarHabitacionHandler` | Agregar habitación a hogar existente |
| `ObtenerHabitacionesHandler` | Listar habitaciones de un hogar |
| `RegistrarDispositivoHandler` | Registrar dispositivo (con unicidad de ID físico) |
| `ObtenerDispositivosHandler` | Listar dispositivos de un hogar |
| `ConectarDispositivoHandler` | Marcar dispositivo como Online |
| `DesconectarDispositivoHandler` | Marcar dispositivo como Offline |
| `EnviarComandoHandler` | Enviar comando validado a un dispositivo |
| `ConsultarEstadoHandler` | Consultar estado (con caché 30s) |
| `ObtenerTelemetriaHandler` | Historial de lecturas por rango de fecha |
| `CrearEscenaHandler` | Crear escena con acciones |
| `EjecutarEscenaHandler` | Ejecutar escena (coordina con `SvcEjecucionEscena`) |

---

### 3.2 Orquestación sin lógica de negocio

Los handlers no contienen reglas de negocio. Delegan a:
- **Aggregate Roots** para invariantes del dominio
- **Servicios de Dominio** para lógica cross-aggregate

Ejemplo — `RegistrarDispositivoHandler`:
```
1. Carga Hogar del repositorio
2. Delega a SvcRegistroDispositivo.RegistrarAsync() → valida unicidad de ID físico global
3. SvcRegistroDispositivo delega a Hogar.RegistrarDispositivo() → valida habitación pertenece al hogar
4. Handler persiste y publica eventos
```
El handler no sabe nada de validaciones; las reglas viven en el dominio.

---

### 3.3 Transacciones bien manejadas

**Caso simple (la mayoría de handlers):**  
Usan `ISaveChanges.SaveChangesAsync()` — interfaz mínima sin transacciones explícitas.

**Caso complejo (`EjecutarEscena`):**  
`EscenaController` abre una transacción explícita y el handler orquesta múltiples escrituras:
```csharp
await _uow.BeginTransactionAsync();
try {
    var result = await _ejecutarHandler.HandleAsync(command); // escena + N comandos
    await _uow.CommitAsync();
} catch {
    await _uow.RollbackAsync();
    throw;
}
```
Esto garantiza que todos los `ComandoDispositivo` de la escena se persisten atómicamente o ninguno.

**ISP (Interface Segregation):** `ISaveChanges` es la interfaz mínima usada por handlers. `IUnitOfWork : ISaveChanges` agrega transacciones y solo la usa `EscenaController`.  
Ambas interfaces resuelven la **misma instancia** de `UnitOfWork` por scope de DI:
```csharp
services.AddScoped<UnitOfWork>();
services.AddScoped<ISaveChanges>(sp => sp.GetRequiredService<UnitOfWork>());
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
```

---

## 4. Capa de Infraestructura — 5 puntos

### 4.1 Repositorios

**5 repositorios en `src/IoT.Infrastructure/Repositories/Repositories.cs`:**

- Implementan las interfaces definidas en `IoT.Domain` (DIP)
- **No contienen referencias a tecnologías específicas** en sus firmas públicas: reciben y retornan entidades del dominio (`Hogar`, `Escena`, etc.), nunca DTOs o tipos de EF
- `SaveAsync()` detecta si es insert o update con `AnyAsync()` → upsert transparente
- `ExisteIdentificadorAsync()` usa `d.Identificador.Valor` (propiedad de dominio), no `EF.Property<>()`

---

### 4.2 ORM — EF Core + SQLite

**`src/IoT.Infrastructure/Persistence/HogarConectadoDbContext.cs`**

- Value Objects mapeados con `OwnsOne` (EF Core owned entities):
  ```csharp
  e.OwnsOne(h => h.Direccion, d => { d.Property(x => x.Ciudad)... });
  e.OwnsOne(d => d.Firmware);
  ```
- `DiasSemana` (lista de enums) serializado a TEXT con conversión JSON:
  ```csharp
  h.Property(x => x.DiasSemana).HasConversion(
      v => string.Join(',', v.Select(d => (int)d)),
      v => v.Split(',').Select(s => (DayOfWeek)int.Parse(s))...
  ```
- `ComandoDispositivo.Id` configurado con `ValueGeneratedOnAdd()` para que SQLite genere el ID automáticamente al insertar múltiples comandos en una sola transacción
- El **dominio no importa `Microsoft.EntityFrameworkCore`** — cero dependencias externas en `IoT.Domain.csproj`

---

### 4.3 Caché en memoria

**`ConsultarEstadoHandler`** implementa el patrón cache-aside:
1. Busca en caché (`IMemoryCache`) con clave `estado:dispositivo:{id}`
2. Si hay hit → retorna el DTO directo (sin hit a BD)
3. Si hay miss → consulta repositorio, almacena con TTL 30s, retorna

`ICacheService` está definida en `Application` e implementada en `Infrastructure` (DIP). El dominio no sabe que existe una caché.

---

## 5. Capa API — 5 puntos

### 5.1 Controllers delgados

**4 controllers en `src/IoT.API/Controllers/Controllers.cs`** — ninguno contiene lógica de negocio:
1. Recibe request HTTP
2. Valida entrada en el boundary (`CommandValidators.Validate()`)
3. Despacha al handler correspondiente
4. Retorna el resultado HTTP apropiado (`201 Created`, `200 OK`, `204 No Content`)

`EscenaController` es el único que hace algo adicional: maneja la transacción explícita (`BeginTransactionAsync/CommitAsync/RollbackAsync`). Esto es gestión de consistencia, no lógica de negocio.

---

### 5.2 DTOs y validación en boundaries

**9 DTOs en `src/IoT.Application/DTOs/`** — `sealed record` para transporte entre capas:

| DTO | Campos clave |
|---|---|
| `HogarDto` | Id, Nombre, Ciudad, Pais, TotalHabitaciones, TotalDispositivos |
| `HabitacionDto` | Id, Nombre, HogarId |
| `DispositivoDto` | Id, Nombre, TipoDispositivo, Estado, Habitacion, Firmware |
| `EscenaDto` | Id, Nombre, HogarId, Activa, TotalAcciones |
| `EstadoDispositivoDto` | Estado, Conectado, UltimoValor, UltimaActualizacion, TotalAlertas |
| `ComandoDispositivoDto` | Id, DispositivoId, Comando, Estado, CreadoEn |
| `LecturaSensorDto` | DispositivoId, Valor, Unidad, Timestamp |
| `RegisterDeviceResponse` | DispositivoId, Nombre, TipoDispositivo, Exito, Mensaje |
| `EjecutarEscenaResponse` | EscenaId, ComandosEnviados, ComandosFallidos, Detalles |

**Mapeo explícito** en `src/IoT.Application/Mappings/DomainToDtoMapper.cs` — método estático `ToDto()` por cada tipo de entidad. Sin AutoMapper; el mapeo es intencional y visible.

**Validación en boundaries** (`CommandValidators`) — verifica campos requeridos antes de pasar al handler:
```csharp
CommandValidators.Validate(command); // lanza DomainException si input inválido
var result = await _handler.HandleAsync(command);
```

---

## 6. SOLID — Evidencias concretas

| Principio | Implementación | Dónde verlo |
|---|---|---|
| **S** — Single Responsibility | Un handler por use case; un controller por área; un servicio de dominio por responsabilidad | `Handlers.cs`, `Controllers.cs` |
| **O** — Open/Closed | `IValidadorTipoDispositivo` + estrategias por tipo. Agregar un nuevo tipo de dispositivo = solo una clase nueva, sin modificar código existente | `ValidadoresTipoDispositivo.cs`, `SvcValidacionComando.cs` |
| **L** — Liskov Substitution | Todos los repositorios implementan sus interfaces sin restricciones adicionales. `HogarRepository` puede usarse donde se espera `IHogarRepository` sin sorpresas | `Repositories.cs` |
| **I** — Interface Segregation | `ISaveChanges` (solo `SaveChangesAsync`) vs `IUnitOfWork : ISaveChanges` (agrega transacciones). Los handlers usan la interfaz mínima | `ApplicationInterfaces.cs` |
| **D** — Dependency Inversion | El dominio define contratos (`IHogarRepository`, `IEventPublisher`); Infrastructure los implementa. El dominio no depende de EF ni de ningún framework | `DomainInterfaces.cs` |

**OCP — punto más importante para demostrar:**  
Antes, `SvcValidacionComando` tenía un `Dictionary` hardcodeado con los tipos. Agregar `"Thermostat"` requería modificar el servicio de dominio. Ahora:
```csharp
// SvcValidacionComando recibe IEnumerable<IValidadorTipoDispositivo> por DI
// Para agregar un tipo nuevo, solo se registra:
services.AddSingleton<IValidadorTipoDispositivo, ValidadorThermostat>();
// Cero cambios en código existente
```

---

## 7. Flujo completo para demostración

### Flujo recomendado en Swagger / Consola

```
1. POST /api/hogar
   Body: { "nombre": "Casa Test", "ciudad": "Medellín", "pais": "Colombia", ... }
   → Retorna hogarId

2. POST /api/hogar/{hogarId}/habitacion
   Body: { "nombre": "Sala" }
   → Retorna habitacionId

3. POST /api/dispositivo
   Body: { "hogarId": 1, "nombre": "Luz Sala", "tipoDispositivo": "Smartlight",
           "identificadorFisico": "AA:BB:CC:DD", "tipoIdentificador": "MAC",
           "habitacionId": 1 }
   → Retorna dispositivoId

4. PUT /api/dispositivo/{id}/conectar
   → Dispositivo pasa a "Online"

5. POST /api/dispositivo/{id}/comando
   Body: { "comando": "TurnOn" }
   → Valida que el comando es válido para Smartlight; persiste ComandoDispositivo

6. POST /api/escena
   Body: { "hogarId": 1, "nombre": "Escena Noche",
           "acciones": [{ "orden": 1, "dispositivoId": 1, "comando": "TurnOff" }] }
   → Crea escena con acciones

7. POST /api/escena/{escenaId}/ejecutar
   → Transacción explícita; crea y persiste todos los ComandoDispositivo atómicamente

8. GET /api/dispositivo/hogar/{hogarId}
   → Lista dispositivos con estado actualizado
```

### Qué demostrar de cada capa

| Capa | Qué abrir / mostrar |
|---|---|
| Domain | `Hogar.cs` línea 62–75: validación de habitación antes de registrar; `ComandoDispositivo.cs` línea 35–56: máquina de estados |
| Value Objects | `NombreEscena.cs`: constructor con validación; test `ValueObjectTests.cs` |
| Application | `Handlers.cs` `RegistrarDispositivoHandler`: delegación al servicio, sin lógica propia |
| Infrastructure | `HogarConectadoDbContext.cs` línea 26–44: `OwnsOne` para Value Objects; `Repositories.cs`: retorna entidades, no tipos EF |
| API | `Controllers.cs` `EscenaController.Ejecutar()` línea 159–175: transacción en controller, handler sin saber de la transacción |
| OCP | `ValidadoresTipoDispositivo.cs`: 3 estrategias; `SvcValidacionComando.cs`: usa diccionario por DI |
| ISP | `ApplicationInterfaces.cs`: `ISaveChanges` vs `IUnitOfWork`; `Handlers.cs`: todos usan `ISaveChanges` |

---

## 8. Tests unitarios — 69 tests

**Proyecto:** `tests/IoT.Domain.Tests/` (xUnit)

```bash
dotnet test tests/IoT.Domain.Tests
# → 69 passed, 0 failed
```

| Archivo | Tests | Qué valida |
|---|---|---|
| `ValueObjectTests.cs` | 14 | Inmutabilidad, validaciones, igualdad por valor |
| `HogarTests.cs` | 12 | Invariantes del AR: habitaciones duplicadas, habitación ajena, eventos elevados |
| `EscenaYComandoTests.cs` | 13 | Escena inactiva/sin acciones, orden de acciones, máquina de estados de `ComandoDispositivo` |
| `EstadoDispositivoTests.cs` | 9 | Lecturas antiguas descartadas, `EstadoCambiado` solo cuando cambia, anomalías |
| `SvcValidacionComandoTests.cs` | 21 | Comandos válidos/inválidos por tipo, dispositivo desconectado |

**Puntos clave de testing:**
- Los tests son 100% de dominio puro — sin mocks de EF, sin base de datos, sin HTTP
- `SvcValidacionComando` recibe `IValidadorTipoDispositivo[]` por constructor (OCP testeable en aislamiento)
- Los tests verifican que los domain events correctos se eleven (`Assert.Contains(hogar.DomainEvents, e => e is DispositivoRegistrado)`)

---

## 9. Preguntas frecuentes y respuestas

### ¿Por qué usar Clean Architecture y no MVC tradicional?

Clean Architecture separa el núcleo de negocio (Domain) de los detalles técnicos (base de datos, HTTP, framework). Si mañana cambiamos SQLite por PostgreSQL, o REST por gRPC, el dominio no cambia. Los tests del dominio no necesitan infraestructura.

### ¿Qué es un Aggregate Root y por qué importa?

Es la única puerta de entrada a un grupo de entidades relacionadas. `Hogar` es el AR que contiene `Dispositivo` y `Habitacion`. Ningún código externo modifica un dispositivo directamente — siempre va por `hogar.ConectarDispositivo(id)`. Esto garantiza que las invariantes del agregado siempre se respetan.

### ¿Por qué los Value Objects son inmutables?

Porque representan un concepto del dominio sin identidad propia. `DireccionFisica("Calle 10", "45", "Medellín", "Colombia", "050001")` es un valor; si la dirección cambia, se reemplaza el objeto completo, no se muta. Esto elimina toda una clase de bugs por estado compartido mutable.

### ¿Cómo funciona el patrón OCP con los validadores?

`SvcValidacionComando` recibe `IEnumerable<IValidadorTipoDispositivo>` en el constructor (inyectado por DI). Cada tipo de dispositivo tiene su propia clase con su lista de comandos válidos. Para agregar `Thermostat`, solo se crea `ValidadorThermostat : IValidadorTipoDispositivo` y se registra en DI. El `SvcValidacionComando` no cambia una sola línea.

### ¿Por qué `HogarRegistrado` se eleva después de `SaveChangesAsync()`?

EF Core asigna el `Id` a la entidad **después** de ejecutar el INSERT en la base de datos. Si el evento se elevaba en el constructor, `Id` era `0`. La solución fue el método `ConfirmarRegistro()` en el AR, que el handler llama explícitamente tras `SaveChangesAsync()`. Así el evento lleva el ID correcto.

### ¿Por qué `ISaveChanges` e `IUnitOfWork` son interfaces diferentes?

ISP (Interface Segregation Principle). La mayoría de handlers solo necesitan persistir — les basta `ISaveChanges.SaveChangesAsync()`. Solo `EscenaController` necesita transacciones explícitas (`BeginTransactionAsync/CommitAsync/RollbackAsync`). Si todos dependieran de `IUnitOfWork`, estarían acoplados a métodos que no usan.

### ¿Por qué los repositorios no tienen referencias a SQL?

Porque las interfaces están definidas en el Dominio y trabajan con entidades del dominio. `IHogarRepository.GetByIdAsync(int id)` retorna un `Hogar?`, no un `DbSet<Hogar>` ni un `DataRow`. La implementación en Infrastructure sí usa EF Core, pero eso está encapsulado. El dominio nunca importa `Microsoft.EntityFrameworkCore`.

### ¿Qué es un Domain Event y qué los diferencia de simples notificaciones?

Un Domain Event representa algo que **ya ocurrió** en el dominio (por eso se nombra en pasado: `HogarRegistrado`, no `RegistrarHogar`). Tiene un `EventId` único y `OccurredOn` para auditoría. Se acumula en el AR durante la operación y se publica **después** de `SaveChangesAsync()` para garantizar que la persistencia fue exitosa antes de notificar al exterior. El `EventBusPublisher` actual es un stub que imprime a consola, pero la arquitectura permite reemplazarlo con RabbitMQ o Kafka sin modificar el dominio ni la aplicación.

### ¿Cuál es la diferencia entre un Domain Service y un handler?

El **Domain Service** (`SvcRegistroDispositivo`, `SvcEjecucionEscena`) contiene lógica de negocio que involucra más de un agregado o que no encaja naturalmente en ninguno de ellos. El **handler** (Application) orquesta la operación: carga entidades, llama al servicio de dominio, persiste, publica eventos. El handler **no sabe** qué hace el servicio de dominio; el servicio **no sabe** que existe una base de datos ni un handler.

### ¿Por qué la ejecución de escena usa transacción explícita?

Porque `EjecutarEscenaHandler` hace múltiples escrituras en la misma operación: actualiza la `Escena` y crea `N` `ComandoDispositivo` (uno por acción). Si fallara después del segundo comando, la base de datos quedaría en estado inconsistente. La transacción garantiza atomicidad: todos persisten o ninguno.

### ¿Cómo funciona la caché?

`ConsultarEstadoHandler` implementa el patrón *cache-aside*: primero busca en `IMemoryCache` con clave `estado:dispositivo:{id}`. Si hay hit, retorna el DTO sin tocar la base de datos. Si hay miss, consulta el repositorio, almacena en caché con TTL de 30 segundos y retorna. `ICacheService` está en `Application` (no en Domain) porque la caché es una optimización técnica, no una regla de negocio.

---

## 10. Comandos para ejecutar en vivo

```bash
# Compilar toda la solución
dotnet build IOT.sln

# Ejecutar API (abre http://localhost:{puerto}/swagger)
dotnet run --project src/IoT.API

# Ejecutar tests (69 tests)
dotnet test tests/IoT.Domain.Tests

# Ejecutar consola interactiva
dotnet run --project src/IoT.ConsoleApp
```

---

## 11. Regla de dependencias (diagrama)

```
IoT.API  ──────────────────────►  IoT.Application  ──►  IoT.Domain
   │                                                          ▲
   └──────►  IoT.Infrastructure  ──────────────────────────►─┘

IoT.ConsoleApp  ──►  IoT.Application + IoT.Infrastructure + IoT.Domain
```

**La regla:** las dependencias apuntan siempre hacia adentro. `IoT.Domain` no importa nada externo. `IoT.Application` solo importa `IoT.Domain`. `IoT.Infrastructure` implementa contratos definidos en Domain/Application. `IoT.API` es la capa más externa y puede referenciar cualquier capa para registrar las implementaciones en DI.
