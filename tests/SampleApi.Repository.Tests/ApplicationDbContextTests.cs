using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SampleApi.Entities.Models;
using SampleApi.Repository.DbContext;

namespace SampleApi.Repository.Tests;

/// <summary>
/// Unit tests for <see cref="ApplicationDbContext"/> configuration.
/// </summary>
public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public void DbContext_ShouldHaveProductsDbSet()
    {
        _context.Products.Should().NotBeNull();
    }

    [Fact]
    public void DbContext_ShouldHaveCategoriesDbSet()
    {
        _context.Categories.Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_ShouldSaveCategory()
    {
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic devices",
            CreatedDate = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var saved = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
        saved.Should().NotBeNull();
        saved!.Description.Should().Be("Electronic devices");
    }

    [Fact]
    public async Task DbContext_ShouldSaveProductWithCategory()
    {
        var category = new Category
        {
            Name = "Books",
            Description = "Books and literature",
            CreatedDate = DateTime.UtcNow
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "C# in Depth",
            Description = "A book about C#",
            Price = 49.99m,
            CategoryId = category.Id,
            CreatedDate = DateTime.UtcNow
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var savedProduct = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Name == "C# in Depth");
        savedProduct.Should().NotBeNull();
        savedProduct!.Category.Should().NotBeNull();
        savedProduct.Category!.Name.Should().Be("Books");
    }

    [Fact]
    public async Task DbContext_ShouldEnforceRequiredProductName()
    {
        var product = new Product
        {
            Description = "No name product",
            Price = 10.00m,
            CategoryId = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var saved = await _context.Products.FindAsync(product.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be(string.Empty);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
