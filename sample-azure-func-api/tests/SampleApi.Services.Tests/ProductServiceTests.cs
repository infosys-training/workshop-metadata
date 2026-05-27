using FluentAssertions;
using Moq;
using SampleApi.DTOs.Requests;
using SampleApi.Entities.Models;
using SampleApi.Repository.Interfaces;
using SampleApi.Services.Implementations;
using SampleApi.Services.ResponseShaping;

namespace SampleApi.Services.Tests;

/// <summary>
/// Unit tests for <see cref="ProductService"/>.
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IRepository<Product>> _mockRepository;
    private readonly Mock<IResponseShaperFactory<Product>> _mockShaperFactory;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IRepository<Product>>();
        _mockShaperFactory = new Mock<IResponseShaperFactory<Product>>();
        _service = new ProductService(_mockRepository.Object, _mockShaperFactory.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        var product = CreateTestProduct(1);
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(19.99m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnShapedResponses()
    {
        var products = new List<Product>
        {
            CreateTestProduct(1, "Product 1"),
            CreateTestProduct(2, "Product 2")
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var mockShaper = new Mock<IResponseShaper<Product>>();
        mockShaper.Setup(s => s.ShapeCollection(It.IsAny<IEnumerable<Product>>()))
            .Returns(products.Select(p => (object)new { p.Id, p.Name }));
        _mockShaperFactory.Setup(f => f.GetShaper("test-consumer")).Returns(mockShaper.Object);

        var result = await _service.GetAllAsync("test-consumer");

        result.Should().HaveCount(2);
        _mockShaperFactory.Verify(f => f.GetShaper("test-consumer"), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoProducts()
    {
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

        var mockShaper = new Mock<IResponseShaper<Product>>();
        mockShaper.Setup(s => s.ShapeCollection(It.IsAny<IEnumerable<Product>>()))
            .Returns(Enumerable.Empty<object>());
        _mockShaperFactory.Setup(f => f.GetShaper("test")).Returns(mockShaper.Object);

        var result = await _service.GetAllAsync("test");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnProduct()
    {
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Description = "A new product",
            Price = 29.99m,
            CategoryId = 1
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) =>
            {
                p.Id = 1;
                return p;
            });

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.Price.Should().Be(29.99m);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndReturnProduct()
    {
        var existingProduct = CreateTestProduct(1);
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            Description = "Updated description",
            Price = 39.99m,
            CategoryId = 2
        };

        var result = await _service.UpdateAsync(1, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Product");
        result.Price.Should().Be(39.99m);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenProductNotExists()
    {
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var request = new UpdateProductRequest
        {
            Name = "Updated",
            Price = 10m,
            CategoryId = 1
        };

        var act = () => _service.UpdateAsync(999, request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct()
    {
        var product = CreateTestProduct(1);
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _mockRepository.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(1);

        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenProductNotExists()
    {
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var act = () => _service.DeleteAsync(999);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    private static Product CreateTestProduct(int id, string name = "Test Product")
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = "A test product",
            Price = 19.99m,
            CategoryId = 1,
            CreatedDate = DateTime.UtcNow
        };
    }
}
