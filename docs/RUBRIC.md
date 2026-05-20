# Aspectos de implementación (25 puntos)

De acuerdo con la(s) arquitectura(s) elegida(s).

## 1. Hay una Capa de Dominio con sus preocupaciones y lógica de negocio claramente implementadas (10 puntos)

### Entidades
- Enriquecidas, ricas e implementan comportamientos.

### Value Objects inmutables
- Solo tienen setters y getters.
- Reciben los datos en constructor con lanzamiento de excepciones.

### Agregados con consistencia
- Se identifica claramente la raíz y su contenido.
- La raíz es el único punto de acceso.

### Domain Events
- Se invocan desde agregados (subscribers).
- Representan algo que sucedió en el dominio.
- Se nombran en pasado.

### Interfaces de Dominio
- Definidas en el Dominio e implementadas en otras capas.

---

## 2. Hay una capa de Aplicación u orquestadora (5 puntos)

### Use cases específicos
- Hay use case y cada uno realiza una única acción de negocio.

### Orquestación
- Se implementa coordinación entre servicios de dominio.
- No contiene lógica de negocio.

### Transacciones bien manejadas
- Garantiza consistencia en la operación completa.

---

## 3. Hay una Capa dedicada a manejar temas de Infraestructura (5 puntos)

### Repositorios
- Implementan interfaces de dominio.
- No hacen referencias a tecnologías específicas: SQL, APIs, NoSQL.
- Retornan entidades y agregados del dominio.

### ORM
- Se tiene implementado ORM para la conversión y transformación de tablas a objetos del dominio.
- Se encarga de las consultas y la persistencia.
- Solo se usa en la capa de infraestructura; el dominio no depende de los ORM.

### Implementación de Caché
- Se tienen implementados mecanismos de caché.

---

## 4. Hay una Capa más externa que maneja las interacciones con el resto del ecosistema digital (5 puntos)

### Controllers delgados
- Implementan los casos de uso.
- Manejan consistencia (commits, rollbacks).

### DTOs para entrada/salida
- Implementados para transportar datos entre capas.
- Mapeo explícito hacia/desde el modelo de dominio.
- Validación de datos de entrada en el punto de recepción (Boundaries).