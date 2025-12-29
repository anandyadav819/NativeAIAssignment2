using Microsoft.Extensions.Logging;
using Tracking.Simulator.Models;

namespace Tracking.Simulator.Services;

/// <summary>
/// Generates realistic GPS coordinates and simulates driver movement
/// </summary>
public class GpsSimulator
{
    private readonly Random _random = new();
    private readonly ILogger<GpsSimulator> _logger;

    // San Francisco Bay Area bounds for realistic coordinates
    private const double MinLatitude = 37.7049;
    private const double MaxLatitude = 37.8349;
    private const double MinLongitude = -122.5194;
    private const double MaxLongitude = -122.3594;

    public GpsSimulator(ILogger<GpsSimulator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate a random location within the defined area
    /// </summary>
    public Location GenerateRandomLocation()
    {
        return new Location
        {
            Latitude = MinLatitude + (_random.NextDouble() * (MaxLatitude - MinLatitude)),
            Longitude = MinLongitude + (_random.NextDouble() * (MaxLongitude - MinLongitude)),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Move driver to a new location based on speed and bearing
    /// Simulates realistic movement along roads
    /// </summary>
    public Location MoveDriver(Location currentLocation, double speedKmPerHour, double bearingDegrees, double timeIntervalSeconds)
    {
        // Distance traveled in the time interval
        var distanceKm = speedKmPerHour * (timeIntervalSeconds / 3600.0);

        // Convert bearing to radians
        var bearingRadians = bearingDegrees * (Math.PI / 180.0);

        // Earth radius in kilometers
        const double earthRadiusKm = 6371.0;

        // Current position in radians
        var lat1 = currentLocation.Latitude * (Math.PI / 180.0);
        var lon1 = currentLocation.Longitude * (Math.PI / 180.0);

        // Calculate new position using haversine formula
        var lat2 = Math.Asin(
            Math.Sin(lat1) * Math.Cos(distanceKm / earthRadiusKm) +
            Math.Cos(lat1) * Math.Sin(distanceKm / earthRadiusKm) * Math.Cos(bearingRadians)
        );

        var lon2 = lon1 + Math.Atan2(
            Math.Sin(bearingRadians) * Math.Sin(distanceKm / earthRadiusKm) * Math.Cos(lat1),
            Math.Cos(distanceKm / earthRadiusKm) - Math.Sin(lat1) * Math.Sin(lat2)
        );

        // Convert back to degrees
        var newLatitude = lat2 * (180.0 / Math.PI);
        var newLongitude = lon2 * (180.0 / Math.PI);

        // Keep within bounds
        newLatitude = Math.Clamp(newLatitude, MinLatitude, MaxLatitude);
        newLongitude = Math.Clamp(newLongitude, MinLongitude, MaxLongitude);

        // Add slight randomness to simulate real GPS jitter (±5 meters)
        var jitterLat = (_random.NextDouble() - 0.5) * 0.00009; // ~5 meters
        var jitterLon = (_random.NextDouble() - 0.5) * 0.00009;

        return new Location
        {
            Latitude = newLatitude + jitterLat,
            Longitude = newLongitude + jitterLon,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generate random speed between 20-60 km/h (typical city driving)
    /// </summary>
    public double GenerateRandomSpeed()
    {
        return 20 + (_random.NextDouble() * 40);
    }

    /// <summary>
    /// Generate random bearing (0-360 degrees)
    /// </summary>
    public double GenerateRandomBearing()
    {
        return _random.NextDouble() * 360;
    }

    /// <summary>
    /// Occasionally change direction (simulates turning at intersections)
    /// </summary>
    public double AdjustBearing(double currentBearing)
    {
        // 20% chance to turn at each update
        if (_random.NextDouble() < 0.2)
        {
            // Random turn between -90 and +90 degrees
            var turnAngle = (_random.NextDouble() - 0.5) * 180;
            currentBearing = (currentBearing + turnAngle) % 360;
            if (currentBearing < 0) currentBearing += 360;
        }

        return currentBearing;
    }

    /// <summary>
    /// Occasionally adjust speed (simulates traffic)
    /// </summary>
    public double AdjustSpeed(double currentSpeed)
    {
        // 30% chance to adjust speed
        if (_random.NextDouble() < 0.3)
        {
            // Adjust speed by ±10 km/h
            var adjustment = (_random.NextDouble() - 0.5) * 20;
            currentSpeed = Math.Clamp(currentSpeed + adjustment, 10, 70);
        }

        return currentSpeed;
    }
}
