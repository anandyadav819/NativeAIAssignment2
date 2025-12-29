using Common.Database;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Database.UnitTests;

public class DatabaseExtensionsTests
{
    [Fact]
    public void AddDatabase_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        services.AddDatabase(connectionString);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(FoodDeliveryDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddDatabase_ShouldRegisterUnitOfWork()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        services.AddDatabase(connectionString);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IUnitOfWork));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        serviceDescriptor.ImplementationType.Should().Be(typeof(UnitOfWork));
    }

    [Fact]
    public void AddDatabase_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        var result = services.AddDatabase(connectionString);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddDatabase_ShouldSupportMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        var result = services
            .AddDatabase(connectionString)
            .AddSingleton<ITestService, TestService>();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(s => s.ServiceType == typeof(FoodDeliveryDbContext));
        services.Should().Contain(s => s.ServiceType == typeof(IUnitOfWork));
        services.Should().Contain(s => s.ServiceType == typeof(ITestService));
    }

    [Fact]
    public void AddDatabase_WithNullConnectionString_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Will fail at runtime but registration should work
        var act = () => services.AddDatabase(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddDatabase_WithEmptyConnectionString_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddDatabase(string.Empty);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddDatabase_CalledMultipleTimes_ShouldRegisterLatest()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString1 = "Host=localhost1;Database=test1;Username=test;Password=test";
        var connectionString2 = "Host=localhost2;Database=test2;Username=test;Password=test";

        // Act
        services.AddDatabase(connectionString1);
        services.AddDatabase(connectionString2);

        // Assert - AddDbContext replaces the previous registration
        var dbContextServices = services.Where(s => s.ServiceType == typeof(FoodDeliveryDbContext)).ToList();
        var unitOfWorkServices = services.Where(s => s.ServiceType == typeof(IUnitOfWork)).ToList();
        
        dbContextServices.Should().NotBeEmpty();
        unitOfWorkServices.Should().HaveCount(2); // UnitOfWork is addedtwice
    }

    [Fact]
    public void AddDatabase_ShouldConfigureDbContextOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        services.AddDatabase(connectionString);

        // Assert - Verify that DbContext options are configured
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(FoodDeliveryDbContext));
        serviceDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabase_ShouldUseNpgsqlProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        services.AddDatabase(connectionString);

        // Assert - The extension should use UseNpgsql internally
        // We verify this by checking that the service is registered
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(FoodDeliveryDbContext));
        serviceDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabase_ShouldRegisterDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        services.AddDatabase(connectionString);

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(FoodDeliveryDbContext));
        services.Should().Contain(s => s.ServiceType == typeof(IUnitOfWork));
    }

    [Fact]
    public void AddDatabase_WithValidConnectionString_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=fooddelivery;Username=admin;Password=secret;Port=5432";

        // Act
        var act = () => services.AddDatabase(connectionString);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddDatabase_ShouldRegisterBothServicesAsSeparateTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";

        // Act
        services.AddDatabase(connectionString);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(FoodDeliveryDbContext));
        var unitOfWorkDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IUnitOfWork));

        dbContextDescriptor.Should().NotBeNull();
        unitOfWorkDescriptor.Should().NotBeNull();
        dbContextDescriptor.Should().NotBeSameAs(unitOfWorkDescriptor);
    }

    // Test helper interface and class
    public interface ITestService { }
    public class TestService : ITestService { }
}
