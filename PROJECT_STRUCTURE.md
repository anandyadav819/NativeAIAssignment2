# Food Delivery Platform - Project Structure

## Overview

This document defines the project structure for the Food Delivery Platform backend services built with .NET Core 8. The structure supports independent parallel development while maximizing code reuse through shared building blocks.

## Design Principles

- **Project References**: Use project references for shared components (no NuGet packages initially)
- **Shared Database**: Single PostgreSQL database with schema separation per service
- **Code Reusability**: Minimize duplication through BuildingBlocks without over-engineering
- **Local Development Focus**: Docker Compose for local environment (K8s/CI-CD out of scope)
- **Clean Architecture**: Layered approach per service (API → Application → Domain → Infrastructure)

---

## Solution Structure

```
FoodDelivery/
├── FoodDelivery.sln
├── .gitignore
├── .editorconfig
├── README.md
├── ARCHITECTURE.md
├── PROJECT-STRUCTURE.md
│
├── src/
│   ├── Services/
│   │   ├── Order/
│   │   │   ├── Order.API/                    # REST API endpoints, controllers
│   │   │   ├── Order.Application/            # Commands, queries, handlers
│   │   │   ├── Order.Domain/                 # Entities, value objects, events
│   │   │   ├── Order.Infrastructure/         # Repositories, external services
│   │   │   └── Order.Worker/                 # Background job processors
│   │   │
│   │   ├── Menu/
│   │   │   ├── Menu.API/                     # REST API endpoints, controllers
│   │   │   ├── Menu.Application/             # Commands, queries, handlers
│   │   │   ├── Menu.Domain/                  # Entities, value objects
│   │   │   └── Menu.Infrastructure/          # Repositories, cache management
│   │   │
│   │   ├── Tracking/
│   │   │   ├── Tracking.API/                 # REST API endpoints
│   │   │   ├── Tracking.Application/         # Commands, queries, handlers
│   │   │   ├── Tracking.Domain/              # Entities, value objects
│   │   │   ├── Tracking.Infrastructure/      # Repositories, Redis streams
│   │   │   └── Tracking.Hub/                 # SignalR real-time hub
│   │   │
│   │   └── ApiGateway/
│   │       └── ApiGateway/                   # YARP reverse proxy configuration
│   │
│   └── BuildingBlocks/
│       ├── Common/
│       │   ├── Common.Contracts/             # Shared DTOs, interfaces, enums
│       │   ├── Common.Domain/                # Base entities, value objects, domain events
│       │   ├── Common.Application/           # CQRS base classes, MediatR behaviors
│       │   ├── Common.Database/              # Shared DbContext, migrations, UnitOfWork
│       │   ├── Common.Messaging/             # Message abstractions, event definitions
│       │   ├── Common.EventBus/              # RabbitMQ/Azure Service Bus implementation
│       │   ├── Common.Caching/               # Redis + IMemoryCache wrappers
│       │   ├── Common.Observability/         # Serilog, OpenTelemetry, health checks
│       │   └── Common.Resilience/            # Polly policies, circuit breakers, retry helpers
│       └── Testing/
│           └── Common.Testing/               # Test fixtures, builders, mock helpers
│
├── tests/
│   ├── Order.UnitTests/                      # Unit tests for Order service
│   ├── Order.IntegrationTests/               # Integration tests for Order service
│   ├── Menu.UnitTests/                       # Unit tests for Menu service
│   ├── Menu.IntegrationTests/                # Integration tests for Menu service
│   ├── Tracking.UnitTests/                   # Unit tests for Tracking service
│   └── Tracking.IntegrationTests/            # Integration tests for Tracking service
│
├── docker/
│   ├── docker-compose.yml                    # All infrastructure + services
│   ├── docker-compose.override.yml           # Local dev overrides
│   └── dockerfiles/
│       ├── Dockerfile.order                  # Order service container
│       ├── Dockerfile.menu                   # Menu service container
│       ├── Dockerfile.tracking               # Tracking service container
│       └── Dockerfile.gateway                # API Gateway container
│
├── scripts/
│   ├── setup-local.ps1                       # Windows setup script
│   ├── setup-local.sh                        # Linux/Mac setup script
│   ├── run-migrations.ps1                    # Database migrations script
│   └── build-all.ps1                         # Build all projects script
│
└── docs/
    ├── api/                                  # OpenAPI/Swagger specifications
    ├── diagrams/                             # Architecture and flow diagrams
    └── getting-started.md                    # Developer onboarding guide
```

---

## Service Dependencies

### Order Service
```
Order.API → Order.Application → Order.Domain
Order.API → Order.Infrastructure
Order.Worker → Order.Application → Order.Domain
Order.Infrastructure → Common.Database, Common.Messaging, Common.EventBus
Order.Application → Common.Application, Common.Messaging
```

### Menu Service
```
Menu.API → Menu.Application → Menu.Domain
Menu.API → Menu.Infrastructure
Menu.Infrastructure → Common.Database, Common.Caching
Menu.Application → Common.Application, Common.Caching
```

### Tracking Service
```
Tracking.API → Tracking.Application → Tracking.Domain
Tracking.Hub → Tracking.Application → Tracking.Domain
Tracking.Infrastructure → Common.Database, Common.Caching
Tracking.Application → Common.Application, Common.Messaging
```

### API Gateway
```
ApiGateway → Common.Observability, Common.Resilience
```

---

## BuildingBlocks Details

### Common.Contracts

**Purpose**: Shared DTOs, interfaces, and enums across services

**Contents**:
- Request/Response DTOs
- Shared enums (OrderStatus, RestaurantStatus, DriverStatus)
- Service-to-service contract interfaces
- Pagination models

**Dependencies**: None (pure POCOs)

---

### Common.Domain

**Purpose**: Base classes for domain-driven design

**Contents**:
- `Entity<T>` base class
- `AggregateRoot<T>` base class
- `ValueObject` base class
- `IDomainEvent` interface
- `DomainException` base exception

**Dependencies**: None

**Example**:
```csharp
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
}

public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

---

### Common.Application

**Purpose**: Shared application layer patterns (CQRS, validation, mapping)

**Contents**:
- `ICommand`, `IQuery<TResponse>` marker interfaces
- `ICommandHandler<TCommand>`, `IQueryHandler<TQuery, TResponse>`
- MediatR pipeline behaviors (logging, validation, performance)
- FluentValidation base validators
- AutoMapper profiles

**Dependencies**: `MediatR`, `FluentValidation`, `AutoMapper`

**Example**:
```csharp
public interface ICommand : IRequest { }
public interface ICommand<TResponse> : IRequest<TResponse> { }

public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Validation logic
    }
}
```

---

### Common.Database

**Purpose**: Shared database context, entity configurations, migrations

**Contents**:
- `FoodDeliveryDbContext` (single DbContext for all services)
- Entity configurations (IEntityTypeConfiguration<T>)
- UnitOfWork pattern implementation
- Database connection factory
- Migration scripts

**Database Schema Organization**:
```sql
-- Schema separation per service
CREATE SCHEMA order;
CREATE SCHEMA menu;
CREATE SCHEMA tracking;

-- Tables
order.orders
order.order_items
menu.restaurants
menu.menu_items
tracking.drivers
tracking.driver_locations
```

**Dependencies**: `Npgsql.EntityFrameworkCore.PostgreSQL`

**Example**:
```csharp
public class FoodDeliveryDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Order schema
        modelBuilder.HasDefaultSchema("order");
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        
        // Menu schema
        modelBuilder.Entity<Restaurant>().ToTable("restaurants", "menu");
        modelBuilder.ApplyConfiguration(new RestaurantConfiguration());
        
        // Tracking schema
        modelBuilder.Entity<Driver>().ToTable("drivers", "tracking");
        modelBuilder.ApplyConfiguration(new DriverConfiguration());
    }
}
```

---

### Common.Messaging

**Purpose**: Message abstractions and event definitions

**Contents**:
- `IEvent` interface
- `IIntegrationEvent` interface (cross-service)
- Event base classes
- Message metadata (correlation ID, timestamp)

**Dependencies**: None

**Example**:
```csharp
public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public interface IIntegrationEvent : IEvent
{
    string EventType { get; }
}

// Example event
public class OrderCreatedEvent : IIntegrationEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }
    public string EventType => nameof(OrderCreatedEvent);
    
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
}
```

---

### Common.EventBus

**Purpose**: Message broker abstraction and implementation

**Contents**:
- `IEventBus` interface
- RabbitMQ implementation (`RabbitMQEventBus`)
- Event subscription manager
- Retry and dead-letter queue handling

**Dependencies**: `RabbitMQ.Client`, `Common.Messaging`

**Example**:
```csharp
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IIntegrationEvent;
    
    void Subscribe<TEvent, THandler>() 
        where TEvent : IIntegrationEvent
        where THandler : IEventHandler<TEvent>;
}

public interface IEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
```

---

### Common.Caching

**Purpose**: Multi-level caching abstraction

**Contents**:
- `ICacheService` interface
- L1 cache (IMemoryCache) implementation
- L2 cache (Redis) implementation
- Hybrid cache strategy
- Cache key generation helpers

**Dependencies**: `StackExchange.Redis`, `Microsoft.Extensions.Caching.Memory`

**Example**:
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default);
    
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, 
        TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

public class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;  // L1
    private readonly IConnectionMultiplexer _redis;  // L2
    
    // Implementation with L1 → L2 → Source fallback
}
```

---

### Common.Observability

**Purpose**: Logging, metrics, tracing, and health checks

**Contents**:
- Serilog configuration
- OpenTelemetry setup
- Prometheus metrics registration
- Health check extensions
- Correlation ID middleware

**Dependencies**: `Serilog`, `OpenTelemetry`, `Prometheus.Client`

**Example**:
```csharp
public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("Service", Assembly.GetEntryAssembly()?.GetName().Name)
            .WriteTo.Console()
            .WriteTo.Seq(configuration["Seq:ServerUrl"])
            .CreateLogger();
        
        // OpenTelemetry
        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddRedisInstrumentation());
        
        // Health checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database"))
            .AddRedis(configuration.GetConnectionString("Redis"));
        
        return services;
    }
}
```

---

### Common.Resilience

**Purpose**: Retry policies, circuit breakers, timeout policies

**Contents**:
- Pre-configured Polly policies
- HTTP resilience policies
- Database resilience policies
- Circuit breaker configuration
- Bulkhead isolation

**Dependencies**: `Polly`, `Polly.Extensions.Http`

**Example**:
```csharp
public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Log.Warning("Retry {RetryCount} after {Delay}s", 
                        retryCount, timespan.TotalSeconds);
                });
    }
    
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
```

---

### Common.Testing

**Purpose**: Shared test utilities and fixtures

**Contents**:
- `WebApplicationFactory<T>` fixtures
- Database test fixtures (with Testcontainers)
- Builder pattern test data generators
- Mock helpers for external services
- Integration test base classes

**Dependencies**: `xUnit`, `Testcontainers`, `Moq`, `Bogus`

**Example**:
```csharp
public class FoodDeliveryWebApplicationFactory<TProgram> 
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("fooddelivery_test")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real DbContext with test container
            var descriptor = services.SingleOrDefault(d => 
                d.ServiceType == typeof(DbContextOptions<FoodDeliveryDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);
            
            services.AddDbContext<FoodDeliveryDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }
}
```

---

## Database Strategy

### Schema Organization

Each service has its own schema within the shared PostgreSQL database:

```sql
-- Order Service Schema
CREATE SCHEMA order;

CREATE TABLE order.orders (
    id UUID PRIMARY KEY,
    customer_id UUID NOT NULL,
    restaurant_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    total_amount DECIMAL(10,2) NOT NULL,
    items JSONB NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP,
    version INT NOT NULL
);

CREATE TABLE order.order_items (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL REFERENCES order.orders(id),
    menu_item_id UUID NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL
);

-- Menu Service Schema
CREATE SCHEMA menu;

CREATE TABLE menu.restaurants (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    address VARCHAR(500) NOT NULL,
    status VARCHAR(50) NOT NULL,
    rating DECIMAL(3,2),
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE menu.menu_items (
    id UUID PRIMARY KEY,
    restaurant_id UUID NOT NULL REFERENCES menu.restaurants(id),
    name VARCHAR(200) NOT NULL,
    description TEXT,
    price DECIMAL(10,2) NOT NULL,
    category VARCHAR(100) NOT NULL,
    available BOOLEAN NOT NULL DEFAULT true
);

-- Tracking Service Schema
CREATE SCHEMA tracking;

CREATE TABLE tracking.drivers (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE tracking.driver_locations (
    id UUID PRIMARY KEY,
    driver_id UUID NOT NULL REFERENCES tracking.drivers(id),
    latitude DECIMAL(9,6) NOT NULL,
    longitude DECIMAL(9,6) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL
);

-- Spatial index for location queries
CREATE INDEX idx_driver_locations_spatial 
ON tracking.driver_locations USING GIST (
    ll_to_earth(latitude, longitude)
);
```

### Migration Strategy

- **Single Migration Project**: All migrations in `Common.Database`
- **Schema Prefixes**: Migration names include service prefix (e.g., `20241220_Order_CreateOrdersTable`)
- **Forward Only**: Avoid down migrations in production
- **Seeding**: Seed data scripts in `Common.Database/Seeds/`

### Future Migration Path

When to split to per-service databases:
1. Team size grows beyond 10 developers
2. Services need independent deployment cycles
3. Different data storage requirements (e.g., NoSQL for tracking)
4. Regulatory/compliance requirements for data isolation

---

## Testing Strategy

### Unit Tests

**Location**: `tests/{Service}.UnitTests/`

**Scope**:
- Domain logic (entities, value objects)
- Application handlers (commands, queries)
- Business rule validation

**Tools**: xUnit, Moq, FluentAssertions

**Example**:
```csharp
public class OrderTests
{
    [Fact]
    public void AddItem_WhenValidItem_ShouldAddToOrder()
    {
        // Arrange
        var order = new Order(customerId: Guid.NewGuid(), 
            restaurantId: Guid.NewGuid());
        var item = new OrderItem(menuItemId: Guid.NewGuid(), 
            quantity: 2, unitPrice: 10.99m);
        
        // Act
        order.AddItem(item);
        
        // Assert
        order.Items.Should().ContainSingle();
        order.TotalAmount.Should().Be(21.98m);
    }
}
```

---

### Integration Tests

**Location**: `tests/{Service}.IntegrationTests/`

**Scope**:
- API endpoint testing
- Database integration
- Message bus integration
- Cache integration

**Tools**: xUnit, Testcontainers, WebApplicationFactory

**Example**:
```csharp
public class OrderApiTests : IClassFixture<FoodDeliveryWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public OrderApiTests(FoodDeliveryWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateOrder_WhenValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            Items = new[] { /* ... */ }
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Status.Should().Be("Pending");
    }
}
```

---

## Project Dependencies Matrix

| Project | References |
|---------|-----------|
| **Order.API** | Order.Application, Order.Infrastructure, Common.Observability, Common.Resilience |
| **Order.Application** | Order.Domain, Common.Application, Common.Messaging, Common.Caching |
| **Order.Domain** | Common.Domain |
| **Order.Infrastructure** | Order.Domain, Common.Database, Common.EventBus, Common.Resilience |
| **Order.Worker** | Order.Application, Common.EventBus, Common.Observability |
| **Menu.API** | Menu.Application, Menu.Infrastructure, Common.Observability |
| **Menu.Application** | Menu.Domain, Common.Application, Common.Caching |
| **Menu.Domain** | Common.Domain |
| **Menu.Infrastructure** | Menu.Domain, Common.Database, Common.Caching |
| **Tracking.API** | Tracking.Application, Tracking.Infrastructure, Common.Observability |
| **Tracking.Application** | Tracking.Domain, Common.Application, Common.Messaging |
| **Tracking.Domain** | Common.Domain |
| **Tracking.Infrastructure** | Tracking.Domain, Common.Database, Common.Caching |
| **Tracking.Hub** | Tracking.Application, Common.Caching, Common.Observability |
| **ApiGateway** | Common.Observability, Common.Resilience |

---

## Key Design Decisions Summary

### 1. Shared Database with Schema Separation
- ✅ **Pros**: Simpler setup, ACID transactions, easier queries across services
- ⚠️ **Cons**: Tight coupling, harder to scale independently
- **Mitigation**: Use schemas to logically separate, design for future split

### 2. Project References vs NuGet Packages
- ✅ **Pros**: Faster development, easier debugging, single version
- ⚠️ **Cons**: Tighter coupling, rebuild required for changes
- **Mitigation**: Keep BuildingBlocks stable, well-tested

### 3. Single DbContext
- ✅ **Pros**: Simplified migrations, UnitOfWork across services
- ⚠️ **Cons**: Services share entity knowledge
- **Mitigation**: Use schema separation, services only reference their entities

### 4. Minimal Code Duplication
- ✅ **Pros**: DRY principle, consistent patterns, faster development
- ⚠️ **Cons**: Potential for over-abstraction
- **Mitigation**: Extract only truly shared code, avoid premature abstraction

---

## Next Steps

1. ✅ Define architecture (COMPLETED)
2. ✅ Define project structure (COMPLETED)
3. ⬜ Create solution and project scaffolding
4. ⬜ Implement BuildingBlocks (Common.*)
5. ⬜ Implement Order Service (highest priority)
6. ⬜ Implement Menu Service
7. ⬜ Implement Tracking Service
8. ⬜ Implement API Gateway
9. ⬜ Add integration tests
10. ⬜ Docker configuration
11. ⬜ Load testing and optimization

---

**Document Version**: 1.0  
**Last Updated**: December 20, 2025  
**Author**: GitHub Copilot  
**Status**: Ready for Implementation