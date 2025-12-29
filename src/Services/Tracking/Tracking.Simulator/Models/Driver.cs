namespace Tracking.Simulator.Models;

public class Driver
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Location CurrentLocation { get; set; } = new();
    public double SpeedKmPerHour { get; set; }
    public double BearingDegrees { get; set; }
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
}

public class LocationUpdateRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
