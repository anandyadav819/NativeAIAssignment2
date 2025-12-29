using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Tracking.Simulator.Services;

namespace Tracking.Simulator;

class Program
{
    static async Task Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureServices((context, services) =>
                {
                    // Add HTTP client for Tracking API
                    var trackingApiUrl = context.Configuration["TrackingAPI:BaseUrl"] 
                        ?? "http://localhost:5002";
                    
                    services.AddHttpClient("TrackingAPI", client =>
                    {
                        client.BaseAddress = new Uri(trackingApiUrl);
                        client.Timeout = TimeSpan.FromSeconds(30);
                    });

                    // Add services
                    services.AddSingleton<GpsSimulator>();
                    services.AddSingleton<DriverSimulationService>();
                })
                .UseSerilog()
                .Build();

            await RunSimulationAsync(host, args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static async Task RunSimulationAsync(IHost host, string[] args)
    {
        var simulationService = host.Services.GetRequiredService<DriverSimulationService>();
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        // Parse command line arguments or use defaults
        var driverCount = GetIntArgument(args, "--drivers", 
            configuration.GetValue<int>("Simulation:DriverCount", 50));
        var updatesPerSecond = GetIntArgument(args, "--rate", 
            configuration.GetValue<int>("Simulation:UpdatesPerSecond", 10));
        var durationSeconds = GetIntArgument(args, "--duration", 
            configuration.GetValue<int>("Simulation:DurationSeconds", 60));

        // Print banner
        PrintBanner(driverCount, updatesPerSecond, durationSeconds);

        // Initialize drivers
        simulationService.InitializeDrivers(driverCount);

        // Run simulation
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Log.Information("Simulation stopped by user");
        };

        await simulationService.RunSimulationAsync(updatesPerSecond, durationSeconds, cts.Token);
    }

    static int GetIntArgument(string[] args, string name, int defaultValue)
    {
        var index = Array.IndexOf(args, name);
        if (index >= 0 && index < args.Length - 1 && int.TryParse(args[index + 1], out var value))
        {
            return value;
        }
        return defaultValue;
    }

    static void PrintBanner(int drivers, int rate, int duration)
    {
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("║        🚗 DRIVER LOCATION SIMULATOR 🗺️                    ║");
        Console.WriteLine("║                                                           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("📋 Configuration:");
        Console.WriteLine($"   👥 Drivers: {drivers}");
        Console.WriteLine($"   📡 Updates per second: {rate}");
        Console.WriteLine($"   ⏱️  Duration: {duration} seconds");
        Console.WriteLine($"   🎯 Target load: {drivers * rate} total events/sec");
        Console.WriteLine();
        Console.WriteLine("💡 Usage:");
        Console.WriteLine("   dotnet run --drivers 50 --rate 10 --duration 60");
        Console.WriteLine("   Press Ctrl+C to stop the simulation");
        Console.WriteLine();
        Console.WriteLine(new string('─', 60));
        Console.WriteLine();
    }
}
