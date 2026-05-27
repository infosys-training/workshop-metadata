using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SampleApi.Entities.Models;
using SampleApi.Repository.DbContext;
using SampleApi.Repository.Implementations;

namespace SampleApi.Repository.Tests;

/// <summary>
/// Unit tests for the generic <see cref="Repository{T}"/> implementation.
/// </summary>
public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<Product> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new Repository<Product>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        var product = CreateTestProduct();

        var result = await _repository.AddAsync(product);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var product = CreateTestProduct();
        await _repository.AddAsync(product);

        var result = await _repository.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        await _repository.AddAsync(CreateTestProduct("Product 1"));
        await _repository.AddAsync(CreateTestProduct("Product 2"));
        await _repository.AddAsync(CreateTestProduct("Product 3"));

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoEntities()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        var product = CreateTestProduct();
        await _repository.AddAsync(product);

        product.Name = "Updated Product";
        product.Price = 29.99m;
        await _repository.UpdateAsync(product);

        var result = await _repository.GetByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Product");
        result.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity_WhenExists()
    {
        var product = CreateTestProduct();
        await _repository.AddAsync(product);
        var id = product.Id;

        await _repository.DeleteAsync(id);

        var result = await _repository.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenEntityNotExists()
    {
        var act = () => _repository.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }

    private static Product CreateTestProduct(string name = "Test Product")
    {
        return new Product
        {
            Name = name,
            Description = "A test product",
            Price = 19.99m,
            CategoryId = 1,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
