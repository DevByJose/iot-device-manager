# Refactorización de la Capa de Dominio (IoT.Domain)

Este plan aborda la solicitud de refactorización estructural y de diseño en el proyecto `IoT.Domain` para asegurar alta cohesión, mantenibilidad e inmutabilidad estricta.

## User Review Required

> [!WARNING]
> Este refactor implicará la creación de varios archivos nuevos y la eliminación de archivos que agrupaban múltiples clases. Esto puede afectar a repositorios, controladores o tests si hay dependencias implícitas en el nombre de los archivos (aunque en C# el compilador resuelve por namespace).
> Por favor, revisa y aprueba el plan antes de que proceda a realizar los cambios masivos.

## Proposed Changes

---

### Entidades (`IoT.Domain/Entities/`)

Separación de múltiples entidades contenidas en archivos de Agregados para cumplir la regla "una entidad por archivo".

#### [MODIFY] [Hogar.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/Hogar.cs)
- Se mantendrá únicamente la clase raíz `Hogar`.
#### [NEW] [Habitacion.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/Habitacion.cs)
- Se extraerá la clase `Habitacion` de `Hogar.cs`.
#### [NEW] [Dispositivo.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/Dispositivo.cs)
- Se extraerá la clase `Dispositivo` de `Hogar.cs`.

#### [MODIFY] [Escena.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/Escena.cs)
- Se mantendrá únicamente la clase raíz `Escena`.
#### [NEW] [AccionEscena.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/AccionEscena.cs)
- Se extraerá la clase `AccionEscena` de `Escena.cs`.
#### [NEW] [Disparador.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/Disparador.cs)
- Se extraerá la clase `Disparador` de `Escena.cs`.

#### [MODIFY] [EstadoDispositivo.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/EstadoDispositivo.cs)
- Se mantendrá únicamente la clase raíz `EstadoDispositivo`.
#### [NEW] [LecturaSensor.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/LecturaSensor.cs)
- Se extraerá la clase `LecturaSensor` de `EstadoDispositivo.cs`.
#### [NEW] [AlertaEstado.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Entities/AlertaEstado.cs)
- Se extraerá la clase `AlertaEstado` de `EstadoDispositivo.cs`.

---

### Servicios de Dominio (`IoT.Domain/Services/`)

Separación del archivo aglutinador `DomainServices.cs` en servicios independientes.

#### [DELETE] [DomainServices.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Services/DomainServices.cs)
- Archivo original que será descompuesto.

#### [NEW] [SvcRegistroDispositivo.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Services/SvcRegistroDispositivo.cs)
#### [NEW] [SvcEjecucionEscena.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Services/SvcEjecucionEscena.cs)
#### [NEW] [SvcValidacionComando.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Services/SvcValidacionComando.cs)
#### [NEW] [SvcConsolidacionEstado.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Services/SvcConsolidacionEstado.cs)
#### [NEW] [SvcDeteccionAnomalia.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Services/SvcDeteccionAnomalia.cs)

---

### Eventos de Dominio (`IoT.Domain/Events/`)

Extracción de todos los records/eventos desde `DomainEvents.cs`.

#### [DELETE] [DomainEvents.cs](file:///c:/Users/andre/Downloads/IOT/IOT/src/IoT.Domain/Events/DomainEvents.cs)
- Archivo original que será eliminado y reemplazado.

#### Nuevos Archivos de Eventos:
- `HogarRegistrado.cs`
- `DispositivoRegistrado.cs`
- `DispositivoDesinstalado.cs`
- `EscenaCreada.cs`
- `EscenaEjecutada.cs`
- `ComandoEnviado.cs`
- `ComandoConfirmado.cs`
- `ComandoFallido.cs`
- `EstadoCambiado.cs`
- `AnomaliaDetectada.cs`
- `DispositivoDesconectado.cs`

---

### Value Objects (`IoT.Domain/ValueObjects/`)

Se ajustarán para implementar inmutabilidad estricta. Actualmente utilizan `{ get; private set; }`. Para ser estrictamente inmutables en C#, vamos a cambiar las propiedades para que utilicen inicialización `{ get; init; }`. (Ya se encuentran en archivos separados, 1 por VO).

#### [MODIFY] Archivos en `IoT.Domain/ValueObjects/*.cs`
- Se reemplazará `private set;` por `init;` en todas las propiedades públicas.
- Se mantendrán los constructores para validación e inicialización.

---

## Verification Plan

### Automated Tests
- Compilar la solución usando `dotnet build IOT.sln` para garantizar que la refactorización (que involucra la recarga de clases desde nuevos archivos y uso de `init;` en Value Objects) no introduce errores de sintaxis o referencias rotas.

### Manual Verification
- Revisar manualmente la estructura de los directorios en `IoT.Domain` para validar que haya un solo elemento (clase/record) principal por archivo.
