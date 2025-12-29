# ============================================
# Food Delivery Platform - Docker Stop Script
# ============================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Stopping Food Delivery Platform      " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

param(
    [switch]$RemoveVolumes = $false
)

if ($RemoveVolumes) {
    Write-Host "Stopping all services and removing volumes..." -ForegroundColor Yellow
    docker-compose down -v
    Write-Host "✓ Services stopped and volumes removed" -ForegroundColor Green
} else {
    Write-Host "Stopping all services (keeping data)..." -ForegroundColor Yellow
    docker-compose down
    Write-Host "✓ Services stopped (data preserved)" -ForegroundColor Green
}

Write-Host ""
Write-Host "To remove volumes (delete all data), run:" -ForegroundColor Yellow
Write-Host "  .\docker-stop.ps1 -RemoveVolumes" -ForegroundColor Gray
