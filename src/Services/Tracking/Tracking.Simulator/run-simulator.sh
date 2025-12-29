#!/bin/bash
# Driver Location Simulator - Quick Start Script

echo ""
echo "==================================================================="
echo "  Driver Location Simulator - Quick Start"
echo "==================================================================="
echo ""

cd "$(dirname "$0")"

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Please install .NET 9.0 SDK"
    exit 1
fi

echo "Building the simulator..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "ERROR: Build failed"
    exit 1
fi

echo ""
echo "==================================================================="
echo "  Quick Test Scenarios"
echo "==================================================================="
echo ""
echo "  1. Light Load    - 10 drivers, 10 updates/sec, 30 seconds"
echo "  2. Medium Load   - 25 drivers, 10 updates/sec, 60 seconds"
echo "  3. Heavy Load    - 50 drivers, 10 updates/sec, 60 seconds"
echo "  4. Stress Test   - 50 drivers, 20 updates/sec, 60 seconds"
echo "  5. Custom        - Specify your own parameters"
echo "  6. Exit"
echo ""

read -p "Select scenario (1-6): " choice

case $choice in
    1)
        echo ""
        echo "Running Light Load Test..."
        dotnet run --configuration Release -- --drivers 10 --rate 10 --duration 30
        ;;
    2)
        echo ""
        echo "Running Medium Load Test..."
        dotnet run --configuration Release -- --drivers 25 --rate 10 --duration 60
        ;;
    3)
        echo ""
        echo "Running Heavy Load Test..."
        dotnet run --configuration Release -- --drivers 50 --rate 10 --duration 60
        ;;
    4)
        echo ""
        echo "Running Stress Test..."
        dotnet run --configuration Release -- --drivers 50 --rate 20 --duration 60
        ;;
    5)
        read -p "Number of drivers: " drivers
        read -p "Updates per second per driver: " rate
        read -p "Duration in seconds: " duration
        echo ""
        echo "Running Custom Test..."
        dotnet run --configuration Release -- --drivers $drivers --rate $rate --duration $duration
        ;;
    6)
        exit 0
        ;;
    *)
        echo "Invalid choice. Exiting."
        exit 1
        ;;
esac

echo ""
echo ""
echo "==================================================================="
echo "  Test Complete"
echo "==================================================================="
