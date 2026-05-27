using FluentAssertions;
using SampleApi.DTOs.Responses;
using SampleApi.Entities.Models;
using SampleApi.Services.ResponseShaping;

namespace SampleApi.Services.Tests.ResponseShaping;

/// <summary>
/// Unit tests for <see cref="FullResponseShaper"/>.
/// </summary>
public class FullResponseShaperTests
{
    private readonly FullResponseShaper _shaper = new();

    [Fact]
    public void Shape_ShouldReturnFullResponse()
    {
        var product = CreateTestProduct();

        var result = _shaper.Shape(product);

        result.Should().BeOfType<ProductResponseFull>();
        var response = (ProductResponseFull)result;
        response.Id.Should().Be(1);
        response.Name.Should().Be("Test Product");
        response.Description.Should().Be("A test product");
        response.Price.Should().Be(19.99m);
        response.CategoryId.Should().Be(1);
        response.CategoryName.Should().Be("Electronics");
        response.CreatedBy.Should().Be("admin");
    }

    [Fact]
    public void Shape_ShouldHandleNullCategory()
    {
        var product = CreateTestProduct();
        product.Category = null;

        var result = _shaper.Shape(product);

        var response = (ProductResponseFull)result;
        response.CategoryName.Should().BeNull();
    }

    [Fact]
    public void ShapeCollection_ShouldReturnAllProducts()
    {
        var products = new List<Product>
        {
            CreateTestProduct(1, "Product 1"),
            CreateTestProduct(2, "Product 2"),
            CreateTestProduct(3, "Product 3")
        };

        var result = _shaper.ShapeCollection(products).ToList();

        result.Should().HaveCount(3);
        result.Should().AllBeOfType<ProductResponseFull>();
    }

    [Fact]
    public void ShapeCollection_ShouldReturnEmpty_WhenEmptyInput()
    {
        var result = _shaper.ShapeCollection(Enumerable.Empty<Product>()).ToList();

        result.Should().BeEmpty();
    }

    private static Product CreateTestProduct(int id = 1, string name = "Test Product")
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = "A test product",
            Price = 19.99m,
            CategoryId = 1,
            Category = new Category { Id = 1, Name = "Electronics" },
            CreatedBy = "admin",
            CreatedDate = DateTime.UtcNow
        };
    }
}
