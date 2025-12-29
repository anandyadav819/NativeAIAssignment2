# Food Delivery Platform

A scalable, event-driven microservices-based food delivery platform built with .NET 9.0, featuring real-time order tracking, high-performance menu browsing, and robust order processing capabilities.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

This food delivery platform is designed to handle high-scale operations with the following capabilities:

- **Order Processing**: 500+ orders per minute with resilient payment processing
- **Menu Browsing**: Sub-200ms P99 response times with Redis caching
- **Real-Time Tracking**: 10,000 concurrent drivers with GPS updates every 5 seconds
- **Event-Driven Architecture**: Asynchronous communication using RabbitMQ
- **Microservices Design**: Three independent services (Order, Menu, Tracking)

## 🏗️ Architecture

The platform consists of three main microservices:

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Applications                       │
│            (Web, Mobile, Admin Dashboard)                    │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                     API Gateway (Future)                     │
│              (YARP Reverse Proxy + Auth)                     │
└───┬────────────────────┬────────────────────┬───────────────┘
    │                    │                    │
    ▼                    ▼                    ▼
┌──────────┐      ┌──────────┐      ┌────────────────┐
│  Order   │      │   Menu   │      │    Tracking    │
│ Service  │      │ Service  │      │    Service     │
│          │      │          │      │                │
│ Port:    │      │ Port:    │      │ Port: 5173     │
│ 5075     │      │ 5284     │      │ SignalR Hub    │
└────┬─────┘      └────┬─────┘      └────┬───────────┘
     │                 │                  │
     │                 │                  │
     ├─────────────────┴──────────────────┤
     │                                    │
┌────▼─────────────────────────────────────▼──────┐
│           Shared Infrastructure                  │
│  ┌──────────────┐  ┌──────────┐  ┌───────────┐ │
│  │ PostgreSQL   │  │  Redis   │  │ RabbitMQ  │ │
│  │ Database     │  │  Cache   │  │ Message   │ │
│  │ Port: 5432   │  │Port: 6379│  │Bus:5672   │ │
│  └──────────────┘  └──────────┘  └───────────┘ │
└──────────────────────────────────────────────────┘
```

### Service Descriptions

**Order Service** (`Order.API`)
- Order creation and management
- Payment processing with mock gateway
- Background workers for async operations
- Event publishing for order state changes
- Runs on: `http://localhost:5075`

**Menu Service** (`Menu.API`)
- Restaurant and menu item management
- High-performance caching with Redis
- Restaurant search and filtering
- Opening hours management
- Runs on: `http://localhost:5284`

**Tracking Service** (`Tracking.API`)
- Real-time GPS tracking with SignalR
- Driver location updates
- Delivery tracking
- WebSocket connections for live updates
- Runs on: `http://localhost:5173`

## ✨ Key Features

### Order Management
- ✅ Create and manage orders with multiple items
- ✅ Async payment processing with retry logic
- ✅ Order status tracking (Pending → Processing → Confirmed → Preparing → Ready → PickedUp → Delivered)
- ✅ Order cancellation with payment refunds
- ✅ Driver assignment

### Menu & Restaurant Management
- ✅ Restaurant CRUD operations
- ✅ Menu item management
- ✅ Restaurant activation/suspension
- ✅ Opening hours configuration
- ✅ Redis-based caching for fast retrieval
- ✅ Real-time availability updates

### Real-Time Tracking
- ✅ Live driver location updates via SignalR
- ✅ GPS coordinate tracking with haversine distance calculation
- ✅ Delivery status updates (pickup, in-transit, delivered)
- ✅ Available driver queries
- ✅ Order tracking by customer

### Cross-Cutting Concerns
- ✅ CQRS pattern with MediatR
- ✅ Domain-Driven Design (DDD)
- ✅ Event sourcing with domain events
- ✅ FluentValidation for input validation
- ✅ Comprehensive logging with Serilog
- ✅ Resilience patterns (Retry, Circuit Breaker)
- ✅ Repository pattern with Unit of Work
- ✅ Comprehensive unit tests (190+ tests)

## 🛠️ Technology Stack

### Backend
- **.NET 9.0** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core 9.0** - ORM
- **MediatR 12.4.1** - CQRS implementation
- **FluentValidation 11.10.0** - Input validation
- **SignalR** - Real-time communication

### Data Stores
- **PostgreSQL 15+** - Primary database
- **Redis 7+** - Caching and session management
- **RabbitMQ 3.12+** - Message broker

### Testing
- **xUnit 2.9.2** - Test framework
- **Moq 4.20.72** - Mocking framework
- **FluentAssertions 6.12.1** - Assertion library

### DevOps & Tools
- **Docker & Docker Compose** - Containerization
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation

## 📦 Prerequisites

Before you begin, ensure you have the following installed on your system:

### Required Software
- **[.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)** or later
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (for running infrastructure)
- **[Git](https://git-scm.com/downloads)** for version control

### Optional (for manual infrastructure)
- **[PostgreSQL 15+](https://www.postgresql.org/download/)**
- **[Redis 7+](https://redis.io/download)**
- **[RabbitMQ 3.12+](https://www.rabbitmq.com/download.html)**

### Verify Installation

```powershell
# Check .NET version
dotnet --version
# Should show: 9.0.x or higher

# Check Docker
docker --version
# Should show: Docker version 20.x or higher

docker-compose --version
# Should show: Docker Compose version 2.x or higher
```

## 🚀 Getting Started

You can run the Food Delivery Platform in two ways:
1. **🐳 Docker Compose (Recommended)** - Run everything in containers
2. **💻 Local Development** - Run services locally with .NET

### Option A: Docker Compose (Recommended) 🐳

This is the quickest way to get the entire platform running, including all microservices and infrastructure.

#### Step 1: Clone the Repository

```powershell
git clone <repository-url>
cd NativeAIAssignment2-main\FoodDelivery
```

#### Step 2: Start the Platform with Docker Compose

**Quick Start (Automated Script):**

```powershell
# This script will build images and start all services
.\docker-start.ps1
```

The script will:
- ✅ Check if Docker is running
- ✅ Clean up any existing containers
- ✅ Build Docker images for all services
- ✅ Start infrastructure services (PostgreSQL, Redis, RabbitMQ)
- ✅ Wait for health checks to pass
- ✅ Start all microservices (Order, Menu, Tracking)
- ✅ Display service URLs

**Manual Start:**

```powershell
# Build and start all services
docker-compose up --build -d

# Or start specific services only
docker-compose up -d postgres redis rabbitmq  # Infrastructure only
docker-compose up -d                          # All services
```

#### Step 3: Verify Services are Running

```powershell
# Check container status
docker-compose ps

# View logs for all services
docker-compose logs -f

# View logs for a specific service
docker-compose logs -f order-api
docker-compose logs -f menu-api
docker-compose logs -f tracking-api
```

Expected output:
```
NAME                          STATUS    PORTS
fooddelivery-postgres         Up        0.0.0.0:5432->5432/tcp
fooddelivery-redis            Up        0.0.0.0:6379->6379/tcp
fooddelivery-rabbitmq         Up        0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
fooddelivery-order-api        Up        0.0.0.0:5075->80/tcp
fooddelivery-menu-api         Up        0.0.0.0:5284->80/tcp
fooddelivery-tracking-api     Up        0.0.0.0:5173->80/tcp
```

#### Step 4: Access the Services

Once all services are running, access them at:

| Service | URL | Description |
|---------|-----|-------------|
| **Order API** | http://localhost:5075/swagger | Order management and payment processing |
| **Menu API** | http://localhost:5284/swagger | Restaurant and menu management |
| **Tracking API** | http://localhost:5173/swagger | Real-time delivery tracking |
| **RabbitMQ Management** | http://localhost:15672 | Message broker UI (guest/guest) |

#### Stop Services

```powershell
# Stop all services
docker-compose stop

# Stop and remove containers (keeps volumes/data)
docker-compose down

# Stop and remove everything including volumes (clean slate)
docker-compose down -v

# Or use the helper script
.\docker-stop.ps1
```

#### Docker Compose Architecture

The platform includes:

**Infrastructure Services:**
- **PostgreSQL 15** - Shared database for all services
- **Redis 7** - Caching and session management
- **RabbitMQ 3.12** - Message broker for event-driven communication

**Microservices:**
- **Order API** (Port 5075) - Order processing with background workers
- **Menu API** (Port 5284) - Restaurant catalog with Redis caching
- **Tracking API** (Port 5173) - Real-time GPS tracking with SignalR

All services are connected via a custom bridge network (`fooddelivery-network`) and configured with health checks for reliable startup.

---

### Option B: Local Development 💻

For active development with hot reload and debugging capabilities.

#### Step 1: Clone the Repository

```powershell
git clone <repository-url>
cd NativeAIAssignment2-main\FoodDelivery
```

#### Step 2: Start Infrastructure Services Only

The application requires PostgreSQL, Redis, and RabbitMQ. Start only the infrastructure using Docker Compose:

```powershell
# Start only infrastructure services
docker-compose up -d postgres redis rabbitmq

# Verify they're running
docker-compose ps
```

#### Start the Infrastructure

```powershell
# Start all infrastructure services
docker-compose up -d

# Check if all services are running
docker-compose ps

# View logs (optional)
docker-compose logs -f
```

```

#### Start the Infrastructure

```powershell
# Start all infrastructure services
docker-compose up -d

# Check if all services are running
docker-compose ps

# View logs (optional)
docker-compose logs -f
```

Expected output:
```
NAME                      STATUS    PORTS
fooddelivery-postgres     Up        0.0.0.0:5432->5432/tcp
fooddelivery-redis        Up        0.0.0.0:6379->6379/tcp
fooddelivery-rabbitmq     Up        0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
```

#### Step 3: Restore NuGet Packages

```powershell
# From the FoodDelivery directory
dotnet restore FoodDelivery.sln
```

#### Step 4: Run Database Migrations

Each service has its own database schema. Run migrations for all services:

```powershell
# Order Service Migrations
cd src\Services\Order\Order.Infrastructure
dotnet ef database update --startup-project ../Order.API --context OrderDbContext

# Menu Service Migrations
cd ..\..\Menu\Menu.Infrastructure
dotnet ef database update --startup-project ../Menu.API --context MenuDbContext

# Tracking Service Migrations
cd ..\..\Tracking\Tracking.Infrastructure
dotnet ef database update --startup-project ../Tracking.API --context TrackingDbContext

# Return to root
cd ..\..\..\..
```

Expected output for each migration:
```
Build started...
Build succeeded.
Applying migration '20241228_InitialCreate'...
Done.
```

#### Step 5: Build the Solution

```powershell
# From the FoodDelivery directory
dotnet build FoodDelivery.sln --configuration Release
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

#### Step 6: Run the Services

Open **3 separate PowerShell terminals** and run each service:

**Terminal 1 - Order Service**
```powershell
cd src\Services\Order\Order.API
dotnet run
```
Service will start on: `http://localhost:5075`

**Terminal 2 - Menu Service**
```powershell
cd src\Services\Menu\Menu.API
dotnet run
```
Service will start on: `http://localhost:5284`

**Terminal 3 - Tracking Service**
```powershell
cd src\Services\Tracking\Tracking.API
dotnet run
```
Service will start on: `http://localhost:5173`

**Or use the helper script** to run all services at once:

```powershell
# Create run-all-services.ps1 with the following content:
Write-Host "Starting Food Delivery Platform..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src\Services\Order\Order.API; dotnet run"
Start-Sleep -Seconds 3
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src\Services\Menu\Menu.API; dotnet run"
Start-Sleep -Seconds 3
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src\Services\Tracking\Tracking.API; dotnet run"
Write-Host "Order Service:    http://localhost:5075/swagger" -ForegroundColor Cyan
Write-Host "Menu Service:     http://localhost:5284/swagger" -ForegroundColor Cyan
Write-Host "Tracking Service: http://localhost:5173/swagger" -ForegroundColor Cyan

# Run the script
.\run-all-services.ps1
```

#### Step 7: Verify Services are Running

Check the health endpoints:

```powershell
# Order Service
curl http://localhost:5075/health

# Menu Service
curl http://localhost:5284/health

# Tracking Service
curl http://localhost:5173/health
```

All should return: `Healthy`

---

## 🔍 Accessing the Platform

Once running (via Docker Compose or locally), access the services at:

| Service | Swagger UI | Health Check |
|---------|------------|--------------|
| **Order Service** | http://localhost:5075/swagger | http://localhost:5075/health |
| **Menu Service** | http://localhost:5284/swagger | http://localhost:5284/health |
| **Tracking Service** | http://localhost:5173/swagger | http://localhost:5173/health |

**Infrastructure Management:**
- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)
- **PostgreSQL**: localhost:5432 (postgres/23rc8efwmed932d@$c83)
- **Redis**: localhost:6379

---

## 📊 Testing

The platform includes comprehensive unit tests for all services.

### Run All Tests

```powershell
# From the FoodDelivery directory
dotnet test --no-build --verbosity normal
```

### Run Tests for Individual Services

```powershell
# Order Service Tests (64 tests)
dotnet test tests\Order.UnitTests\Order.UnitTests.csproj

# Menu Service Tests (57 tests)
dotnet test tests\Menu.UnitTests\Menu.UnitTests.csproj

# Tracking Service Tests (69 tests)
dotnet test tests\Tracking.UnitTests\Tracking.UnitTests.csproj
```

### Test Coverage Summary

| Service  | Test Count | Coverage Areas |
|----------|-----------|----------------|
| Order    | 64 tests  | Domain, Application, Infrastructure, Validators |
| Menu     | 57 tests  | Domain, Application, Validators |
| Tracking | 69 tests  | Domain, Application, Query Handlers |
| **Total** | **190 tests** | **100% Pass Rate** |

### Run Tests with Coverage

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## 📚 API Documentation

Each service exposes Swagger UI for API exploration:

- **Order Service**: [http://localhost:5075/swagger](http://localhost:5075/swagger)
- **Menu Service**: [http://localhost:5284/swagger](http://localhost:5284/swagger)
- **Tracking Service**: [http://localhost:5173/swagger](http://localhost:5173/swagger)

### Quick API Examples

#### Create a Restaurant (Menu Service)
```bash
POST http://localhost:5284/api/restaurants
Content-Type: application/json

{
  "name": "Pizza Palace",
  "address": "123 Main St",
  "phoneNumber": "+1234567890",
  "openingTime": "09:00:00",
  "closingTime": "22:00:00"
}
```

#### Create an Order (Order Service)
```bash
POST http://localhost:5075/api/orders
Content-Type: application/json

{
  "restaurantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "deliveryAddress": "456 Elm St",
  "items": [
    {
      "menuItemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2,
      "price": 12.99
    }
  ]
}
```

#### Update Driver Location (Tracking Service)
```bash
POST http://localhost:5173/api/drivers/location
Content-Type: application/json

{
  "driverId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "latitude": 40.7128,
  "longitude": -74.0060
}
```

## 📁 Project Structure

```
FoodDelivery/
├── src/
│   ├── Services/
│   │   ├── Order/              # Order management service
│   │   │   ├── Order.API
│   │   │   ├── Order.Application
│   │   │   ├── Order.Domain
│   │   │   ├── Order.Infrastructure
│   │   │   └── Order.Worker
│   │   ├── Menu/               # Menu and restaurant service
│   │   │   ├── Menu.API
│   │   │   ├── Menu.Application
│   │   │   ├── Menu.Domain
│   │   │   └── Menu.Infrastructure
│   │   └── Tracking/           # Real-time tracking service
│   │       ├── Tracking.API
│   │       ├── Tracking.Application
│   │       ├── Tracking.Domain
│   │       ├── Tracking.Infrastructure
│   │       └── Tracking.Hub
│   └── BuildingBlocks/
│       └── Common/             # Shared libraries
│           ├── Common.Application
│           ├── Common.Caching
│           ├── Common.Contracts
│           ├── Common.Database
│           ├── Common.Domain
│           ├── Common.EventBus
│           ├── Common.Messaging
│           ├── Common.Observability
│           └── Common.Resilience
├── tests/
│   ├── Order.UnitTests/        # 64 tests
│   ├── Menu.UnitTests/         # 57 tests
│   └── Tracking.UnitTests/     # 69 tests
└── docs/                       # Documentation
```

## 🗄️ Database Schema

The platform uses a single PostgreSQL database with schema separation:

### Schemas
- `orders` - Order service tables
- `menu` - Menu service tables  
- `tracking` - Tracking service tables

### Key Tables

**Orders Schema:**
- `orders.orders` - Order entities
- `orders.order_items` - Order line items
- `orders.payments` - Payment records

**Menu Schema:**
- `menu.restaurants` - Restaurant entities
- `menu.menu_items` - Menu item entities

**Tracking Schema:**
- `tracking.drivers` - Driver entities
- `tracking.delivery_trackings` - Delivery tracking records

### Connection String
```
Host=localhost;Port=5432;Database=fooddelivery;Username=postgres;Password=23rc8efwmed932d@$c83
```

## 🔧 Troubleshooting

### Common Issues and Solutions

#### 1. Docker Issues

**Error**: `Docker is not running`

**Solution**: Start Docker Desktop and wait for it to fully initialize

```powershell
# Check Docker status
docker info

# If still not working, restart Docker Desktop
```

**Error**: `Cannot connect to Docker daemon`

**Solution**: 
- Ensure Docker Desktop is running
- Check if Docker service is running in Services (services.msc)
- Restart Docker Desktop

#### 2. Port Already in Use

**Error**: `Address already in use: bind` or `port is already allocated`

**Solution**: 
```powershell
# Option 1: Find and kill process using the port
netstat -ano | findstr :5075   # Replace 5075 with your port
taskkill /PID <PID> /F          # Replace <PID> with actual process ID

# Option 2: Stop Docker containers
docker-compose down

# Option 3: Check what's using Docker ports
docker ps -a
```

#### 3. Database Connection Failed

**Error**: `Connection refused` or `No connection could be made`

**Solutions**:
```powershell
# Check if PostgreSQL is running
docker-compose ps postgres

# Check PostgreSQL logs
docker-compose logs postgres

# Restart PostgreSQL
docker-compose restart postgres

# Wait for health check
docker-compose ps  # Wait until postgres shows "healthy"

# Test connection manually
docker exec -it fooddelivery-postgres psql -U postgres -c "SELECT version();"
```

#### 4. Redis Connection Failed

**Error**: `It was not possible to connect to the redis server(s)`

**Solutions**:
```powershell
# Check if Redis is running
docker-compose ps redis

# Check Redis logs
docker-compose logs redis

# Restart Redis
docker-compose restart redis

# Test Redis connection
docker exec -it fooddelivery-redis redis-cli ping
# Should return: PONG
```

#### 5. RabbitMQ Connection Failed

**Error**: `None of the specified endpoints were reachable`

**Solutions**:
```powershell
# Check if RabbitMQ is running
docker-compose ps rabbitmq

# Check RabbitMQ logs
docker-compose logs rabbitmq

# Restart RabbitMQ
docker-compose restart rabbitmq

# Access RabbitMQ Management UI
# http://localhost:15672 (guest/guest)

# Check RabbitMQ status
docker exec -it fooddelivery-rabbitmq rabbitmq-diagnostics status
```

#### 6. Migration Errors

**Error**: `Npgsql.PostgresException: database "fooddelivery" does not exist`

**Solution**:
```powershell
# Option 1: Let EF Core create the database
cd src\Services\Order\Order.Infrastructure
dotnet ef database update --startup-project ../Order.API --context OrderDbContext

# Option 2: Create database manually
docker exec -it fooddelivery-postgres psql -U postgres -c "CREATE DATABASE fooddelivery;"

# Option 3: Restart with clean slate
docker-compose down -v
docker-compose up -d postgres redis rabbitmq
# Wait for services to be healthy, then run migrations
```

#### 7. Docker Build Failures

**Error**: `failed to solve: failed to compute cache key`

**Solutions**:
```powershell
# Clean Docker cache
docker system prune -a --volumes

# Rebuild without cache
docker-compose build --no-cache

# Or use the startup script
.\docker-start.ps1
```

#### 8. Service Not Responding

**Error**: `Health check failed` or service container exits immediately

**Solutions**:
```powershell
# Check container logs for specific service
docker-compose logs order-api
docker-compose logs menu-api
docker-compose logs tracking-api

# Check if dependencies are healthy
docker-compose ps

# Restart specific service
docker-compose restart order-api

# Restart all services
docker-compose restart

# Full reset
docker-compose down
docker-compose up -d
```

#### 9. Cannot Access Swagger UI

**Error**: `Connection refused` when accessing http://localhost:5075/swagger

**Solutions**:
```powershell
# Check if service is running
docker-compose ps

# Check service logs
docker-compose logs order-api

# Verify port mapping
docker port fooddelivery-order-api

# Try accessing health endpoint first
curl http://localhost:5075/health

# Check if service started successfully
docker-compose logs order-api | Select-String "Now listening"
```

#### 10. Volume/Permission Issues

**Error**: `Permission denied` or volume mounting errors

**Solutions**:
```powershell
# On Windows with WSL2:
# Ensure Docker Desktop has access to C: drive
# Settings > Resources > File Sharing

# Remove volumes and recreate
docker-compose down -v
docker volume prune -f
docker-compose up -d
```

#### 6. Build Errors

**Error**: `The type or namespace name 'X' could not be found`

**Solution**:
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Logs Location

- **Application Logs**: `src/Services/{ServiceName}/{ServiceName}.API/logs/`
- **Docker Logs**: `docker-compose logs [service-name]`

### Reset Everything

If you need to start fresh:

```powershell
# Stop all services
docker-compose down -v

# Remove all data volumes
docker volume prune -f

# Clean solution
dotnet clean

# Start fresh
docker-compose up -d
dotnet restore
dotnet build
```

## 🛑 Stopping the Application

### Stop All Docker Services
```powershell
# Stop containers
docker-compose down

# Stop and remove volumes (⚠️ deletes all data)
docker-compose down -v
```

### Stop .NET Services
Simply press `Ctrl+C` in each terminal running a service.

## 📖 Additional Resources

- **Architecture Documentation**: `ARCHITECTURE.md`
- **Project Structure**: `PROJECT_STRUCTURE.md`
- **Order Service Details**: `ORDER_SERVICE_SUMMARY.md`
- **Payment Gateway**: `PAYMENT_GATEWAY_IMPLEMENTATION.md`
- **Tracking Service**: `TRACKING_SERVICE_SUMMARY.md`

## 🤝 Contributing

1. Follow the existing code structure and patterns
2. Write unit tests for new features
3. Update documentation as needed
4. Follow C# coding conventions
5. Use meaningful commit messages

## 📝 License

[Add your license information here]

## 👥 Authors

[Add author information here]

---

**Built with ❤️ using .NET 9.0**
