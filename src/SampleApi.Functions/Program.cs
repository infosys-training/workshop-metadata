using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleApi.Entities.Models;
using SampleApi.Functions.Middleware;
using SampleApi.Repository.DbContext;
using SampleApi.Repository.Implementations;
using SampleApi.Repository.Interfaces;
using SampleApi.Services.Implementations;
using SampleApi.Services.Interfaces;
using SampleApi.Services.ResponseShaping;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<ExceptionHandlingMiddleware>();
        worker.UseMiddleware<AuthenticationMiddleware>();
        worker.UseMiddleware<ConsumerResolverMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        // Configure Entity Framework Core
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                context.Configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        services.AddScoped<IRepository<Product>, Repository<Product>>();
        services.AddScoped<IRepository<Category>, Repository<Category>>();

        // Register response shaping
        services.AddSingleton<IResponseShaperFactory<Product>, ResponseShaperFactory>();

        // Register services
        services.AddScoped<IProductService, ProductService>();
    })
    .Build();

host.Run();
