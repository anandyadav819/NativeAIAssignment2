# Order Service - Next Steps Guide

## What's Been Completed ✅

### All 4 Layers Implemented:
1. ✅ **Order.Domain** - Entities, Events, Repository Interface
2. ✅ **Order.Application** - Commands, Queries, Handlers, DTOs, Validators
3. ✅ **Order.Infrastructure** - DbContext, Configurations, Repository Implementation
4. ✅ **Order.API** - REST Controllers, Dependency Injection, Configuration

### Build Status:
- ✅ All projects compile without errors
- ⚠️ Minor NuGet version warnings (OpenTelemetry) - can be ignored

---

## Immediate Next Steps

### Step 1: Set Up Local Development Environment

#### Prerequisites:
```bash
# Install PostgreSQL 15+
# Install RabbitMQ
# Install Redis
```

#### Start Services:
```bash
# PostgreSQL (default port 5432)
# RabbitMQ (default ports 5672, 15672)
# Redis (default port 6379)
```

---

### Step 2: Create Database Migration

```bash
cd c:\Users\anandy\Documents\Learning\Talentica\NativeAIAssignment2-main\FoodDelivery

# Install EF Core tools if needed
dotnet tool install --global dotnet-ef

# Add migration
dotnet ef migrations add InitialCreate `
  --project src\Services\Order\Order.Infrastructure `
  --startup-project src\Services\Order\Order.API `
  --context OrderDbContext

# Apply migration to database
dotnet ef database update `
  --project src\Services\Order\Order.Infrastructure `
  --startup-project src\Services\Order\Order.API `
  --context OrderDbContext
```

---

### Step 3: Run the Order API

```bash
cd c:\Users\anandy\Documents\Learning\Talentica\NativeAIAssignment2-main\FoodDelivery\src\Services\Order\Order.API

dotnet run
```

The API will start at:
- HTTPS: `https://localhost:7001` (or check console output)
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001/swagger`

---

### Step 4: Test the API

#### Using Swagger UI:
1. Navigate to `https://localhost:7001/swagger`
2. Try the `POST /api/orders` endpoint
3. Try the `GET /api/orders/{id}` endpoint
4. Try the `GET /api/orders/customer/{customerId}` endpoint

#### Using curl:
```bash
# Create Order
curl -X POST https://localhost:7001/api/orders `
  -H "Content-Type: application/json" `
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "restaurantId": "22222222-2222-2222-2222-222222222222",
    "deliveryAddress": "123 Main St, Apt 4B",
    "items": [
      {
        "menuItemId": "33333333-3333-3333-3333-333333333333",
        "name": "Margherita Pizza",
        "quantity": 2,
        "unitPrice": 12.99
      }
    ]
  }'

# Get Order (replace {id} with actual order ID from previous response)
curl https://localhost:7001/api/orders/{id}

# Get Customer Orders
curl "https://localhost:7001/api/orders/customer/11111111-1111-1111-1111-111111111111?pageNumber=1&pageSize=10"
```

---

### Step 5: Verify Event Publishing

Check RabbitMQ Management UI:
1. Navigate to `http://localhost:15672` (default credentials: guest/guest)
2. Go to "Exchanges" tab
3. Verify `food_delivery_events` exchange exists
4. Go to "Queues" tab
5. Create test queue bound to the exchange to see published events

---

## What to Build Next

### Priority 1: Complete Order Service

#### A. Additional Commands (Order.Application/Commands):
```
✅ CreateOrderCommand - DONE
⬜ UpdateOrderStatusCommand - Update order status
⬜ CancelOrderCommand - Cancel an order
⬜ ConfirmPaymentCommand - Confirm payment
⬜ AssignDriverCommand - Assign driver to order
```

#### B. Domain Event Handlers (Order.Application/EventHandlers):
```
⬜ OrderCreatedDomainEventHandler - Additional side effects
⬜ OrderStatusChangedDomainEventHandler - Notifications, logging
⬜ OrderAssignedToDriverDomainEventHandler - Notify tracking service
```

#### C. Integration Event Handlers (Order.Application/IntegrationEventHandlers):
```
⬜ PaymentProcessedEventHandler - Handle payment results
⬜ DriverAssignedEventHandler - Handle driver assignment from tracking service
```

#### D. Order.Worker Project:
```
⬜ Background job processing (Hangfire/Quartz)
⬜ Payment processing workflows
⬜ Order timeout handling
⬜ Retry failed operations
```

#### E. Unit Tests (tests/Order.UnitTests):
```
⬜ Domain entity tests
⬜ Command handler tests
⬜ Query handler tests
⬜ Validator tests
```

---

### Priority 2: Menu Service

Following the same pattern as Order Service:

#### Menu.Domain:
- Restaurant entity (aggregate root)
- MenuItem entity
- Category entity
- Menu aggregate
- Repository interfaces

#### Menu.Application:
- Commands: CreateRestaurant, AddMenuItem, UpdateMenuItem, DeleteMenuItem
- Queries: GetRestaurants, GetMenuByRestaurant, SearchMenuItems
- DTOs, Validators, Handlers

#### Menu.Infrastructure:
- MenuDbContext (schema: "menu")
- Repository implementations
- EF Core configurations

#### Menu.API:
- RestaurantsController
- MenuController
- Caching for menu data (high read volume)

---

### Priority 3: Tracking Service

Real-time GPS tracking with SignalR:

#### Tracking.Domain:
- Driver entity
- Location value object
- DeliveryTracking aggregate

#### Tracking.Application:
- Commands: UpdateDriverLocation, AssignDriverToOrder
- Queries: GetDriverLocation, GetOrderTracking
- Real-time location updates

#### Tracking.Infrastructure:
- TrackingDbContext (schema: "tracking")
- Redis for real-time location data
- Repository implementations

#### Tracking.Hub:
- SignalR hub for real-time updates
- Client subscriptions to order tracking

#### Tracking.API:
- DriversController
- TrackingController
- WebSocket endpoint configuration

---

### Priority 4: Cross-Service Integration

#### Event Handlers:
- Menu service publishes MenuItemUpdatedEvent
- Order service validates menu items
- Tracking service consumes OrderConfirmedEvent
- Order service consumes DriverLocationUpdatedEvent

#### API Gateway (YARP):
- Route aggregation
- Request/response transformation
- Rate limiting
- Authentication/Authorization

---

## Architecture Enhancements

### Once Services Are Running:

#### 1. Monitoring:
- Set up Jaeger for distributed tracing
- Configure Serilog sinks (Elasticsearch, Seq)
- Add custom metrics with OpenTelemetry

#### 2. Resilience:
- Circuit breakers for inter-service calls
- Retry policies with exponential backoff
- Bulkhead isolation
- Timeout policies

#### 3. Security:
- Add authentication (JWT Bearer tokens)
- Add authorization policies
- API key management
- Rate limiting per client

#### 4. Performance:
- Implement caching strategies
- Add database read replicas
- Optimize queries with proper indexes
- Connection pooling configuration

---

## Testing Strategy

### Unit Tests:
- Domain entity logic
- Command/Query handlers
- Validators
- Value objects

### Integration Tests:
- API endpoint tests
- Database tests with test containers
- Event bus integration
- Cache integration

### Load Tests:
- Order creation throughput (target: 500 orders/min)
- Menu browse performance (target: P99 < 200ms)
- GPS tracking updates (target: 2000 events/sec)

---

## Configuration Files to Update

Before running in production:

### appsettings.Production.json:
```json
{
  "ConnectionStrings": {
    "FoodDeliveryDb": "Your production connection string"
  },
  "RabbitMQ": {
    "HostName": "production-rabbitmq-host",
    "UserName": "prod-user",
    "Password": "***"
  },
  "Redis": {
    "ConnectionString": "production-redis-host:6379,password=***"
  }
}
```

---

## Quick Commands Reference

### Build All:
```bash
dotnet build FoodDelivery.sln
```

### Run Order API:
```bash
cd src\Services\Order\Order.API
dotnet run
```

### Database Migrations:
```bash
# Add migration
dotnet ef migrations add MigrationName --project src\Services\Order\Order.Infrastructure --startup-project src\Services\Order\Order.API

# Apply migration
dotnet ef database update --project src\Services\Order\Order.Infrastructure --startup-project src\Services\Order\Order.API

# Remove last migration
dotnet ef migrations remove --project src\Services\Order\Order.Infrastructure --startup-project src\Services\Order\Order.API
```

### View Logs:
```bash
# Logs are in src\Services\Order\Order.API\logs\
```

---

## Documentation

### Created Documents:
1. ✅ `ARCHITECTURE.md` - Overall system architecture
2. ✅ `PROJECT_STRUCTURE.md` - Project organization and dependencies
3. ✅ `ORDER_SERVICE_SUMMARY.md` - Order service implementation details
4. ✅ `ORDER_SERVICE_NEXT_STEPS.md` - This file

### Recommended Additions:
- API documentation (OpenAPI/Swagger export)
- Database schema diagrams
- Sequence diagrams for key flows
- Deployment guide
- Operations runbook

---

## Key Design Decisions Recap

1. **Clean Architecture**: Separation of concerns, dependency inversion
2. **DDD**: Rich domain model, aggregate roots, domain events
3. **CQRS**: Separate commands and queries via MediatR
4. **Event-Driven**: RabbitMQ for async communication
5. **Schema Separation**: Shared database with different schemas
6. **Project References**: Direct references for local development
7. **Hybrid Caching**: Memory (L1) + Redis (L2)
8. **Observability**: Structured logging, distributed tracing, health checks

---

## Support & Troubleshooting

### Common Issues:

#### Build Errors:
- Ensure all NuGet packages restored: `dotnet restore`
- Check .NET 9.0 SDK installed: `dotnet --version`

#### Database Connection Issues:
- Verify PostgreSQL is running
- Check connection string in appsettings.json
- Test with: `psql -h localhost -U postgres -d fooddelivery`

#### RabbitMQ Issues:
- Verify RabbitMQ is running: http://localhost:15672
- Check credentials in appsettings.json
- Look for connection logs in console output

#### Redis Issues:
- Verify Redis is running: `redis-cli ping` (should return PONG)
- Check connection string in appsettings.json
- Service will fallback to Memory cache if Redis unavailable

---

## Success Criteria

The Order Service is considered complete when:
- ✅ All layers compile without errors
- ⬜ Database migrations apply successfully
- ⬜ API starts and Swagger UI loads
- ⬜ Can create orders via API
- ⬜ Can retrieve orders via API
- ⬜ Events published to RabbitMQ
- ⬜ Health checks return 200 OK
- ⬜ Logs written to console and file
- ⬜ Unit tests pass (when written)

---

## Contact & Questions

For questions or issues:
1. Check the documentation in docs/ folder
2. Review ARCHITECTURE.md for design decisions
3. Check PROJECT_STRUCTURE.md for project organization
4. Review this guide for next steps
