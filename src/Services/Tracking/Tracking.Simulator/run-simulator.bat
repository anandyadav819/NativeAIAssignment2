@echo off
REM Driver Location Simulator - Quick Start Script

echo.
echo ===================================================================
echo   Driver Location Simulator - Quick Start
echo ===================================================================
echo.

cd /d "%~dp0"

REM Check if dotnet is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found. Please install .NET 9.0 SDK
    pause
    exit /b 1
)

echo Building the simulator...
dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo ===================================================================
echo   Quick Test Scenarios
echo ===================================================================
echo.
echo   1. Light Load    - 10 drivers, 10 updates/sec, 30 seconds
echo   2. Medium Load   - 25 drivers, 10 updates/sec, 60 seconds  
echo   3. Heavy Load    - 50 drivers, 10 updates/sec, 60 seconds
echo   4. Stress Test   - 50 drivers, 20 updates/sec, 60 seconds
echo   5. Custom        - Specify your own parameters
echo   6. Exit
echo.

set /p choice="Select scenario (1-6): "

if "%choice%"=="1" (
    echo.
    echo Running Light Load Test...
    dotnet run --configuration Release -- --drivers 10 --rate 10 --duration 30
) else if "%choice%"=="2" (
    echo.
    echo Running Medium Load Test...
    dotnet run --configuration Release -- --drivers 25 --rate 10 --duration 60
) else if "%choice%"=="3" (
    echo.
    echo Running Heavy Load Test...
    dotnet run --configuration Release -- --drivers 50 --rate 10 --duration 60
) else if "%choice%"=="4" (
    echo.
    echo Running Stress Test...
    dotnet run --configuration Release -- --drivers 50 --rate 20 --duration 60
) else if "%choice%"=="5" (
    set /p drivers="Number of drivers: "
    set /p rate="Updates per second per driver: "
    set /p duration="Duration in seconds: "
    echo.
    echo Running Custom Test...
    dotnet run --configuration Release -- --drivers %drivers% --rate %rate% --duration %duration%
) else if "%choice%"=="6" (
    exit /b 0
) else (
    echo Invalid choice. Exiting.
    pause
    exit /b 1
)

echo.
echo.
echo ===================================================================
echo   Test Complete
echo ===================================================================
pause
