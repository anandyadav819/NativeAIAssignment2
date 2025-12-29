# ============================================
# Food Delivery Platform - View Logs Script
# ============================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Service Logs                          " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

param(
    [string]$Service = "all"
)

$validServices = @("all", "order-api", "menu-api", "tracking-api", "postgres", "redis", "rabbitmq")

if ($validServices -notcontains $Service) {
    Write-Host "Invalid service name: $Service" -ForegroundColor Red
    Write-Host "Valid services: $($validServices -join ', ')" -ForegroundColor Yellow
    exit 1
}

if ($Service -eq "all") {
    Write-Host "Showing logs for all services (press Ctrl+C to exit)..." -ForegroundColor Yellow
    Write-Host ""
    docker-compose logs -f
} else {
    Write-Host "Showing logs for $Service (press Ctrl+C to exit)..." -ForegroundColor Yellow
    Write-Host ""
    docker-compose logs -f $Service
}
