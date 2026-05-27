# Architecture Documentation

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Azure Functions Host                      │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                   Middleware Pipeline                      │   │
│  │  ExceptionHandling → Authentication → ConsumerResolver    │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              SampleApi.Functions (API Layer)               │   │
│  │              ProductFunctions (HTTP Triggers)              │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              SampleApi.Services (Business Logic)           │   │
│  │         ProductService + ResponseShaperFactory             │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │            SampleApi.Repository (Data Access)              │   │
│  │         Repository<T> + ApplicationDbContext                │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │            SampleApi.Entities (Domain Models)              │   │
│  │              BaseEntity, Product, Category                 │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              SampleApi.DTOs (Data Transfer)                │   │
│  │    Requests, Responses (Full/Limited), ApiResponse<T>      │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────┴─────────┐
                    │    SQL Server DB    │
                    └────────────────────┘
```

## Project Descriptions

### SampleApi.Entities
Domain models and base entity classes. Contains the core data structures used throughout the application.

- `Base/BaseEntity.cs` — Abstract base with Id, audit fields (CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
- `Models/Product.cs` — Product entity with Name, Description, Price, CategoryId
- `Models/Category.cs` — Category entity with Name, Description, Products collection

### SampleApi.DTOs
Data Transfer Objects for API requests and responses. Separates API contracts from domain models.

- `Requests/CreateProductRequest.cs` — Validated request for creating products
- `Requests/UpdateProductRequest.cs` — Validated request for updating products
- `Responses/ProductResponseFull.cs` — Complete product response (all fields + audit info)
- `Responses/ProductResponseLimited.cs` — Minimal product response (Id, Name, Price, CategoryName)
- `Responses/ApiResponse<T>.cs` — Generic wrapper with Success, Message, Data properties

### SampleApi.Repository
Data access layer using Entity Framework Core with the generic repository pattern.

- `DbContext/ApplicationDbContext.cs` — EF Core context with fluent API configuration
- `Interfaces/IRepository<T>.cs` — Generic CRUD interface
- `Implementations/Repository<T>.cs` — Generic repository implementation

### SampleApi.Services
Business logic layer with consumer-based response shaping.

- `Interfaces/IProductService.cs` — Product service contract
- `Implementations/ProductService.cs` — Product service with entity-to-DTO mapping
- `ResponseShaping/` — Factory-based response shaping system

### SampleApi.Functions
Azure Functions v4 isolated worker API layer with middleware pipeline.

- `Controllers/ProductFunctions.cs` — HTTP triggers for CRUD + health check
- `Middleware/AuthenticationMiddleware.cs` — JWT token validation
- `Middleware/ExceptionHandlingMiddleware.cs` — Centralized error handling
- `Middleware/ConsumerResolverMiddleware.cs` — Consumer key extraction

## Data Flow

```
HTTP Request
    │
    ▼
ExceptionHandlingMiddleware (catches exceptions → error responses)
    │
    ▼
AuthenticationMiddleware (validates JWT → sets User principal)
    │
    ▼
ConsumerResolverMiddleware (extracts X-Consumer-Key or JWT claim → sets ConsumerKey)
    │
    ▼
ProductFunctions (HTTP trigger handler)
    │
    ▼
ProductService (business logic + response shaping)
    │
    ▼
Repository<Product> (EF Core data access)
    │
    ▼
ApplicationDbContext → SQL Server
```

## Consumer-Based Response Shaping

The response shaping system allows different API consumers to receive different response formats without code changes.

```
                    ┌────────────────────┐
                    │ ResponseShaperFactory│
                    └────────┬───────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
    ┌─────────────┐  ┌─────────────┐  ┌──────────┐
    │FullResponse │  │LimitedResp. │  │ (Future)  │
    │   Shaper    │  │   Shaper    │  │  Shaper   │
    └─────────────┘  └─────────────┘  └──────────┘
```

**Configuration (local.settings.json):**
```json
"ConsumerProfiles": {
  "infosys": { "ResponseLevel": "Limited" },
  "cognition": { "ResponseLevel": "Full" },
  "default": { "ResponseLevel": "Limited" }
}
```

## Authentication Flow

1. Client sends request with `Authorization: Bearer <token>` header
2. `AuthenticationMiddleware` extracts and validates the JWT token
3. If valid, the `ClaimsPrincipal` is stored in `context.Items["User"]`
4. `ConsumerResolverMiddleware` checks for `consumer_key` claim in the principal
5. If no JWT or no claim, falls back to `X-Consumer-Key` header, then `"default"`

## Project Dependency Diagram

```
SampleApi.Functions
    ├── SampleApi.Services
    │       ├── SampleApi.Repository
    │       │       └── SampleApi.Entities
    │       ├── SampleApi.DTOs
    │       │       └── SampleApi.Entities
    │       └── SampleApi.Entities
    ├── SampleApi.DTOs
    ├── SampleApi.Entities
    └── SampleApi.Repository
```

## How to Add a New Entity

1. Create the entity class in `src/SampleApi.Entities/Models/` inheriting from `BaseEntity`
2. Add a `DbSet<NewEntity>` to `ApplicationDbContext`
3. Add fluent API configuration in `OnModelCreating`
4. Create DTOs in `src/SampleApi.DTOs/Responses/` and `Requests/`
5. Create response shapers in `src/SampleApi.Services/ResponseShaping/`
6. Create a service interface and implementation in `src/SampleApi.Services/`
7. Create an Azure Functions controller in `src/SampleApi.Functions/Controllers/`
8. Register the new service and repository in `Program.cs`
9. Add unit tests for the new service, repository, and function

## How to Add a New Consumer

No code changes needed — only configuration:

1. Add an entry to `ConsumerProfiles` in `local.settings.json`:
   ```json
   "ConsumerProfiles": {
     "new-consumer": { "ResponseLevel": "Full" }
   }
   ```
2. The `ResponseShaperFactory` will automatically resolve the appropriate shaper

## How to Add a New Response Level

1. Create a new response DTO in `src/SampleApi.DTOs/Responses/`
2. Create a new shaper class in `src/SampleApi.Services/ResponseShaping/` implementing `IResponseShaper<T>`
3. Register the new response level in `ResponseShaperFactory.GetShaper()` method
4. Configure consumers to use the new level in `local.settings.json`
