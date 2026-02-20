# Complete Repository Example

This example shows a complete repository implementation with caching, filtering, and CRUD operations.

## JmVatRepository - Simple Read-Only Repository

```csharp
using Evanto.Directus.Client.Contracts;
using Evanto.Directus.Client.Models;
using Prefix.Application.Contracts;
using Prefix.Domain.Constants;
using Prefix.Domain.Models.Shop;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Evanto.Directus.Client.Filters.EvDirectusFilter;

namespace Prefix.Infrastructure.Directus.Repositories;

public partial class JmVatRepository(
    IEvDirectusClient directusClient,
    HybridCache cache,
    ILogger<JmVatRepository> logger) : IJmVatRepository
{
    private readonly IEvDirectusClient mDirectusClient = directusClient;
    private readonly HybridCache mCache = cache;

    private const String CACHE_KEY_PREFIX = "JmVat";
    private static readonly TimeSpan sCacheExpiration = TimeSpan.FromHours(24);

    public async Task<IEnumerable<JmVat>> GetVatRatesAsync(
        String country,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        var cacheKey = $"{CACHE_KEY_PREFIX}_{country}_{date:yyyy-MM-dd}";

        try
        {
            return await mCache.GetOrCreateAsync(
                cacheKey,
                async token => await FetchVatRatesFromDirectusAsync(country, date, token),
                new HybridCacheEntryOptions
                {
                    Expiration = sCacheExpiration,
                    LocalCacheExpiration = TimeSpan.FromMinutes(30)
                },
                cancellationToken: cancellationToken
            ) ?? [];
        }
        catch (Exception ex)
        {
            LogErrorFetchingVatRates(ex, country);
            return [];
        }
    }

    private async Task<List<JmVat>> FetchVatRatesFromDirectusAsync(
        String country,
        DateTime date,
        CancellationToken cancellationToken)
    {
        var dateString = date.ToString("yyyy-MM-dd");

        // Build filter: status = published AND country = {country} AND valid_from <= {date}
        var filter = And(
            Eq("status", "published"),
            Eq("country", country),
            Lte("valid_from", dateString)
        );

        var query = new EvQueryParameters()
            .WithFilter(filter)
            .WithLimit(100);

        var vatRates = await mDirectusClient
            .GetItemsAsync<JmVat>(JmConstants.VAT_COLLECTION, query, cancellationToken);

        // Filter in code for complex logic (null OR >= date)
        var validRates = vatRates
            .Where(r => r.IsValid)
            .ToList();

        LogVatRatesLoaded(country, validRates.Count);

        return validRates;
    }

    #region LoggerMessages

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to fetch VAT rates for country {Country}")]
    private partial void LogErrorFetchingVatRates(Exception ex, String country);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Loaded {Count} VAT rates for country {Country}")]
    private partial void LogVatRatesLoaded(String country, Int32 count);

    #endregion
}
```

## JmVideoRepository - Full CRUD Repository with Pagination

```csharp
using Evanto.Directus.Client.Contracts;
using Evanto.Directus.Client.Models;
using Prefix.Application.Contracts;
using Prefix.Domain.Constants;
using Prefix.Domain.Models.Cms;
using static Evanto.Directus.Client.Filters.EvDirectusFilter;

namespace Prefix.Infrastructure.Directus.Repositories;

public class JmVideoRepository(IEvDirectusClient directusClient) : IJmVideoRepository
{
    private readonly IEvDirectusClient mDirectusClient = directusClient;

    #region Read Operations

    public async Task<IEnumerable<JmVideo>> GetVideosAsync(
        String language,
        String? category = null,
        String? level = null,
        String status = "published",
        Int32 limit = 1000,
        CancellationToken cancellationToken = default)
    {
        // Build filter dictionary for simple equality filters
        var filter = new Dictionary<String, String>
        {
            { "status", status }
        };

        if (!String.IsNullOrEmpty(language) && language != "all")
            filter.Add("language", language);

        if (!String.IsNullOrWhiteSpace(category) && category != "all")
            filter.Add("category", category);

        if (!String.IsNullOrWhiteSpace(level) && level != "all")
            filter.Add("level", level);

        var query = new EvQueryParameters
        {
            Filter = filter,
            Limit = limit,
            Fields = "*,membership_plan.*"
        };

        return await mDirectusClient
            .GetItemsAsync<JmVideo>(JmConstants.VIDEOS_COLLECTION, query, cancellationToken);
    }

    public async Task<EvPagedResult<JmVideo>> GetVideosPagedAsync(
        String language,
        String? category = null,
        String? level = null,
        String status = "published",
        Int32 page = 1,
        Int32 pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var filter = new Dictionary<String, String>
        {
            { "status", status }
        };

        if (!String.IsNullOrEmpty(language) && language != "all")
            filter.Add("language", language);

        if (!String.IsNullOrWhiteSpace(category) && category != "all")
            filter.Add("category", category);

        if (!String.IsNullOrWhiteSpace(level) && level != "all")
            filter.Add("level", level);

        // Calculate offset from page number
        var offset = (page - 1) * pageSize;

        var query = new EvQueryParameters
        {
            Filter = filter,
            Fields = "*,membership_plan.*"
        }.WithPagination(pageSize, offset);

        return await mDirectusClient
            .GetItemsPagedAsync<JmVideo>(JmConstants.VIDEOS_COLLECTION, query, cancellationToken);
    }

    public async Task<JmVideo?> GetVideoBySlugAsync(String slug, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(slug);

        // Using advanced filter with static helpers
        var filter = And(
            In("status", "published", "draft"),
            Eq("slug", slug)
        );

        var query = new EvQueryParameters()
            .WithLimit(1)
            .WithFields("*,membership_plan.*")
            .WithFilter(filter);

        var videos = await mDirectusClient
            .GetItemsAsync<JmVideo>(JmConstants.VIDEOS_COLLECTION, query, cancellationToken);

        return videos.FirstOrDefault();
    }

    public async Task<JmVideo?> GetVideoByIdAsync(Guid videoID, CancellationToken cancellationToken = default)
    {
        var query = new EvQueryParameters()
            .WithFields("*,playlists.*,membership_plan.*");

        return await mDirectusClient
            .GetItemAsync<JmVideo>(JmConstants.VIDEOS_COLLECTION, videoID, query, cancellationToken);
    }

    #endregion

    #region Write Operations

    public async Task<Guid?> CreateVideoAsync(JmVideo video, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(video);

        return await mDirectusClient
            .CreateItemAsync<JmVideo>(JmConstants.VIDEOS_COLLECTION, video, true, cancellationToken);
    }

    public async Task<JmVideo?> UpdateVideoAsync(JmVideo video, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(video);

        return await mDirectusClient
            .UpdateItemAsync(JmConstants.VIDEOS_COLLECTION, video.ID, video, cancellationToken);
    }

    public async Task DeleteVideoAsync(Guid videoID, CancellationToken cancellationToken = default)
    {
        await mDirectusClient
            .DeleteItemAsync(JmConstants.VIDEOS_COLLECTION, videoID, cancellationToken);
    }

    #endregion
}
```

## Key Patterns

### 1. Constructor Injection
Always inject `IEvDirectusClient` via constructor. Add `HybridCache` and `ILogger` for caching and logging.

### 2. Collection Constants
Define collection names as constants in a shared constants class:

```csharp
public static class JmConstants
{
    public const String VIDEOS_COLLECTION = "jm_videos";
    public const String VAT_COLLECTION = "jm_vat";
}
```

### 3. Filter Approaches

**Simple Dictionary** - For basic equality filters:
```csharp
var filter = new Dictionary<String, String>
{
    { "status", "published" },
    { "language", "de" }
};
```

**Advanced Filter Tree** - For complex conditions:
```csharp
using static Evanto.Directus.Client.Filters.EvDirectusFilter;

var filter = And(
    Eq("status", "published"),
    Or(
        Eq("language", "de"),
        Eq("language", "en")
    ),
    Gte("date_published", "2025-01-01")
);
```

### 4. Related Fields
Use dot notation in `WithFields()` to include related data:

```csharp
.WithFields("*,author.*")                    // All fields + author relation
.WithFields("*,items.*.related_item.*")      // Nested relations
.WithFields("id,title,author.name")          // Specific fields only
```

### 5. Error Handling
Wrap Directus calls in try-catch and return empty collections on failure:

```csharp
try
{
    return await mDirectusClient.GetItemsAsync<T>(...);
}
catch (Exception ex)
{
    LogError(ex);
    return [];
}
```
