---
name: ddd-csharp
description: >
  Guía de Domain-Driven Design (DDD) para proyectos C# / .NET. Úsala siempre que el usuario
  mencione DDD, modelado de dominio, agregados, entidades, value objects, repositorios,
  domain events, bounded contexts, o cuando quiera estructurar o revisar la capa de dominio
  de su aplicación. También aplica cuando el usuario pregunta "¿cómo modelo esto?",
  "¿dónde va esta lógica?", o "¿cómo separo el dominio?".
---

# Domain-Driven Design en C# / .NET

## Principio central
La lógica de negocio vive **exclusivamente** en el dominio. Infraestructura, UI y aplicación son detalles.

---

## Bloques de construcción (Building Blocks)

### 1. Entity
- Tiene **identidad única** (`Id`) que persiste en el tiempo.
- La igualdad se basa en el `Id`, no en sus atributos.
- Contiene comportamiento, no solo datos.

```csharp
public class Order : Entity<OrderId>
{
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    // Constructor privado — se crea solo desde el dominio
    private Order(OrderId id, CustomerId customerId) : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Draft;
    }

    public static Order Create(CustomerId customerId)
    {
        var id = OrderId.New();
        var order = new Order(id, customerId);
        order.AddDomainEvent(new OrderCreatedEvent(id, customerId));
        return order;
    }

    public void AddLine(ProductId productId, int quantity, Money price)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Solo se pueden agregar líneas a órdenes en borrador.");

        _lines.Add(new OrderLine(productId, quantity, price));
    }
}
```

---

### 2. Value Object
- **Sin identidad** — igualdad por valor de sus atributos.
- **Inmutable** — nunca se modifica, se reemplaza.
- Encapsula validación y reglas de formato.

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new DomainException("El monto no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("La moneda es requerida.");
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency) throw new DomainException("No se pueden sumar monedas distintas.");
        return new Money(Amount + other.Amount, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

**Base recomendada:**
```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, HashCode.Combine);
}
```

---

### 3. Aggregate & Aggregate Root
- Un **Aggregate** es un clúster de entidades y value objects tratados como una unidad.
- El **Aggregate Root** es la única entidad accesible desde fuera.
- Todas las modificaciones pasan por el Root.
- La consistencia se garantiza **dentro** del aggregate.

```
OrderAggregate
├── Order (Root)          ← único punto de entrada
├── OrderLine (Entity)    ← solo accesible desde Order
└── Money (Value Object)
```

**Regla de oro:** Los repositorios solo guardan y recuperan Aggregate Roots.

---

### 4. Domain Events
- Representan algo que **ocurrió** en el dominio (pasado).
- Permiten comunicación entre aggregates sin acoplamiento directo.
- Se disparan dentro del aggregate, se publican en la capa de aplicación.

```csharp
public record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId) : IDomainEvent;

// Base sugerida
public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _events.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event) => _events.Add(@event);
    public void ClearDomainEvents() => _events.Clear();

    protected Entity(TId id) => Id = id;
}
```

---

### 5. Repository (interfaz en el dominio)
- La **interfaz** vive en el dominio — el dominio define el contrato.
- La **implementación** vive en infraestructura (EF Core, Dapper, etc.).

```csharp
// Dominio
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}

// Infraestructura
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    public OrderRepository(AppDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct)
        => await _context.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct);
}
```

---

### 6. Domain Service
- Lógica de dominio que **no pertenece** a una sola entidad o aggregate.
- Orquesta múltiples aggregates o realiza cálculos cross-dominio.
- Nunca accede a infraestructura directamente.

```csharp
public class OrderPricingService
{
    public Money CalculateTotal(Order order, IEnumerable<Discount> discounts)
    {
        var subtotal = order.Lines.Aggregate(
            new Money(0, "COP"),
            (acc, line) => acc.Add(line.Subtotal));

        foreach (var discount in discounts)
            subtotal = discount.Apply(subtotal);

        return subtotal;
    }
}
```

---

## Bounded Context

Cada Bounded Context es un **límite explícito** donde el modelo de dominio es consistente.

```
┌─────────────────────────┐    ┌──────────────────────────┐
│   Orders Context         │    │   Inventory Context       │
│  - Order, OrderLine      │    │  - Product, Stock         │
│  - Customer (referencia) │    │  - Warehouse              │
└────────────┬────────────┘    └────────────┬─────────────┘
             │   Domain Event / Integration Event            │
             └─────────────────────────────────────────────┘
```

- Cada contexto tiene su **propio modelo** — un `Product` en Orders no es el mismo que en Inventory.
- La comunicación entre contextos es vía **Integration Events** (no Domain Events).

---

## Estructura de carpetas recomendada

```
src/
├── Domain/
│   ├── Orders/
│   │   ├── Order.cs
│   │   ├── OrderLine.cs
│   │   ├── OrderId.cs
│   │   ├── OrderStatus.cs
│   │   ├── Events/
│   │   │   └── OrderCreatedEvent.cs
│   │   └── Repositories/
│   │       └── IOrderRepository.cs
│   ├── Customers/
│   │   └── ...
│   └── Shared/
│       ├── Entity.cs
│       ├── ValueObject.cs
│       └── IDomainEvent.cs
├── Application/
├── Infrastructure/
└── API/
```

---

## Anti-patrones comunes a evitar

| Anti-patrón | Problema | Corrección |
|---|---|---|
| Anemic Domain Model | Entidades solo con getters/setters, lógica en servicios | Mover comportamiento a las entidades |
| Repository genérico (`IRepository<T>`) | Expone operaciones innecesarias, rompe encapsulación | Interfaces específicas por aggregate |
| Acceder a DB desde el dominio | Acopla dominio a infraestructura | Usar interfaces de repositorio |
| Aggregate demasiado grande | Problemas de concurrencia y rendimiento | Dividir por invariantes de negocio |
| Levantar eventos desde la capa de aplicación | Pierdes consistencia del dominio | Siempre desde dentro del aggregate |

---

## Lecturas de referencia
- *Domain-Driven Design* — Eric Evans (libro azul)
- *Implementing Domain-Driven Design* — Vaughn Vernon (libro rojo)
- *Domain Modeling Made Functional* — Scott Wlaschin