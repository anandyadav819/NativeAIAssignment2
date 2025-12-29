using Common.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace Common.Caching.UnitTests;

public class HybridCacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _redisDatabaseMock;
    private readonly Mock<ILogger<HybridCacheService>> _loggerMock;
    private readonly IMemoryCache _memoryCache;
    private readonly HybridCacheService _cacheService;

    public HybridCacheServiceTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _redisDatabaseMock = new Mock<IDatabase>();
        _loggerMock = new Mock<ILogger<HybridCacheService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_redisDatabaseMock.Object);

        _cacheService = new HybridCacheService(_memoryCache, _redisMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenItemInL1Cache_ShouldReturnFromL1()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };
        _memoryCache.Set(key, value);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
        _redisDatabaseMock.Verify(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_WhenItemInL2CacheOnly_ShouldReturnFromL2AndPopulateL1()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(value);

        _redisDatabaseMock.Setup(r => r.StringGetAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()))
            .ReturnsAsync(json);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
        
        // Verify L1 cache was populated
        var l1Result = _memoryCache.Get<TestData>(key);
        l1Result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_WhenItemNotInCache_ShouldReturnNull()
    {
        // Arrange
        var key = "non-existent-key";
        _redisDatabaseMock.Setup(r => r.StringGetAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldStoreInBothCaches()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMinutes(10);

        _redisDatabaseMock.Setup(r => r.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetAsync(key, value, expiration);

        // Assert
        var l1Result = _memoryCache.Get<TestData>(key);
        l1Result.Should().NotBeNull();
        l1Result!.Id.Should().Be(1);

        _redisDatabaseMock.Verify(r => r.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithNullExpiration_ShouldUseDefaultExpiration()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };

        _redisDatabaseMock.Setup(r => r.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        var l1Result = _memoryCache.Get<TestData>(key);
        l1Result.Should().NotBeNull();

        _redisDatabaseMock.Verify(r => r.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveFromBothCaches()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };
        _memoryCache.Set(key, value);

        _redisDatabaseMock.Setup(r => r.KeyDeleteAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        var l1Result = _memoryCache.Get<TestData>(key);
        l1Result.Should().BeNull();

        _redisDatabaseMock.Verify(r => r.KeyDeleteAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenItemExists_ShouldReturnCachedValue()
    {
        // Arrange
        var key = "test-key";
        var existingValue = new TestData { Id = 1, Name = "Existing" };
        _memoryCache.Set(key, existingValue);

        var factoryCalled = false;
        Task<TestData> Factory()
        {
            factoryCalled = true;
            return Task.FromResult(new TestData { Id = 2, Name = "New" });
        }

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, Factory);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1); // Should return existing value
        result.Name.Should().Be("Existing");
        factoryCalled.Should().BeFalse(); // Factory should not be called
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenItemDoesNotExist_ShouldCallFactoryAndCache()
    {
        // Arrange
        var key = "test-key";
        var newValue = new TestData { Id = 1, Name = "New" };

        _redisDatabaseMock.Setup(r => r.StringGetAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        _redisDatabaseMock.Setup(r => r.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var factoryCalled = false;
        Task<TestData> Factory()
        {
            factoryCalled = true;
            return Task.FromResult(newValue);
        }

        // Act
        var result = await _cacheService.GetOrCreateAsync(key, Factory);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("New");
        factoryCalled.Should().BeTrue();

        // Verify it was cached
        var l1Result = _memoryCache.Get<TestData>(key);
        l1Result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenItemInL1Cache_ShouldReturnTrue()
    {
        // Arrange
        var key = "test-key";
        _memoryCache.Set(key, new TestData { Id = 1, Name = "Test" });

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenItemInL2CacheOnly_ShouldReturnTrue()
    {
        // Arrange
        var key = "test-key";
        _redisDatabaseMock.Setup(r => r.KeyExistsAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenItemNotInCache_ShouldReturnFalse()
    {
        // Arrange
        var key = "non-existent-key";
        _redisDatabaseMock.Setup(r => r.KeyExistsAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_WithRedisException_ShouldHandleGracefully()
    {
        // Arrange
        var key = "test-key";
        _redisDatabaseMock.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.InternalFailure, "Connection failed"));

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithComplexObject_ShouldSerializeCorrectly()
    {
        // Arrange
        var key = "complex-key";
        var value = new ComplexTestData
        {
            Id = 1,
            Name = "Test",
            Items = new List<string> { "Item1", "Item2" },
            Metadata = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 42 }
            }
        };

        _redisDatabaseMock.Setup(r => r.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        var l1Result = _memoryCache.Get<ComplexTestData>(key);
        l1Result.Should().NotBeNull();
        l1Result!.Id.Should().Be(1);
        l1Result.Items.Should().HaveCount(2);
        l1Result.Metadata.Should().HaveCount(2);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class ComplexTestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
