# Docker Deployment Guide

This directory contains Docker configurations for the Food Delivery Platform.

## 📦 What's Included

- `docker-compose.yml` - Main orchestration file
- `docker-compose.override.yml` - Development overrides
- `Dockerfile.order` - Order Service container
- `Dockerfile.menu` - Menu Service container
- `Dockerfile.tracking` - Tracking Service container

## 🚀 Quick Start

### Prerequisites
- Docker Desktop 20.x or higher
- Docker Compose 2.x or higher
- At least 4GB RAM allocated to Docker

### Start All Services

```powershell
# Using the convenience script (recommended)
.\docker-start.ps1

# Or manually
docker-compose up -d
```

### Stop All Services

```powershell
# Keep data
.\docker-stop.ps1

# Remove data volumes
.\docker-stop.ps1 -RemoveVolumes

# Or manually
docker-compose down
docker-compose down -v  # with volumes
```

### View Logs

```powershell
# All services
.\docker-logs.ps1

# Specific service
.\docker-logs.ps1 -Service order-api
.\docker-logs.ps1 -Service menu-api
.\docker-logs.ps1 -Service tracking-api

# Or manually
docker-compose logs -f
docker-compose logs -f order-api
```

## 🔍 Service Details

### Infrastructure Services

| Service | Port(s) | Description |
|---------|---------|-------------|
| PostgreSQL | 5432 | Primary database |
| Redis | 6379 | Cache and session store |
| RabbitMQ | 5672, 15672 | Message broker (15672 = Management UI) |

### Application Services

| Service | Internal Port | External Port | Description |
|---------|---------------|---------------|-------------|
| Order API | 80 | 5075 | Order management and payments |
| Menu API | 80 | 5284 | Restaurant and menu management |
| Tracking API | 80 | 5173 | Real-time GPS tracking |

## 📊 Health Checks

All services include health checks that monitor:
- Infrastructure: Database connectivity, Redis availability
- Applications: HTTP endpoints responding

Check service health:
```powershell
docker-compose ps
docker inspect --format='{{.State.Health.Status}}' fooddelivery-order-api
```

## 🗄️ Data Persistence

Data is persisted in Docker volumes:
- `postgres-data` - Database files
- `redis-data` - Cache persistence
- `rabbitmq-data` - Message queue data

### View Volumes
```powershell
docker volume ls | findstr fooddelivery
```

### Backup Database
```powershell
docker exec fooddelivery-postgres pg_dump -U postgres fooddelivery > backup.sql
```

### Restore Database
```powershell
cat backup.sql | docker exec -i fooddelivery-postgres psql -U postgres fooddelivery
```

## 🔧 Development Workflow

### Rebuild After Code Changes

```powershell
# Rebuild specific service
docker-compose build order-api
docker-compose up -d order-api

# Rebuild all services
docker-compose build
docker-compose up -d
```

### Access Service Containers

```powershell
# Execute command in container
docker exec -it fooddelivery-order-api bash

# View environment variables
docker exec fooddelivery-order-api printenv

# Check disk usage
docker exec fooddelivery-order-api df -h
```

### Database Migrations

Migrations run automatically when services start. To run manually:

```powershell
# Connect to database
docker exec -it fooddelivery-postgres psql -U postgres -d fooddelivery

# List schemas
\dn

# List tables
\dt orders.*
\dt menu.*
\dt tracking.*
```

## 🐛 Troubleshooting

### Service Won't Start

```powershell
# Check logs
docker-compose logs order-api

# Check if port is in use
netstat -ano | findstr :5075

# Restart service
docker-compose restart order-api
```

### Database Connection Issues

```powershell
# Check PostgreSQL is healthy
docker inspect --format='{{.State.Health.Status}}' fooddelivery-postgres

# Test connection
docker exec -it fooddelivery-postgres pg_isready -U postgres

# View PostgreSQL logs
docker-compose logs postgres
```

### Out of Memory

```powershell
# Check resource usage
docker stats

# Increase Docker memory limit in Docker Desktop settings
# Settings → Resources → Memory
```

### Clean Start

```powershell
# Stop everything
docker-compose down -v

# Remove images
docker-compose down --rmi all -v

# Rebuild from scratch
docker-compose build --no-cache
docker-compose up -d
```

## 🌐 Accessing Services

### Application APIs
- Order Service: http://localhost:5075/swagger
- Menu Service: http://localhost:5284/swagger
- Tracking Service: http://localhost:5173/swagger

### Infrastructure UIs
- RabbitMQ Management: http://localhost:15672 (guest/guest)

### Health Endpoints
- Order: http://localhost:5075/health
- Menu: http://localhost:5284/health
- Tracking: http://localhost:5173/health

## 📝 Environment Variables

Environment variables can be customized in:
1. `docker-compose.yml` - Base configuration
2. `docker-compose.override.yml` - Development overrides
3. `.env` file (create if needed)

Example `.env` file:
```env
POSTGRES_PASSWORD=your-secure-password
ASPNETCORE_ENVIRONMENT=Development
```

## 🔒 Security Notes

**⚠️ Important for Production:**
- Change default passwords in `docker-compose.yml`
- Use Docker secrets for sensitive data
- Enable TLS/SSL for services
- Configure firewall rules
- Use private Docker registry
- Implement proper authentication

## 📈 Performance Tuning

### PostgreSQL
```yaml
environment:
  - POSTGRES_SHARED_BUFFERS=256MB
  - POSTGRES_MAX_CONNECTIONS=200
```

### Redis
```yaml
command: redis-server --maxmemory 256mb --maxmemory-policy allkeys-lru
```

### Application
```yaml
environment:
  - DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
  - DOTNET_GCHeapCount=2
```

## 🔄 CI/CD Integration

### Build Images
```powershell
docker-compose build --parallel
```

### Tag and Push
```powershell
docker tag fooddelivery-order-api registry.example.com/order-api:latest
docker push registry.example.com/order-api:latest
```

### Pull and Deploy
```powershell
docker-compose pull
docker-compose up -d --force-recreate
```

## 📚 Additional Resources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [Redis Docker Hub](https://hub.docker.com/_/redis)
- [RabbitMQ Docker Hub](https://hub.docker.com/_/rabbitmq)
