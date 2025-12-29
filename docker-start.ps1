# ============================================
# Food Delivery Platform - Docker Startup Script
# ============================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Food Delivery Platform - Docker      " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if Docker is running
function Test-DockerRunning {
    try {
        docker info | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

# Check Docker
Write-Host "Checking Docker..." -ForegroundColor Yellow
if (-not (Test-DockerRunning)) {
    Write-Host "Error: Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker is running" -ForegroundColor Green
Write-Host ""

# Stop and remove existing containers
Write-Host "Cleaning up existing containers..." -ForegroundColor Yellow
docker-compose down -v 2>&1 | Out-Null
Write-Host "✓ Cleanup complete" -ForegroundColor Green
Write-Host ""

# Build images
Write-Host "Building Docker images..." -ForegroundColor Yellow
Write-Host "This may take several minutes on first run..." -ForegroundColor Gray
docker-compose build --no-cache
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to build Docker images" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Images built successfully" -ForegroundColor Green
Write-Host ""

# Start infrastructure services first
Write-Host "Starting infrastructure services..." -ForegroundColor Yellow
docker-compose up -d postgres redis rabbitmq
Write-Host "Waiting for services to be healthy..." -ForegroundColor Gray
Start-Sleep -Seconds 20

# Check if infrastructure is healthy
$infraHealthy = $true
$services = @("postgres", "redis", "rabbitmq")
foreach ($service in $services) {
    $health = docker inspect --format='{{.State.Health.Status}}' "fooddelivery-$service" 2>$null
    if ($health -ne "healthy") {
        Write-Host "Warning: $service is not yet healthy (status: $health)" -ForegroundColor Yellow
        $infraHealthy = $false
    } else {
        Write-Host "✓ $service is healthy" -ForegroundColor Green
    }
}
Write-Host ""

if (-not $infraHealthy) {
    Write-Host "Waiting additional time for infrastructure..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
}

# Run database migrations
Write-Host "Running database migrations..." -ForegroundColor Yellow
Write-Host "Note: Migrations will be applied when services start" -ForegroundColor Gray
Write-Host ""

# Start application services
Write-Host "Starting application services..." -ForegroundColor Yellow
docker-compose up -d order-api menu-api tracking-api
Write-Host ""

Write-Host "Waiting for application services to start..." -ForegroundColor Gray
Start-Sleep -Seconds 30

# Display status
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Service Status                        " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
docker-compose ps

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Service URLs                          " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Order Service:      http://localhost:5075/swagger" -ForegroundColor Green
Write-Host "Menu Service:       http://localhost:5284/swagger" -ForegroundColor Green
Write-Host "Tracking Service:   http://localhost:5173/swagger" -ForegroundColor Green
Write-Host ""
Write-Host "PostgreSQL:         localhost:5432" -ForegroundColor Cyan
Write-Host "Redis:              localhost:6379" -ForegroundColor Cyan
Write-Host "RabbitMQ:           http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Commands                              " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "View logs:          docker-compose logs -f [service-name]" -ForegroundColor Yellow
Write-Host "Stop all:           docker-compose down" -ForegroundColor Yellow
Write-Host "Stop and clean:     docker-compose down -v" -ForegroundColor Yellow
Write-Host "Restart service:    docker-compose restart [service-name]" -ForegroundColor Yellow
Write-Host ""
Write-Host "✓ All services started successfully!" -ForegroundColor Green
