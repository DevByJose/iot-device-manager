---
name: clean-architecture-csharp
description: >
  Guía de Clean Architecture para proyectos C# / .NET. Úsala siempre que el usuario mencione
  Clean Architecture, arquitectura en capas, separación de responsabilidades, dependencias entre
  proyectos, CQRS, Use Cases, Application Services, o cuando quiera estructurar o revisar cómo
  organizar su solución .NET. También aplica cuando el usuario pregunta "¿en qué capa va esto?",
  "¿cómo estructuro mi proyecto?", "¿cómo evito el acoplamiento?", o "¿dónde pongo esta clase?".
---

# Clean Architecture en C# / .NET

## Principio central
Las dependencias siempre apuntan **hacia adentro**. El dominio no conoce nada de infraestructura, frameworks, ni bases de datos.

```
        ┌──────────────────────────────┐
        │          API / UI             │  ← depende de Application
        ├──────────────────────────────┤
        │        Application            │  ← depende de Domain
        ├──────────────────────────────┤
        │          Domain               │  ← no depende de nadie
        └──────────────────────────────┘
        │       Infrastructure          │  ← depende de Domain y Application
        └──────────────────────────────┘
```

La **Infrastructure** implementa las interfaces definidas en Domain/Application y se inyecta vía DI.

---

## Capas y responsabilidades

### Domain (núcleo)
- Entidades, Value Objects, Aggregates, Domain Events.
- Interfaces de repositorios y servicios de dominio.
- **Cero dependencias externas** — sin NuGet, sin EF Core, sin System.Net.
- Es el código más estable; cambia solo cuando cambia el negocio.

```
Domain/
├── Entities/
├── ValueObjects/
├── Events/
├── Repositories/        ← interfaces, no implementaciones
├── Services/            ← domain services
└── Exceptions/
```

---

### Application (casos de uso)
- Orquesta el dominio para cumplir un caso de uso específico.
- Contiene Commands, Queries, Handlers (si usas CQRS).
- Define interfaces para servicios externos (email, storage, etc.) que Infrastructure implementa.
- Depende solo de `Domain`.

```
Application/
├── Orders/
│   ├── Commands/
│   │   ├── CreateOrder/
│   │   │   ├── CreateOrderCommand.cs
│   │   │   └── CreateOrderCommandHandler.cs
│   │   └── AddOrderLine/
│   │       ├── AddOrderLineCommand.cs
│   │       └── AddOrderLineCommandHandler.cs
│   └── Queries/
│       └── GetOrderById/
│           ├── GetOrderByIdQuery.cs
│           ├── GetOrderByIdQueryHandler.cs
│           └── OrderDto.cs
├── Common/
│   ├── Interfaces/
│   │   ├── IEmailService.cs
│   │   └── IFileStorage.cs
│   └── Behaviors/
│       ├── ValidationBehavior.cs
│       └── LoggingBehavior.cs
└── DependencyInjection.cs
```

---

### Infrastructure
- Implementa todas las interfaces definidas en Domain y Application.
- EF Core, Dapper, HTTP clients, servicios de email, almacenamiento, etc.
- Depende de `Domain` y `Application`.

```
Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs
│   ├── Repositories/
│   │   └── OrderRepository.cs
│   ├── Configurations/       ← IEntityTypeConfiguration
│   │   └── OrderConfiguration.cs
│   └── Migrations/
├── Services/
│   ├── EmailService.cs
│   └── FileStorageService.cs
└── DependencyInjection.cs
```

---

### API / Presentation
- Controllers, Minimal API endpoints, gRPC, etc.
- Transforma requests HTTP en Commands/Queries y llama a Application.
- Depende de `Application` (nunca de `Infrastructure` directamente, excepto para DI en Program.cs).

```
API/
├── Controllers/
│   └── OrdersController.cs
├── Contracts/
│   ├── Requests/
│   └── Responses/
├── Middleware/
└── Program.cs             ← aquí se registra todo
```

---

## CQRS con MediatR

### Command (escribe, modifica estado)

```csharp
// Application/Orders/Commands/CreateOrder/CreateOrderCommand.cs
public record CreateOrderCommand(Guid CustomerId) : IRequest<Guid>;

// Application/Orders/Commands/CreateOrder/CreateOrderCommandHandler.cs
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public CreateOrderCommandHandler(IOrderRepository orders, IUnitOfWork uow)
    {
        _orders = orders;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(new CustomerId(request.CustomerId));
        await _orders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);
        return order.Id.Value;
    }
}
```

### Query (solo lee, no modifica)

```csharp
// Application/Orders/Queries/GetOrderById/GetOrderByIdQuery.cs
public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

// Application/Orders/Queries/GetOrderById/GetOrderByIdQueryHandler.cs
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderReadRepository _read;

    public GetOrderByIdQueryHandler(IOrderReadRepository read) => _read = read;

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken ct)
        => await _read.GetOrderDtoByIdAsync(new OrderId(request.OrderId), ct);
}
```

### Controller delgado

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var orderId = await _sender.Send(new CreateOrderCommand(request.CustomerId));
        return CreatedAtAction(nameof(GetById), new { id = orderId }, null);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(id));
        return order is null ? NotFound() : Ok(order);
    }
}
```

---

## Configuración de proyectos (.csproj)

```
MyApp.sln
├── src/
│   ├── MyApp.Domain/            ← sin referencias a otros proyectos
│   ├── MyApp.Application/       ← referencia a Domain
│   ├── MyApp.Infrastructure/    ← referencia a Domain + Application
│   └── MyApp.API/               ← referencia a Application + Infrastructure (solo DI)
└── tests/
    ├── MyApp.Domain.Tests/
    ├── MyApp.Application.Tests/
    └── MyApp.Integration.Tests/
```

**Regla de dependencias en csproj:**
```xml
<!-- Application.csproj -->
<ItemGroup>
  <ProjectReference Include="..\MyApp.Domain\MyApp.Domain.csproj" />
</ItemGroup>

<!-- Infrastructure.csproj -->
<ItemGroup>
  <ProjectReference Include="..\MyApp.Domain\MyApp.Domain.csproj" />
  <ProjectReference Include="..\MyApp.Application\MyApp.Application.csproj" />
</ItemGroup>
```

---

## Validación con FluentValidation

```csharp
// Application/Orders/Commands/CreateOrder/CreateOrderCommandValidator.cs
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("El cliente es requerido.");
    }
}

// Application/Common/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var failures = _validators
            .SelectMany(v => v.Validate(request).Errors)
            .Where(e => e != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

---

## Registro de dependencias por capa

```csharp
// Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}

// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("Default")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        return services;
    }
}

// API/Program.cs
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
```

---

## Checklist: ¿Estoy respetando la arquitectura?

- [ ] ¿Domain tiene referencias a NuGet de infraestructura (EF, Http, etc.)? → **Mover a Infrastructure**
- [ ] ¿Application llama directamente a DbContext? → **Usar interfaz de repositorio**
- [ ] ¿El Controller tiene lógica de negocio? → **Mover a un Command/Query Handler**
- [ ] ¿Infrastructure conoce detalles de la API? → **Invertir la dependencia**
- [ ] ¿Los handlers hacen más de un caso de uso? → **Dividir en handlers separados**
- [ ] ¿Los DTOs tienen lógica? → **Son solo contenedores de datos**

---

## Anti-patrones comunes

| Anti-patrón | Señal | Corrección |
|---|---|---|
| Fat Controller | El controller tiene más de 20 líneas de lógica | Mover a Command/Query Handler |
| DbContext en Application | `using MyApp.Infrastructure` en Application | Crear interfaz `IAppDbContext` en Application |
| Lógica en Infrastructure | Reglas de negocio en `OrderRepository` | Mover al Domain o Application |
| DTOs con comportamiento | Métodos de negocio en `OrderDto` | DTOs son solo datos; lógica al dominio |
| Circular dependencies | A → B → A | Revisar si una capa se saltó el modelo |