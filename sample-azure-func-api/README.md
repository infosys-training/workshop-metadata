# Sample Azure Functions API

A .NET 8 Azure Functions v4 (isolated worker model) API demonstrating clean architecture with consumer-based response shaping, JWT authentication, Swagger/OpenAPI, and Entity Framework Core.

## Project Overview

This project follows clean architecture with separate projects for:

| Project | Responsibility |
|---------|---------------|
| **SampleApi.Entities** | Domain models (Product, Category) with base entity |
| **SampleApi.DTOs** | Request/response DTOs with validation |
| **SampleApi.Repository** | Generic repository pattern with EF Core |
| **SampleApi.Services** | Business logic and response shaping |
| **SampleApi.Functions** | Azure Functions HTTP triggers, middleware |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express LocalDB)

## Project Structure

```
sample-azure-func-api/
├── src/
│   ├── SampleApi.Entities/          # Domain models
│   ├── SampleApi.DTOs/              # Data transfer objects
│   ├── SampleApi.Repository/        # Data access layer
│   ├── SampleApi.Services/          # Business logic + response shaping
│   └── SampleApi.Functions/         # Azure Functions API layer
├── tests/
│   ├── SampleApi.Functions.Tests/   # API layer tests
│   ├── SampleApi.Services.Tests/    # Service layer tests
│   └── SampleApi.Repository.Tests/  # Repository tests
├── docs/
│   └── architecture.md             # Architecture documentation
├── Directory.Build.props            # Build settings
├── .editorconfig                    # Code style rules
├── sonar-project.properties         # SonarCloud config
└── SampleApi.sln                    # Solution file
```

## How to Run Locally

1. **Restore and build:**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Update the connection string** in `src/SampleApi.Functions/local.settings.json` if needed:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SampleApiDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **Run the Azure Functions project:**
   ```bash
   cd src/SampleApi.Functions
   func start
   ```

4. The API will be available at `http://localhost:7071/api/`

## How to Run Tests

```bash
dotnet test SampleApi.sln --verbosity normal
```

To generate code coverage:
```bash
dotnet test SampleApi.sln --settings coverlet.runsettings --collect:"XPlat Code Coverage"
```

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/health` | Health check |
| GET | `/api/products` | Get all products (consumer-shaped) |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create a product |
| PUT | `/api/products/{id}` | Update a product |
| DELETE | `/api/products/{id}` | Delete a product |

## Swagger UI

When running locally, Swagger UI is accessible at:
```
http://localhost:7071/api/swagger/ui
```

## Consumer-Based Response Shaping

Send an `X-Consumer-Key` header to control response shape:

- `X-Consumer-Key: cognition` → Full response (all fields)
- `X-Consumer-Key: infosys` → Limited response (Id, Name, Price, CategoryName)
- No header → Default (Limited response)

New consumers are added via configuration only — no code changes needed.

## JWT Authentication

Include a JWT Bearer token in the `Authorization` header:
```
Authorization: Bearer <your-token>
```

Configure JWT settings in `local.settings.json`:
```json
"JwtSettings": {
  "Authority": "https://login.microsoftonline.com/{tenant-id}",
  "Audience": "api://sample-api"
}
```

## Architecture

See [docs/architecture.md](docs/architecture.md) for detailed architecture documentation including:
- High-level architecture diagram
- Data flow
- Consumer-based response shaping flow
- Authentication flow
- How to extend the project
