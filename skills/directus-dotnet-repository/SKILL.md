---
name: directus-dotnet-repository
description: Create .NET repositories using Evanto.Directus.Client for Directus CMS data access. Covers filtering, pagination, CRUD operations, authentication providers, and project setup. Use when building repositories that interact with Directus collections.
license: Proprietary
metadata:
  author: evanto
  version: "1.0"
compatibility: Requires .NET 10.0+ and access to a Directus CMS instance. Check for Directus MCP server availability to access collection structure and data directly.
---

# Directus .NET Repository Skill

This skill guides you in creating .NET repositories using the `Evanto.Directus.Client` library for Directus CMS data access.

## Prerequisites

Before implementing repositories:

1. **Check for Directus MCP Server**: If a Directus MCP server is available in the current environment, use it to explore collection structures, field definitions, and sample data directly. This provides real-time access to the database schema.

2. **Reference the existing codebase**: Look at `evanto/directus/src/Evanto.Directus.Client/` for the client implementation and `lib/JM.Infrastructure/Directus/Repositories/` for repository examples.

## Adding Directus Client to a Project

### 1. Configure Settings

Create or update `appsettings.json`:

```json
{
  "Directus": {
    "BaseUrl": "http://localhost:8055",
    "User": "admin@example.com",
    "Password": "your-password",
    "AutoLogin": true,
    "CacheDirectory": "./cache/assets",
    "MaxCacheSizeGB": 5.0,
    "CacheRetentionDays": 30
  }
}
```

### 2. Register Services in Program.cs

```csharp
using Evanto.Directus.Client.Extensions;
using Evanto.Directus.Client.Settings;

// Bind settings from configuration
var directusSettings = builder.Configuration
    .GetSection("Directus")
    .Get<EvDirectusSettings>()!;

// Add Directus client with default secret provider
builder.Services.AddDirectusClient(directusSettings);

// OR with custom secret provider (e.g., claims-based for authenticated users)
builder.Services.AddDirectusClient(directusSettings, services =>
{
    services.AddScoped<IEvDirectusSecretProvider, JmDirectusClaimSecretProvider>();
});
```

### 3. Environment Variable Overrides

The client supports environment variable overrides:
- `DIRECTUS_URL` - Directus base URL
- `DIRECTUS_USER` - Service account email
- `DIRECTUS_PASSWORD` - Service account password

## Creating a Repository

### Basic Repository Structure

```csharp
using Evanto.Directus.Client.Contracts;
using Evanto.Directus.Client.Models;
using static Evanto.Directus.Client.Filters.EvDirectusFilter;

namespace YourNamespace.Repositories;

public class YourRepository(IEvDirectusClient directusClient) : IYourRepository
{
    private readonly IEvDirectusClient mDirectusClient = directusClient;
    private const String COLLECTION_NAME = "your_collection";

    // Repository methods here...
}
```

### Repository with Caching (Recommended Pattern)

```csharp
using Evanto.Directus.Client.Contracts;
using Evanto.Directus.Client.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Evanto.Directus.Client.Filters.EvDirectusFilter;

public partial class YourRepository(
    IEvDirectusClient directusClient,
    HybridCache cache,
    ILogger<YourRepository> logger) : IYourRepository
{
    private readonly IEvDirectusClient mDirectusClient = directusClient;
    private readonly HybridCache mCache = cache;

    private const String CACHE_KEY_PREFIX = "YourEntity";
    private static readonly TimeSpan sCacheExpiration = TimeSpan.FromHours(24);

    public async Task<IEnumerable<YourModel>> GetAllAsync(CancellationToken ct = default)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}_all";

        return await mCache.GetOrCreateAsync(
            cacheKey,
            async token => await FetchFromDirectusAsync(token),
            new HybridCacheEntryOptions
            {
                Expiration = sCacheExpiration,
                LocalCacheExpiration = TimeSpan.FromMinutes(30)
            },
            cancellationToken: ct
        ) ?? [];
    }

    private async Task<List<YourModel>> FetchFromDirectusAsync(CancellationToken ct)
    {
        var query = new EvQueryParameters()
            .WithFilter(Eq("status", "published"))
            .WithLimit(1000);

        var items = await mDirectusClient
            .GetItemsAsync<YourModel>(COLLECTION_NAME, query, ct);

        return items.ToList();
    }
}
```

## Filtering with EvDirectusFilter

Import the static filter helpers:

```csharp
using static Evanto.Directus.Client.Filters.EvDirectusFilter;
```

### Available Filter Operators

| Method | Operator | Description |
|--------|----------|-------------|
| `Eq(field, value)` | `_eq` | Equals |
| `Neq(field, value)` | `_neq` | Not equals |
| `Lt(field, value)` | `_lt` | Less than |
| `Lte(field, value)` | `_lte` | Less than or equal |
| `Gt(field, value)` | `_gt` | Greater than |
| `Gte(field, value)` | `_gte` | Greater than or equal |
| `Contains(field, value)` | `_contains` | Contains (case-sensitive) |
| `IContains(field, value)` | `_icontains` | Contains (case-insensitive) |
| `StartsWith(field, value)` | `_starts_with` | Starts with |
| `EndsWith(field, value)` | `_ends_with` | Ends with |
| `In(field, ...values)` | `_in` | In array of values |
| `Nin(field, ...values)` | `_nin` | Not in array of values |
| `Between(field, from, to)` | `_between` | Between two values |
| `Null(field)` | `_null` | Is null |
| `NotNull(field)` | `_nnull` | Is not null |
| `Empty(field)` | `_empty` | Is empty |
| `NotEmpty(field)` | `_nempty` | Is not empty |

### Logical Operators

| Method | Description |
|--------|-------------|
| `And(...filters)` | Logical AND |
| `Or(...filters)` | Logical OR |

### Filter Examples

```csharp
// Simple equality filter
var filter = Eq("status", "published");

// Multiple conditions with AND
var filter = And(
    Eq("status", "published"),
    Eq("country", "DE"),
    Lte("valid_from", "2025-01-01")
);

// OR conditions
var filter = Or(
    Eq("category", "karate"),
    Eq("category", "judo")
);

// IN operator for multiple values
var filter = In("status", "published", "draft");

// Nested AND/OR
var filter = And(
    Eq("status", "published"),
    Or(
        Eq("language", "de"),
        Eq("language", "en")
    )
);

// Relational field filtering (dot notation)
var filter = Eq("author.name", "John Doe");

// Apply filter to query
var query = new EvQueryParameters()
    .WithFilter(filter)
    .WithLimit(100);
```

## Query Parameters

### EvQueryParameters Methods

```csharp
var query = new EvQueryParameters()
    .WithFields("id", "title", "status")           // Select specific fields
    .WithFields("*,author.*")                       // All fields + related
    .WithFilter(filter)                             // Apply filter
    .WithLimit(50)                                  // Limit results
    .WithOffset(100)                                // Skip results
    .WithSort("-date_created")                      // Sort (- for descending)
    .WithSearch("keyword")                          // Full-text search
    .WithMeta(true)                                 // Include metadata
    .WithPagination(limit: 12, offset: 0);          // Pagination helper
```

## CRUD Operations

### Get Items (List)

```csharp
// Simple list
var items = await mDirectusClient
    .GetItemsAsync<YourModel>(COLLECTION_NAME, null, ct);

// With query parameters
var query = new EvQueryParameters()
    .WithFilter(Eq("status", "published"))
    .WithLimit(100)
    .WithSort("-date_created");

var items = await mDirectusClient
    .GetItemsAsync<YourModel>(COLLECTION_NAME, query, ct);
```

### Get Single Item

```csharp
// By ID (Int32)
var item = await mDirectusClient
    .GetItemAsync<YourModel>(COLLECTION_NAME, 123, ct);

// By ID (Guid)
var item = await mDirectusClient
    .GetItemAsync<YourModel>(COLLECTION_NAME, guidId, ct);

// By ID with query parameters (for related fields)
var query = new EvQueryParameters()
    .WithFields("*,related_field.*");

var item = await mDirectusClient
    .GetItemAsync<YourModel>(COLLECTION_NAME, guidId, query, ct);
```

### Get Item by Slug (or other unique field)

```csharp
public async Task<YourModel?> GetBySlugAsync(String slug, CancellationToken ct)
{
    var filter = And(
        In("status", "published", "draft"),
        Eq("slug", slug)
    );

    var query = new EvQueryParameters()
        .WithLimit(1)
        .WithFields("*,related.*")
        .WithFilter(filter);

    var items = await mDirectusClient
        .GetItemsAsync<YourModel>(COLLECTION_NAME, query, ct);

    return items.FirstOrDefault();
}
```

### Create Item

```csharp
// Returns Int32 ID
var intId = await mDirectusClient
    .CreateItemAsync<YourModel>(COLLECTION_NAME, newItem, ct);

// Returns Guid ID (for UUID primary keys)
var guidId = await mDirectusClient
    .CreateItemAsync<YourModel>(COLLECTION_NAME, newItem, true, ct);
```

### Update Item

```csharp
// By Int32 ID
var updated = await mDirectusClient
    .UpdateItemAsync(COLLECTION_NAME, 123, updatedItem, ct);

// By Guid ID
var updated = await mDirectusClient
    .UpdateItemAsync(COLLECTION_NAME, guidId, updatedItem, ct);
```

### Delete Item

```csharp
// By Int32 ID
await mDirectusClient.DeleteItemAsync(COLLECTION_NAME, 123, ct);

// By Guid ID
await mDirectusClient.DeleteItemAsync(COLLECTION_NAME, guidId, ct);
```

## Pagination

### Using GetItemsPagedAsync

```csharp
public async Task<EvPagedResult<YourModel>> GetPagedAsync(
    Int32 page = 1,
    Int32 pageSize = 12,
    CancellationToken ct = default)
{
    var filter = Eq("status", "published");

    // Calculate offset from page number
    var offset = (page - 1) * pageSize;

    var query = new EvQueryParameters
    {
        Fields = "*,related.*"
    }
    .WithFilter(filter)
    .WithPagination(pageSize, offset);  // Sets limit, offset, and meta=true

    return await mDirectusClient
        .GetItemsPagedAsync<YourModel>(COLLECTION_NAME, query, ct);
}
```

### Working with EvPagedResult

```csharp
var result = await GetPagedAsync(page: 2, pageSize: 12);

// Access data
var items = result.Data;              // IEnumerable<T>
var count = result.Count;             // Items in current page
var total = result.TotalCount;        // Total matching items

// Pagination helpers
var totalPages = result.TotalPages(pageSize);          // Total pages
var currentPage = result.CurrentPage(offset, pageSize); // Current page (1-based)
var hasMore = result.HasMore(offset, pageSize);         // More items available?
```

## Authentication Providers

### EvDirectusBasicSecretProvider (Default)

Stores tokens in memory. Use for single technical user scenarios:

```csharp
builder.Services.AddDirectusClient(settings);
// Automatically registers EvDirectusBasicSecretProvider
```

### JmDirectusClaimSecretProvider (Claims-Based)

Stores tokens in user claims. Use when users authenticate via Directus and you want per-user tokens:

```csharp
builder.Services.AddDirectusClient(settings, services =>
{
    services.AddScoped<IEvDirectusSecretProvider, JmDirectusClaimSecretProvider>();
});
```

This provider:
- Retrieves access/refresh tokens from user claims
- Falls back to service credentials if user not authenticated
- Updates authentication cookies when tokens are refreshed

### Custom Secret Provider

Implement `IEvDirectusSecretProvider`:

```csharp
public interface IEvDirectusSecretProvider
{
    Task<(String Email, String Password)> GetServiceCredentialsAsync(CancellationToken ct);
    Task<String> GetAccessTokenAsync(CancellationToken ct);
    Task<String?> GetRefreshTokenAsync(CancellationToken ct);
    Task SaveTokensAsync(String accessToken, String? refreshToken, CancellationToken ct);
}
```

## Model Definition

Define models with `[JsonPropertyName]` attributes matching Directus field names:

```csharp
using System.Text.Json.Serialization;

public class YourModel
{
    [JsonPropertyName("id")]
    public Guid ID { get; set; }

    [JsonPropertyName("status")]
    public String Status { get; set; } = "draft";

    [JsonPropertyName("date_created")]
    public DateTime? DateCreated { get; set; }

    [JsonPropertyName("date_updated")]
    public DateTime? DateUpdated { get; set; }

    [JsonPropertyName("title")]
    public String Title { get; set; } = String.Empty;

    [JsonPropertyName("slug")]
    public String Slug { get; set; } = String.Empty;

    // Related field (M2O)
    [JsonPropertyName("author")]
    public AuthorModel? Author { get; set; }

    // Related collection (O2M or M2M)
    [JsonPropertyName("items")]
    public IEnumerable<RelatedModel> Items { get; set; } = [];
}
```

## Interface Definition

Define repository interfaces in the Application layer:

```csharp
namespace YourNamespace.Contracts;

public interface IYourRepository
{
    Task<IEnumerable<YourModel>> GetAllAsync(CancellationToken ct = default);
    Task<EvPagedResult<YourModel>> GetPagedAsync(Int32 page, Int32 pageSize, CancellationToken ct = default);
    Task<YourModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<YourModel?> GetBySlugAsync(String slug, CancellationToken ct = default);
    Task<Guid?> CreateAsync(YourModel item, CancellationToken ct = default);
    Task<YourModel?> UpdateAsync(YourModel item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

## Logging with LoggerMessage

Use source-generated logging for better performance:

```csharp
public partial class YourRepository
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Loaded {Count} items from {Collection}")]
    private partial void LogItemsLoaded(Int32 count, String collection);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to fetch items from {Collection}")]
    private partial void LogFetchError(Exception ex, String collection);
}
```

## Additional Resources

See reference files for complete examples:
- [Repository Example](references/REPOSITORY_EXAMPLE.md) - Complete repository implementation
- [Filter Examples](references/FILTER_EXAMPLES.md) - Advanced filtering patterns
- [Pagination Example](references/PAGINATION_EXAMPLE.md) - Pagination implementation
