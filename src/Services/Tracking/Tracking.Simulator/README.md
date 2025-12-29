# Driver Location Simulator

## Overview
A high-performance data simulator for testing the Tracking Service. Generates realistic GPS location updates for multiple drivers to demonstrate functionality and load testing capabilities.

## Features

- ✅ **Realistic GPS Simulation**: Generates coordinates in San Francisco Bay Area
- ✅ **Movement Patterns**: Simulates realistic driver movement with speed and bearing
- ✅ **GPS Jitter**: Adds realistic GPS inaccuracy (±5 meters)
- ✅ **Traffic Simulation**: Random speed adjustments and direction changes
- ✅ **Configurable Load**: Support for up to 50 drivers with 10 events/second
- ✅ **Real-time Statistics**: Progress updates every 5 seconds
- ✅ **Error Handling**: Tracks successful and failed API calls

## Quick Start

### 1. Build the Simulator
```bash
cd src/Services/Tracking/Tracking.Simulator
dotnet build
```

### 2. Start Tracking API
Make sure the Tracking API is running on port 5002 (or configure in appsettings.json)

### 3. Run with Defaults (50 drivers, 10 events/sec, 60 seconds)
```bash
dotnet run
```

### 4. Run with Custom Parameters
```bash
dotnet run --drivers 30 --rate 10 --duration 120
```

## Configuration

### Command Line Arguments

| Argument | Description | Default |
|----------|-------------|---------|
| `--drivers` | Number of drivers to simulate | 50 |
| `--rate` | Location updates per second per driver | 10 |
| `--duration` | Simulation duration in seconds | 60 |

### appsettings.json

```json
{
  "TrackingAPI": {
    "BaseUrl": "http://localhost:5002"
  },
  "Simulation": {
    "DriverCount": 50,
    "UpdatesPerSecond": 10,
    "DurationSeconds": 60
  }
}
```

## Usage Examples

### Test with 10 Drivers (Low Load)
```bash
dotnet run --drivers 10 --rate 10 --duration 30
# Total load: 100 events/sec
```

### Test with 25 Drivers (Medium Load)
```bash
dotnet run --drivers 25 --rate 10 --duration 60
# Total load: 250 events/sec
```

### Test with 50 Drivers (High Load)
```bash
dotnet run --drivers 50 --rate 10 --duration 120
# Total load: 500 events/sec
```

### Stress Test (Max Load)
```bash
dotnet run --drivers 50 --rate 20 --duration 60
# Total load: 1,000 events/sec
```

## Output

### Startup Banner
```
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║        🚗 DRIVER LOCATION SIMULATOR 🗺️                    ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

📋 Configuration:
   👥 Drivers: 50
   📡 Updates per second: 10
   ⏱️  Duration: 60 seconds
   🎯 Target load: 500 total events/sec
```

### Progress Updates (Every 5 seconds)
```
📊 Progress Update:
   ⏱️  Elapsed: 00:15
   📤 Total Updates: 7,500
   ✅ Successful: 7,450
   ❌ Failed: 50
   📈 Rate: 500.00 updates/sec
   💯 Success Rate: 99.3%
```

### Final Statistics
```
════════════════════════════════════════════════════════════
🏁 SIMULATION COMPLETE
════════════════════════════════════════════════════════════
⏱️  Total Duration: 01:00.123
👥 Drivers: 50
📤 Total Updates Sent: 30,000
✅ Successful Updates: 29,850
❌ Failed Updates: 150
📈 Average Rate: 499.75 updates/sec
💯 Success Rate: 99.5%
⚡ Peak Throughput: 24,987 potential events/sec
════════════════════════════════════════════════════════════
```

## How It Works

### 1. GPS Simulation
- Generates coordinates within San Francisco Bay Area bounds
- Uses haversine formula for realistic movement calculation
- Adds GPS jitter (±5 meters) to simulate real-world GPS inaccuracy

### 2. Movement Patterns
- Random initial speed: 20-60 km/h (typical city driving)
- Random initial bearing: 0-360 degrees
- 20% chance to turn at each update (simulates intersections)
- 30% chance to adjust speed (simulates traffic)

### 3. API Integration
- Sends POST requests to `/api/drivers/{driverId}/location`
- Tracks success/failure rates
- Handles timeouts and errors gracefully
- Maintains target update rate

### 4. Performance Monitoring
- Real-time throughput calculation
- Success rate tracking
- Progress updates every 5 seconds
- Comprehensive final statistics

## Architecture

```
Tracking.Simulator/
├── Program.cs                          # Entry point and configuration
├── appsettings.json                    # Configuration file
├── Models/
│   └── Driver.cs                       # Driver and Location models
└── Services/
    ├── GpsSimulator.cs                 # GPS coordinate generation
    └── DriverSimulationService.cs      # Main simulation logic
```

## Performance Targets

| Scenario | Drivers | Rate/Driver | Total Load | Expected Result |
|----------|---------|-------------|------------|-----------------|
| Light | 10 | 10/sec | 100/sec | 100% success |
| Medium | 25 | 10/sec | 250/sec | >99% success |
| Heavy | 50 | 10/sec | 500/sec | >98% success |
| Stress | 50 | 20/sec | 1000/sec | >95% success |

## Troubleshooting

### "Connection refused" Error
- Ensure Tracking API is running
- Check the BaseUrl in appsettings.json
- Verify firewall settings

### Low Success Rate
- Check Tracking API logs for errors
- Verify database connectivity
- Check server resource utilization
- Reduce load (fewer drivers or lower rate)

### High Response Times
- Check database performance
- Monitor server CPU/memory
- Consider scaling infrastructure
- Review API logging overhead

## Integration with Tracking Service

The simulator sends location updates that:
1. Update driver GPS coordinates in the database
2. Create location history entries
3. Trigger SignalR real-time notifications (if configured)
4. Generate domain events for tracking

## Next Steps

1. **Monitor API Performance**: Use the statistics to identify bottlenecks
2. **Database Optimization**: Check query performance under load
3. **SignalR Testing**: Connect clients to receive real-time updates
4. **Horizontal Scaling**: Test with multiple API instances
5. **Load Balancing**: Add load balancer for production scenarios

## Stop Simulation

Press `Ctrl+C` at any time to gracefully stop the simulation and view final statistics.
