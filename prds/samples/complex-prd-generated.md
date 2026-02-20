name: "Asset Proxy and Cache Implementation PRD"
description: |

## Purpose
Complete implementation of asset proxy and caching system for Directus CMS to reduce load, improve performance, and provide better security control over asset delivery.

## Core Principles
1. **Context is King**: All necessary documentation, examples, and caveats included
2. **Validation Loops**: Executable tests/lints for iterative refinement  
3. **Information Dense**: Uses existing codebase patterns and keywords
4. **Progressive Success**: Start simple, validate, then enhance
5. **Global rules**: Follow all rules in CLAUDE.md and CodingRules.md

---

## Goal
Implementation of a proxy endpoint and cache system for assets from headless CMS Directus, providing file caching, image transformations, and cache management capabilities.

## Why
- Currently images and downloads (assets) require direct access to Directus /assets endpoint
- This exposes the headless CMS publicly and increases server load
- Most assets don't change frequently, making caching highly effective
- Provides centralized control over asset delivery and security
- Enables optimization and transformation caching for better performance

## What
A complete asset proxy system with the following user-visible behavior:

### Endpoints
- `GET /assets/{uuid}?format=webp&width=300&height=200` - Proxy endpoint with Directus parameter support
- `GET /assets/{uuid}_{width}x{height}.{format}` - Direct cached asset access
- `GET /assets/{uuid}` - Original asset without transformations
- `DELETE /clear-asset-cache` - Clear entire asset cache (admin only)
- `DELETE /clear-asset-cache/{uuid}` - Clear specific asset in all variations

### Technical Requirements
- File-based caching in configurable directory
- Support for both images (webp, jpg, png, gif) and binary files (PDF, etc.)
- MIME type detection and appropriate file extensions
- Concurrent request handling with proper locking
- Comprehensive logging and error handling
- Memory-efficient streaming for large files

### Success Criteria
- [ ] Assets are served from cache when available (< 50ms response time)
- [ ] Cache miss triggers Directus fetch and local storage (< 3s total)
- [ ] All Directus asset transformation parameters supported
- [ ] Cache clearing works for individual and bulk operations
- [ ] Proper HTTP status codes and error handling
- [ ] No memory leaks or resource exhaustion under load
- [ ] Follows all existing codebase patterns and conventions

## All Needed Context

### Documentation & References
```yaml
# MUST READ - Include these in your context window
- url: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-9.0
  why: IHttpClientFactory patterns and best practices for HTTP requests
  
- url: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-9.0
  why: Response caching strategies and HTTP cache headers
  
- url: https://stackoverflow.com/questions/67401835/c-sharp-net-core-web-api-proxy-file-download
  why: .NET Core file download proxy implementation examples
  
- file: /Users/svk/dev/kunden/srs/lib/Evanto.Directus.Client/EvDirectusClient.cs
  why: Existing HTTP client patterns, error handling, and authentication
  
- file: /Users/svk/dev/kunden/srs/lib/Evanto.Directus.Client/Extensions/EvDirectusExtensions.cs
  why: Service registration pattern to follow for dependency injection
  
- file: /Users/svk/dev/kunden/srs/web/my-system/Program.cs
  why: Service configuration and middleware setup patterns
  
- file: /Users/svk/dev/kunden/srs/CodingRules.md
  why: CRITICAL - All C# coding conventions must be followed exactly
  
- file: /Users/svk/dev/kunden/srs/Areas/Identity/Pages/Account/Manage/DownloadPersonalData.cshtml.cs
  why: FileContentResult pattern for serving downloadable files (lines 63-64)
```

### Current Codebase Tree
```bash
lib/Evanto.Directus.Client/
├── Contracts/IEvDirectusClient.cs
├── EvDirectusClient.cs  
├── Extensions/EvDirectusExtensions.cs
├── Models/[EvDirectusException.cs, EvItemsResponse.cs, etc.]
└── Settings/EvDirectusSettings.cs

web/my-system/
├── Program.cs
├── Pages/[various razor pages with [slug].cshtml pattern]
└── Models/SrsWebPageModel.cs
```

### Desired Codebase Tree with New Files
```bash
lib/Evanto.Directus.Client/
├── Contracts/
│   ├── IEvDirectusClient.cs
│   └── IEvAssetProxyService.cs          # NEW - Asset proxy service interface
├── Services/
│   └── EvAssetProxyService.cs           # NEW - Main asset proxy implementation  
├── Models/
│   ├── [existing models...]
│   ├── EvAssetCacheInfo.cs             # NEW - Cache metadata model
│   └── EvAssetRequest.cs               # NEW - Asset request parsing model
├── Settings/
│   └── EvDirectusSettings.cs           # MODIFY - Add cache settings
└── Extensions/
    └── EvDirectusExtensions.cs         # MODIFY - Register new service

web/my-system/
├── Program.cs                          # MODIFY - Configure asset proxy endpoint
└── Pages/
    └── Assets.cshtml.cs                # NEW - Asset proxy endpoint handler
```

### Known Gotchas & Library Quirks
```csharp
// CRITICAL: IHttpClientFactory requires proper disposal patterns
// Use 'using var client = mHttpClientFactory.CreateClient()' not field storage

// CRITICAL: File I/O requires proper locking for concurrent access
// Use ReaderWriterLockSlim or SemaphoreSlim for cache file operations

// CRITICAL: Large file streaming - use Stream.CopyToAsync, not ReadAllBytes
// Memory usage will explode with large PDFs if loaded entirely into memory

// CRITICAL: Directus asset URLs support these query parameters:
// ?format=webp&width=300&height=200&quality=80&fit=cover
// Must preserve all parameters when caching and use in filename

// CRITICAL: MIME type detection must handle edge cases
// Use both file extension AND HTTP Content-Type header for reliability

// CRITICAL: Follow CodingRules.md exactly:
// - Member variables with 'm' prefix (mHttpClientFactory)  
// - Vertical alignment of parameters and assignments
// - Guard clauses for all parameters using ArgumentNullException.ThrowIfNull()
// - Comments above all methods with current date
// - Try-catch blocks with proper logging
```

## Implementation Blueprint

### Data Models and Structure

Core data models to ensure type safety and consistency:
```csharp
// Cache metadata tracking
public class EvAssetCacheInfo
{
    public String     AssetId       { get; set; } = String.Empty;
    public String     OriginalUrl   { get; set; } = String.Empty; 
    public String     CachedPath    { get; set; } = String.Empty;
    public String     ContentType   { get; set; } = String.Empty;
    public Int64      FileSize      { get; set; }
    public DateTime   CachedAt      { get; set; }
    public DateTime   LastAccessed  { get; set; }
}

// Asset request parsing
public class EvAssetRequest  
{
    public String   AssetId    { get; set; } = String.Empty;
    public String?  Format     { get; set; }
    public Int32?   Width      { get; set; }
    public Int32?   Height     { get; set; }
    public Int32?   Quality    { get; set; }
    public String?  Fit        { get; set; }
}
```

### List of Tasks (Implementation Order)

```yaml
Task 1: Extend EvDirectusSettings
  MODIFY lib/Evanto.Directus.Client/Settings/EvDirectusSettings.cs:
    - ADD properties for cache configuration
    - ADD CacheDirectory, MaxCacheSizeGB, CacheRetentionDays
    - PRESERVE existing properties and formatting

Task 2: Create Asset Request Model  
  CREATE lib/Evanto.Directus.Client/Models/EvAssetRequest.cs:
    - MIRROR pattern from existing models in same directory
    - IMPLEMENT parsing from URL query parameters and path segments
    - ADD validation for supported formats and dimensions

Task 3: Create Asset Cache Info Model
  CREATE lib/Evanto.Directus.Client/Models/EvAssetCacheInfo.cs:
    - MIRROR pattern from existing models 
    - IMPLEMENT JSON serialization attributes
    - ADD methods for cache file path generation

Task 4: Create Asset Proxy Service Interface
  CREATE lib/Evanto.Directus.Client/Contracts/IEvAssetProxyService.cs:
    - MIRROR pattern from IEvDirectusClient.cs
    - DEFINE methods: GetAssetAsync, ClearCacheAsync, ClearAssetCacheAsync
    - USE async Task<Stream> return type for file streaming

Task 5: Implement Asset Proxy Service
  CREATE lib/Evanto.Directus.Client/Services/EvAssetProxyService.cs:
    - MIRROR class structure from EvDirectusClient.cs  
    - IMPLEMENT IEvAssetProxyService interface
    - USE IHttpClientFactory pattern from existing client
    - ADD file system operations with proper locking
    - IMPLEMENT cache hit/miss logic with logging

Task 6: Extend Service Registration
  MODIFY lib/Evanto.Directus.Client/Extensions/EvDirectusExtensions.cs:
    - ADD asset proxy service registration to AddDirectusClient method
    - PRESERVE existing registration patterns
    - ADD validation for cache directory settings

Task 7: Create Asset Proxy Razor Page  
  CREATE web/my-system/Pages/Assets.cshtml:
    - MIRROR minimal content from other pages
    - HANDLE both GET asset and DELETE cache operations

Task 8: Implement Asset Proxy Page Model
  CREATE web/my-system/Pages/Assets.cshtml.cs:
    - MIRROR pattern from SrsWebPageModel.cs
    - IMPLEMENT OnGetAsync for asset serving
    - IMPLEMENT OnDeleteAsync for cache clearing  
    - USE proper FileStreamResult for file serving
    - ADD authorization for cache clearing operations

Task 9: Configure Asset Proxy Routing
  MODIFY web/my-system/Program.cs:
    - ADD endpoint mapping for /assets route before MapRazorPages
    - PRESERVE existing middleware order
    - ENSURE static assets don't conflict with proxy

Task 10: Add Comprehensive Unit Tests
  CREATE tests/Evanto.Directus.Client.Tests/EvAssetProxyServiceTests.cs:
    - MIRROR pattern from existing test files  
    - TEST cache hit/miss scenarios
    - TEST concurrent access scenarios
    - TEST error conditions and edge cases
    - MOCK IHttpClientFactory and file system operations
```

### Per Task Pseudocode

#### Task 5: Asset Proxy Service Implementation (Critical Core Logic)
```csharp
public class EvAssetProxyService : IEvAssetProxyService
{
    // PATTERN: Member variables with m prefix, aligned formatting  
    private readonly IHttpClientFactory         mHttpClientFactory;
    private readonly IEvDirectusClient          mDirectusClient;  
    private readonly ILogger<EvAssetProxyService> mLogger;
    private readonly EvDirectusSettings         mSettings;
    private readonly ReaderWriterLockSlim       mCacheLock;
    private readonly String                     mCacheDirectory;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets an asset from cache or fetches from Directus. </summary>
    ///
    /// <remarks>   SvK, 22.07.2025. </remarks>
    ///
    /// <param name="assetRequest">       The asset request with transformations. </param>
    /// <param name="cancellationToken"> Cancellation token. </param>
    ///
    /// <returns>   Stream containing the asset data. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Stream> GetAssetAsync(
        EvAssetRequest    assetRequest,
        CancellationToken cancellationToken = default)
    {
        // PATTERN: Guard clauses first
        ArgumentNullException.ThrowIfNull(assetRequest);
        ArgumentException.ThrowIfNullOrEmpty(assetRequest.AssetId);

        // PATTERN: Try-catch with logging
        try  
        {
            var cacheKey  = GenerateCacheKey(assetRequest);
            var cachePath = Path.Combine(mCacheDirectory, cacheKey);

            // PATTERN: Use read lock for cache check
            mCacheLock.EnterReadLock();
            try
            {
                if (File.Exists(cachePath))
                {   // Cache hit - return file stream
                    mLogger.LogDebug("Cache hit for asset: {AssetId}", assetRequest.AssetId);
                    return File.OpenRead(cachePath);
                }
            }
            finally
            {
                mCacheLock.ExitReadLock();
            }

            // Cache miss - fetch from Directus
            mLogger.LogDebug("Cache miss for asset: {AssetId}", assetRequest.AssetId);
            
            using var httpClient = mHttpClientFactory.CreateClient();
            var directusUrl      = BuildDirectusAssetUrl(assetRequest);
            
            var response = await httpClient.GetAsync(directusUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            // PATTERN: Use write lock for cache storage
            mCacheLock.EnterWriteLock();
            try  
            {   // Download and cache the asset
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                
                using var fileStream = File.Create(cachePath);
                await response.Content.CopyToAsync(fileStream, cancellationToken);
                
                mLogger.LogInformation("✅ Cached asset: {AssetId} to {CachePath}", 
                    assetRequest.AssetId, cachePath);
            }
            finally
            {
                mCacheLock.ExitWriteLock(); 
            }

            // Return fresh file stream
            return File.OpenRead(cachePath);
        }
        catch (Exception ex)
        {
            mLogger.LogError(ex, "❌ Error getting asset: {AssetId}", assetRequest.AssetId);
            throw;
        }
    }
}
```

## Validation Loop

### Level 1: Build, Style & Static Analysis

Run these first—fix any errors or style violations before writing tests.

1. Restore packages
```bash
dotnet restore
```

2. Build with warnings as errors
```bash
dotnet build --no-restore --warnaserror
```

3. Apply and verify code style
```bash
dotnet format ./my-system.sln --verify-no-changes
```

Expected:
- Build returns exit 0 with no warnings
- dotnet format returns exit 0

### Level 2: Unit Tests

Create comprehensive tests covering cache hit/miss, concurrent access, and error scenarios.

```bash
dotnet test --no-build
```

Expected:
- All tests pass including new EvAssetProxyServiceTests
- Coverage > 80% on new asset proxy functionality

### Level 3: Manual Integration Tests

Test the complete flow with real asset requests:

```bash
# Test asset serving  
curl -v http://localhost:5000/assets/0e28376d-ae8e-4efc-9b02-2d53006c3216?format=webp&width=300&height=200

# Test cache clearing (requires authentication)
curl -X DELETE http://localhost:5000/clear-asset-cache/0e28376d-ae8e-4efc-9b02-2d53006c3216

# Verify cache directory structure
ls -la ./cache/assets/
```

Expected:
- First request fetches from Directus (slower ~2-3s)
- Second identical request serves from cache (faster <50ms)
- Cache files exist with correct naming pattern
- Cache clearing removes relevant files

## Final Validation Checklist
- [ ] All tests pass: `dotnet test --no-build`
- [ ] No errors: `dotnet build --no-restore --warnaserror`
- [ ] Manual asset serving works: `curl http://localhost:5000/assets/{uuid}`
- [ ] Cache hit performance < 50ms response time
- [ ] Cache miss creates files in cache directory
- [ ] Error cases return appropriate HTTP status codes
- [ ] Logs are informative but not verbose
- [ ] Memory usage stable under concurrent load
- [ ] All CodingRules.md conventions followed

---

## Anti-Patterns to Avoid
- ❌ Don't load entire files into memory - use streaming
- ❌ Don't use HttpClient as singleton - use IHttpClientFactory  
- ❌ Don't ignore concurrent access - implement proper locking
- ❌ Don't hardcode paths - use configuration settings
- ❌ Don't ignore MIME types - detect and validate properly
- ❌ Don't skip error logging - comprehensive error tracking needed
- ❌ Don't deviate from existing patterns - follow codebase conventions
- ❌ Don't forget authentication on admin endpoints

**PRP Confidence Score: 9/10** - Comprehensive context, clear implementation path, executable validation, and follows established patterns. High confidence for one-pass success.