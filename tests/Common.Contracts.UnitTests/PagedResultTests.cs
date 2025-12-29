using Common.Contracts.Models;
using FluentAssertions;

namespace Common.Contracts.UnitTests;

public class PagedResultTests
{
    private record TestItem(int Id, string Name);

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePagedResult()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new(1, "Item 1"),
            new(2, "Item 2"),
            new(3, "Item 3")
        };
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 25;

        // Act
        var result = new PagedResult<TestItem>(items, totalCount, pageNumber, pageSize);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Should().BeEquivalentTo(items);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);
    }

    [Fact]
    public void TotalPages_WithExactDivision_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<TestItem>();
        var totalCount = 100;
        var pageSize = 10;

        // Act
        var result = new PagedResult<TestItem>(items, totalCount, 1, pageSize);

        // Assert
        result.TotalPages.Should().Be(10);
    }

    [Fact]
    public void TotalPages_WithRemainder_ShouldRoundUp()
    {
        // Arrange
        var items = new List<TestItem>();
        var totalCount = 95;
        var pageSize = 10;

        // Act
        var result = new PagedResult<TestItem>(items, totalCount, 1, pageSize);

        // Assert
        result.TotalPages.Should().Be(10); // 95 / 10 = 9.5, rounds up to 10
    }

    [Fact]
    public void TotalPages_WithZeroTotalCount_ShouldBeZero()
    {
        // Arrange
        var items = new List<TestItem>();

        // Act
        var result = new PagedResult<TestItem>(items, 0, 1, 10);

        // Assert
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasPreviousPage_OnFirstPage_ShouldBeFalse()
    {
        // Arrange
        var items = new List<TestItem>();

        // Act
        var result = new PagedResult<TestItem>(items, 100, 1, 10);

        // Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_OnSecondPage_ShouldBeTrue()
    {
        // Arrange
        var items = new List<TestItem>();

        // Act
        var result = new PagedResult<TestItem>(items, 100, 2, 10);

        // Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_OnLastPage_ShouldBeFalse()
    {
        // Arrange
        var items = new List<TestItem>();
        var totalCount = 100;
        var pageSize = 10;
        var lastPage = 10;

        // Act
        var result = new PagedResult<TestItem>(items, totalCount, lastPage, pageSize);

        // Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_OnFirstPage_ShouldBeTrue()
    {
        // Arrange
        var items = new List<TestItem>();

        // Act
        var result = new PagedResult<TestItem>(items, 100, 1, 10);

        // Assert
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_WithPartialLastPage_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<TestItem>();
        var totalCount = 95;
        var pageSize = 10;

        // Act
        var result1 = new PagedResult<TestItem>(items, totalCount, 9, pageSize);
        var result2 = new PagedResult<TestItem>(items, totalCount, 10, pageSize);

        // Assert
        result1.HasNextPage.Should().BeTrue();
        result2.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void EmptyResult_ShouldHaveNoItems()
    {
        // Act
        var result = new PagedResult<TestItem>(new List<TestItem>(), 0, 1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResult_WithSingleItem_ShouldWork()
    {
        // Arrange
        var items = new List<TestItem> { new(1, "Single Item") };

        // Act
        var result = new PagedResult<TestItem>(items, 1, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PagedResult_WithDifferentItemsPerPage_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<TestItem>();

        // Act
        var result5 = new PagedResult<TestItem>(items, 100, 1, 5);
        var result25 = new PagedResult<TestItem>(items, 100, 1, 25);
        var result50 = new PagedResult<TestItem>(items, 100, 1, 50);

        // Assert
        result5.TotalPages.Should().Be(20);
        result25.TotalPages.Should().Be(4);
        result50.TotalPages.Should().Be(2);
    }

    [Fact]
    public void PagedResult_MiddlePage_ShouldHaveBothPreviousAndNext()
    {
        // Arrange
        var items = new List<TestItem>();

        // Act
        var result = new PagedResult<TestItem>(items, 100, 5, 10);

        // Assert
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PagedResult_ItemsCollection_ShouldBeReadOnly()
    {
        // Arrange
        var items = new List<TestItem> { new(1, "Item") };

        // Act
        var result = new PagedResult<TestItem>(items, 1, 1, 10);

        // Assert
        result.Items.Should().BeAssignableTo<IReadOnlyList<TestItem>>();
    }
}
