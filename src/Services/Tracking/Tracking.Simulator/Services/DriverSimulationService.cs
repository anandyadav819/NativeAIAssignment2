using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tracking.Simulator.Models;

namespace Tracking.Simulator.Services;

/// <summary>
/// Manages driver simulation and sends location updates to the Tracking API
/// </summary>
public class DriverSimulationService
{
    private readonly HttpClient _httpClient;
    private readonly GpsSimulator _gpsSimulator;
    private readonly ILogger<DriverSimulationService> _logger;
    private readonly List<Driver> _drivers = new();
    private readonly Stopwatch _stopwatch = new();
    private int _totalUpdatesSent = 0;
    private int _successfulUpdates = 0;
    private int _failedUpdates = 0;

    public DriverSimulationService(
        IHttpClientFactory httpClientFactory,
        GpsSimulator gpsSimulator,
        ILogger<DriverSimulationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("TrackingAPI");
        _gpsSimulator = gpsSimulator;
        _logger = logger;
    }

    /// <summary>
    /// Initialize drivers with random starting positions
    /// </summary>
    public void InitializeDrivers(int driverCount)
    {
        _logger.LogInformation("Initializing {DriverCount} drivers...", driverCount);

        _drivers.Clear();
        for (int i = 0; i < driverCount; i++)
        {
            var driver = new Driver
            {
                Id = Guid.NewGuid(),
                Name = $"Driver-{i + 1:D3}",
                CurrentLocation = _gpsSimulator.GenerateRandomLocation(),
                SpeedKmPerHour = _gpsSimulator.GenerateRandomSpeed(),
                BearingDegrees = _gpsSimulator.GenerateRandomBearing()
            };

            _drivers.Add(driver);
        }

        _logger.LogInformation("Initialized {DriverCount} drivers", _drivers.Count);
    }

    /// <summary>
    /// Start the simulation - sends location updates at specified rate
    /// </summary>
    public async Task RunSimulationAsync(int updatesPerSecond, int durationSeconds, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting simulation: {DriverCount} drivers, {UpdatesPerSecond} updates/sec, {Duration} seconds",
            _drivers.Count,
            updatesPerSecond,
            durationSeconds);

        _stopwatch.Restart();
        var intervalMs = 1000.0 / updatesPerSecond;
        var endTime = DateTime.UtcNow.AddSeconds(durationSeconds);

        var updateTasks = new List<Task>();
        var lastPrintTime = DateTime.UtcNow;

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            var iterationStart = DateTime.UtcNow;

            // Send location update for each driver
            foreach (var driver in _drivers)
            {
                updateTasks.Add(UpdateDriverLocationAsync(driver, cancellationToken));

                // Throttle to maintain desired rate
                if (updateTasks.Count >= updatesPerSecond)
                {
                    await Task.WhenAll(updateTasks);
                    updateTasks.Clear();
                }
            }

            // Wait for remaining tasks
            if (updateTasks.Any())
            {
                await Task.WhenAll(updateTasks);
                updateTasks.Clear();
            }

            // Print progress every 5 seconds
            if ((DateTime.UtcNow - lastPrintTime).TotalSeconds >= 5)
            {
                PrintProgress();
                lastPrintTime = DateTime.UtcNow;
            }

            // Calculate sleep time to maintain rate
            var elapsed = (DateTime.UtcNow - iterationStart).TotalMilliseconds;
            var sleepTime = Math.Max(0, intervalMs - elapsed);
            
            if (sleepTime > 0)
            {
                await Task.Delay((int)sleepTime, cancellationToken);
            }
        }

        _stopwatch.Stop();
        PrintFinalStatistics();
    }

    /// <summary>
    /// Update a single driver's location
    /// </summary>
    private async Task UpdateDriverLocationAsync(Driver driver, CancellationToken cancellationToken)
    {
        try
        {
            // Move driver to new location
            driver.CurrentLocation = _gpsSimulator.MoveDriver(
                driver.CurrentLocation,
                driver.SpeedKmPerHour,
                driver.BearingDegrees,
                1.0); // 1 second interval

            // Occasionally adjust speed and bearing
            driver.SpeedKmPerHour = _gpsSimulator.AdjustSpeed(driver.SpeedKmPerHour);
            driver.BearingDegrees = _gpsSimulator.AdjustBearing(driver.BearingDegrees);

            // Send location update to API
            var request = new LocationUpdateRequest
            {
                Latitude = driver.CurrentLocation.Latitude,
                Longitude = driver.CurrentLocation.Longitude
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"/api/drivers/{driver.Id}/location",
                request,
                cancellationToken);

            Interlocked.Increment(ref _totalUpdatesSent);

            if (response.IsSuccessStatusCode)
            {
                Interlocked.Increment(ref _successfulUpdates);
            }
            else
            {
                Interlocked.Increment(ref _failedUpdates);
                _logger.LogWarning(
                    "Failed to update location for {DriverName}: {StatusCode}",
                    driver.Name,
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _totalUpdatesSent);
            Interlocked.Increment(ref _failedUpdates);
            _logger.LogError(ex, "Error updating location for {DriverName}", driver.Name);
        }
    }

    /// <summary>
    /// Print progress statistics
    /// </summary>
    private void PrintProgress()
    {
        var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        var updatesPerSecond = _totalUpdatesSent / elapsedSeconds;
        var successRate = _totalUpdatesSent > 0 
            ? (_successfulUpdates * 100.0) / _totalUpdatesSent 
            : 0;

        Console.WriteLine($"\n📊 Progress Update:");
        Console.WriteLine($"   ⏱️  Elapsed: {_stopwatch.Elapsed:mm\\:ss}");
        Console.WriteLine($"   📤 Total Updates: {_totalUpdatesSent:N0}");
        Console.WriteLine($"   ✅ Successful: {_successfulUpdates:N0}");
        Console.WriteLine($"   ❌ Failed: {_failedUpdates:N0}");
        Console.WriteLine($"   📈 Rate: {updatesPerSecond:F2} updates/sec");
        Console.WriteLine($"   💯 Success Rate: {successRate:F1}%");
    }

    /// <summary>
    /// Print final statistics
    /// </summary>
    private void PrintFinalStatistics()
    {
        var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        var updatesPerSecond = _totalUpdatesSent / elapsedSeconds;
        var successRate = _totalUpdatesSent > 0 
            ? (_successfulUpdates * 100.0) / _totalUpdatesSent 
            : 0;

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("🏁 SIMULATION COMPLETE");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"⏱️  Total Duration: {_stopwatch.Elapsed:mm\\:ss\\.fff}");
        Console.WriteLine($"👥 Drivers: {_drivers.Count}");
        Console.WriteLine($"📤 Total Updates Sent: {_totalUpdatesSent:N0}");
        Console.WriteLine($"✅ Successful Updates: {_successfulUpdates:N0}");
        Console.WriteLine($"❌ Failed Updates: {_failedUpdates:N0}");
        Console.WriteLine($"📈 Average Rate: {updatesPerSecond:F2} updates/sec");
        Console.WriteLine($"💯 Success Rate: {successRate:F1}%");
        Console.WriteLine($"⚡ Peak Throughput: {updatesPerSecond * _drivers.Count:F0} potential events/sec");
        Console.WriteLine(new string('=', 60));
    }
}
