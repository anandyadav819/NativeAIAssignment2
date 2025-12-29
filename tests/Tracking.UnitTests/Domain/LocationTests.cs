using FluentAssertions;
using Tracking.Domain.ValueObjects;

namespace Tracking.UnitTests.Domain;

public class LocationTests
{
    [Fact]
    public void Constructor_WithValidCoordinates_CreatesLocation()
    {
        // Arrange & Act
        var location = new Location(40.7128, -74.0060);

        // Assert
        location.Latitude.Should().Be(40.7128);
        location.Longitude.Should().Be(-74.0060);
        location.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithCustomTimestamp_UsesProvidedTimestamp()
    {
        // Arrange
        var customTimestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var location = new Location(40.7128, -74.0060, customTimestamp);

        // Assert
        location.Timestamp.Should().Be(customTimestamp);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(-90.1, 0)]
    [InlineData(90.1, 0)]
    public void Constructor_WithInvalidLatitude_ThrowsArgumentException(double latitude, double longitude)
    {
        // Act & Assert
        var act = () => new Location(latitude, longitude);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Latitude must be between -90 and 90*");
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    [InlineData(0, -180.1)]
    [InlineData(0, 180.1)]
    public void Constructor_WithInvalidLongitude_ThrowsArgumentException(double latitude, double longitude)
    {
        // Act & Assert
        var act = () => new Location(latitude, longitude);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Longitude must be between -180 and 180*");
    }

    [Fact]
    public void DistanceTo_SameLocation_ReturnsZero()
    {
        // Arrange
        var location1 = new Location(40.7128, -74.0060);
        var location2 = new Location(40.7128, -74.0060);

        // Act
        var distance = location1.DistanceTo(location2);

        // Assert
        distance.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void DistanceTo_NewYorkToLosAngeles_ReturnsCorrectDistance()
    {
        // Arrange - New York and Los Angeles coordinates
        var newYork = new Location(40.7128, -74.0060);
        var losAngeles = new Location(34.0522, -118.2437);

        // Act
        var distance = newYork.DistanceTo(losAngeles);

        // Assert - Distance should be approximately 3936 km
        distance.Should().BeApproximately(3936, 50);
    }

    [Fact]
    public void DistanceTo_CloseLocations_ReturnsSmallDistance()
    {
        // Arrange - Two locations 1km apart approximately
        var location1 = new Location(40.7128, -74.0060);
        var location2 = new Location(40.7218, -74.0060); // ~1km north

        // Act
        var distance = location1.DistanceTo(location2);

        // Assert - Distance should be approximately 1 km
        distance.Should().BeApproximately(1, 0.1);
    }

    [Fact]
    public void Equals_SameCoordinates_ReturnsTrue()
    {
        // Arrange
        var location1 = new Location(40.7128, -74.0060);
        var location2 = new Location(40.7128, -74.0060);

        // Act & Assert
        location1.Should().Be(location2);
        (location1 == location2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var location1 = new Location(40.7128, -74.0060);
        var location2 = new Location(40.7129, -74.0061);

        // Act & Assert
        location1.Should().NotBe(location2);
        (location1 == location2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsFormattedCoordinates()
    {
        // Arrange
        var location = new Location(40.712800, -74.006000);

        // Act
        var result = location.ToString();

        // Assert
        result.Should().Be("(40.712800, -74.006000)");
    }
}
