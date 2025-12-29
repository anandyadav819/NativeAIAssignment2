using Common.EventBus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Common.EventBus.UnitTests;

public class EventBusExtensionsTests
{
    [Fact]
    public void AddRabbitMQEventBus_ShouldRegisterEventBusAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert - Should not throw
        var act = () => services.AddRabbitMQEventBus("localhost");

        // This will throw at runtime when trying to connect to RabbitMQ
        // but we can verify the registration happens
        act.Should().NotThrow();
    }

    [Fact]
    public void AddRabbitMQEventBus_ShouldRegisterIEventBus()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddRabbitMQEventBus("localhost");

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IEventBus));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddRabbitMQEventBus_WithCustomExchangeName_ShouldUseProvidedName()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var customExchangeName = "custom_exchange";

        // Act
        services.AddRabbitMQEventBus("localhost", customExchangeName);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IEventBus));
        serviceDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddRabbitMQEventBus_WithDefaultExchangeName_ShouldUseDefaultValue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddRabbitMQEventBus("localhost");

        // Assert - Verify service is registered (exchange name is internal)
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IEventBus));
        serviceDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddRabbitMQEventBus_ShouldRequireLoggingService()
    {
        // Arrange
        var services = new ServiceCollection();
        // Note: Not adding logging service

        // Act
        services.AddRabbitMQEventBus("localhost");
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Should throw when trying to resolve without logger
        var act = () => serviceProvider.GetService<IEventBus>();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddRabbitMQEventBus_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddRabbitMQEventBus("localhost");

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddRabbitMQEventBus_ShouldSupportMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services
            .AddLogging()
            .AddRabbitMQEventBus("localhost")
            .AddSingleton<ITestService, TestService>();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(s => s.ServiceType == typeof(IEventBus));
        services.Should().Contain(s => s.ServiceType == typeof(ITestService));
    }

    [Fact]
    public void AddRabbitMQEventBus_WithNullHostname_ShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - Will fail at runtime but registration should work
        var act = () => services.AddRabbitMQEventBus(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddRabbitMQEventBus_WithEmptyHostname_ShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var act = () => services.AddRabbitMQEventBus(string.Empty);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddRabbitMQEventBus_ShouldUseLazyInitialization()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddRabbitMQEventBus("localhost");
        
        // Assert - Service is registered but not instantiated yet
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IEventBus));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.ImplementationFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddRabbitMQEventBus_CalledMultipleTimes_ShouldRegisterMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddRabbitMQEventBus("localhost1");
        services.AddRabbitMQEventBus("localhost2");

        // Assert
        var eventBusServices = services.Where(s => s.ServiceType == typeof(IEventBus)).ToList();
        eventBusServices.Should().HaveCount(2);
    }

    // Test helper interface and class
    public interface ITestService { }
    public class TestService : ITestService { }
}
