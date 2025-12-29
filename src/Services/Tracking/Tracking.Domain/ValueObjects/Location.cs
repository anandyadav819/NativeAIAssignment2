using Common.Domain;

namespace Tracking.Domain.ValueObjects;

/// <summary>
/// Location value object representing GPS coordinates
/// </summary>
public class Location : ValueObject
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTime Timestamp { get; private set; }

    private Location() { } // EF Core

    public Location(double latitude, double longitude, DateTime? timestamp = null)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
        
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
        Timestamp = timestamp ?? DateTime.UtcNow;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public double DistanceTo(Location other)
    {
        // Haversine formula to calculate distance between two points on Earth
        const double R = 6371; // Earth's radius in kilometers

        var lat1 = ToRadians(Latitude);
        var lat2 = ToRadians(other.Latitude);
        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distance in kilometers
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public override string ToString()
    {
        return $"({Latitude:F6}, {Longitude:F6})";
    }
}
