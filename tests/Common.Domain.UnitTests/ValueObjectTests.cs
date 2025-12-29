using Common.Domain;
using FluentAssertions;

namespace Common.Domain.UnitTests;

public class ValueObjectTests
{
    private class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string Country { get; }
        public string ZipCode { get; }

        public Address(string street, string city, string country, string zipCode)
        {
            Street = street;
            City = city;
            Country = country;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return Country;
            yield return ZipCode;
        }
    }

    private class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void Equals_WithIdenticalValues_ShouldReturnTrue()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("123 Main St", "New York", "USA", "10001");

        // Act & Assert
        address1.Equals(address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("456 Oak Ave", "Los Angeles", "USA", "90001");

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithOneDifferentComponent_ShouldReturnFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("123 Main St", "New York", "USA", "10002"); // Different zip

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var address = new Address("123 Main St", "New York", "USA", "10001");

        // Act & Assert
        address.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var address = new Address("123 Main St", "New York", "USA", "10001");
        var money = new Money(100m, "USD");

        // Act & Assert
        address.Equals(money).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_WithIdenticalValues_ShouldReturnTrue()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("123 Main St", "New York", "USA", "10001");

        // Act & Assert
        (address1 == address2).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("456 Oak Ave", "Los Angeles", "USA", "90001");

        // Act & Assert
        (address1 == address2).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_WithIdenticalValues_ShouldReturnFalse()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("123 Main St", "New York", "USA", "10001");

        // Act & Assert
        (address1 != address2).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_WithDifferentValues_ShouldReturnTrue()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("456 Oak Ave", "Los Angeles", "USA", "90001");

        // Act & Assert
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithIdenticalValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("123 Main St", "New York", "USA", "10001");

        // Act & Assert
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("456 Oak Ave", "Los Angeles", "USA", "90001");

        // Act & Assert
        address1.GetHashCode().Should().NotBe(address2.GetHashCode());
    }

    [Fact]
    public void ValueObjects_InHashSet_ShouldWorkCorrectly()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("123 Main St", "New York", "USA", "10001"); // Same as address1
        var address3 = new Address("456 Oak Ave", "Los Angeles", "USA", "90001");

        var hashSet = new HashSet<Address> { address1, address3 };

        // Act & Assert
        hashSet.Should().HaveCount(2);
        hashSet.Contains(address1).Should().BeTrue();
        hashSet.Contains(address2).Should().BeTrue(); // Should find it because values are equal
        hashSet.Contains(address3).Should().BeTrue();
    }

    [Fact]
    public void ValueObjects_AsDictionaryKeys_ShouldWorkCorrectly()
    {
        // Arrange
        var address1 = new Address("123 Main St", "New York", "USA", "10001");
        var address2 = new Address("123 Main St", "New York", "USA", "10001"); // Same as address1
        var dictionary = new Dictionary<Address, string>
        {
            { address1, "Home" }
        };

        // Act & Assert
        dictionary.ContainsKey(address2).Should().BeTrue(); // Should find it because values are equal
        dictionary[address2].Should().Be("Home");
    }

    [Fact]
    public void Copy_ShouldBeEqualToOriginal()
    {
        // Arrange
        var original = new Money(100.50m, "USD");
        var copy = new Money(100.50m, "USD");

        // Act & Assert
        copy.Should().Be(original);
        copy.Equals(original).Should().BeTrue();
    }
}
