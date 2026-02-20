# Pagination Examples

This reference shows how to implement pagination with the Directus client.

## EvPagedResult Structure

The `EvPagedResult<T>` class contains:

```csharp
public class EvPagedResult<T>
{
    public IEnumerable<T> Data { get; set; }     // The items
    public EvPaginationMeta? Meta { get; set; }  // Pagination metadata

    // Computed properties
    public Int32 TotalCount { get; }             // Total matching items
    public Int32 Count { get; }                  // Items in current page

    // Helper methods
    public Boolean HasMore(Int32 currentOffset, Int32 pageSize);
    public Int32 TotalPages(Int32 pageSize);
    public Int32 CurrentPage(Int32 currentOffset, Int32 pageSize);
}
```

## Basic Pagination

### Repository Method

```csharp
public async Task<EvPagedResult<JmVideo>> GetVideosPagedAsync(
    String language,
    String? category = null,
    String status = "published",
    Int32 page = 1,
    Int32 pageSize = 12,
    CancellationToken cancellationToken = default)
{
    // Build filter
    var filters = new List<DFilter> { Eq("status", status) };

    if (!String.IsNullOrEmpty(language) && language != "all")
        filters.Add(Eq("language", language));

    if (!String.IsNullOrWhiteSpace(category) && category != "all")
        filters.Add(Eq("category", category));

    var filter = filters.Count == 1
        ? filters[0]
        : And(filters.ToArray());

    // Calculate offset from page number (1-based)
    var offset = (page - 1) * pageSize;

    var query = new EvQueryParameters()
        .WithFields("*,thumbnail.*")
        .WithFilter(filter)
        .WithSort("-date_created")
        .WithPagination(pageSize, offset);  // Sets limit, offset, meta=true

    return await mDirectusClient
        .GetItemsPagedAsync<JmVideo>(COLLECTION, query, cancellationToken);
}
```

### Using the Result

```csharp
// In a service or controller
var result = await repository.GetVideosPagedAsync(
    language: "de",
    category: "karate",
    page: 2,
    pageSize: 12
);

// Access the data
var videos = result.Data;
var totalVideos = result.TotalCount;
var videosOnThisPage = result.Count;

// Calculate pagination info
var currentPage = 2;
var pageSize = 12;
var offset = (currentPage - 1) * pageSize;  // 12

var totalPages = result.TotalPages(pageSize);           // e.g., 5
var hasNextPage = result.HasMore(offset, pageSize);     // true if more pages
var displayPage = result.CurrentPage(offset, pageSize); // 2
```

## Page Model for Views

### ViewModel

```csharp
public class VideoListViewModel
{
    public IEnumerable<JmVideo> Videos { get; set; } = [];
    public Int32 CurrentPage { get; set; } = 1;
    public Int32 PageSize { get; set; } = 12;
    public Int32 TotalItems { get; set; }
    public Int32 TotalPages { get; set; }
    public Boolean HasPreviousPage => CurrentPage > 1;
    public Boolean HasNextPage => CurrentPage < TotalPages;

    // Filters
    public String? Language { get; set; }
    public String? Category { get; set; }
}
```

### Page Handler

```csharp
public class VideosModel : PageModel
{
    private readonly IJmVideoRepository _repository;

    [BindProperty(SupportsGet = true)]
    public Int32 Page { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public String? Category { get; set; }

    public VideoListViewModel ViewModel { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        const Int32 pageSize = 12;
        var language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        var result = await _repository.GetVideosPagedAsync(
            language: language,
            category: Category,
            page: Page,
            pageSize: pageSize,
            cancellationToken: ct
        );

        ViewModel = new VideoListViewModel
        {
            Videos = result.Data,
            CurrentPage = Page,
            PageSize = pageSize,
            TotalItems = result.TotalCount,
            TotalPages = result.TotalPages(pageSize),
            Language = language,
            Category = Category
        };
    }
}
```

## Infinite Scroll / Load More

### Repository with Offset-Based Loading

```csharp
public async Task<EvPagedResult<JmVideo>> GetVideosWithOffsetAsync(
    Int32 skip,
    Int32 take,
    String? category = null,
    CancellationToken ct = default)
{
    var filter = Eq("status", "published");

    if (!String.IsNullOrWhiteSpace(category))
    {
        filter = And(
            Eq("status", "published"),
            Eq("category", category)
        );
    }

    var query = new EvQueryParameters()
        .WithFields("*,thumbnail.*")
        .WithFilter(filter)
        .WithSort("-date_created")
        .WithPagination(take, skip);

    return await mDirectusClient
        .GetItemsPagedAsync<JmVideo>(COLLECTION, query, ct);
}
```

### API Endpoint

```csharp
[HttpGet("api/videos")]
public async Task<IActionResult> GetVideos(
    [FromQuery] Int32 skip = 0,
    [FromQuery] Int32 take = 12,
    [FromQuery] String? category = null,
    CancellationToken ct = default)
{
    var result = await _repository.GetVideosWithOffsetAsync(skip, take, category, ct);

    return Ok(new
    {
        data = result.Data,
        total = result.TotalCount,
        hasMore = result.HasMore(skip, take)
    });
}
```

## Cursor-Based Pagination

For large datasets, cursor-based pagination is more efficient:

```csharp
public async Task<(IEnumerable<JmVideo> Videos, Guid? NextCursor)> GetVideosAfterCursorAsync(
    Guid? cursor,
    Int32 limit = 12,
    CancellationToken ct = default)
{
    var filters = new List<DFilter> { Eq("status", "published") };

    if (cursor.HasValue)
    {
        // Get items with ID greater than cursor (assumes UUIDs are sortable)
        filters.Add(Gt("id", cursor.Value.ToString()));
    }

    var filter = filters.Count == 1
        ? filters[0]
        : And(filters.ToArray());

    var query = new EvQueryParameters()
        .WithFields("id,title,thumbnail.*")
        .WithFilter(filter)
        .WithSort("id")
        .WithLimit(limit + 1);  // Fetch one extra to check if more exist

    var items = (await mDirectusClient
        .GetItemsAsync<JmVideo>(COLLECTION, query, ct))
        .ToList();

    var hasMore = items.Count > limit;
    var videos = items.Take(limit);
    var nextCursor = hasMore ? items[limit - 1].ID : (Guid?)null;

    return (videos, nextCursor);
}
```

## Pagination UI Component (Razor)

```cshtml
@if (Model.ViewModel.TotalPages > 1)
{
    <nav aria-label="Page navigation">
        <ul class="pagination justify-content-center">
            @* Previous button *@
            <li class="page-item @(!Model.ViewModel.HasPreviousPage ? "disabled" : "")">
                <a class="page-link"
                   asp-page="./Index"
                   asp-route-page="@(Model.ViewModel.CurrentPage - 1)"
                   asp-route-category="@Model.ViewModel.Category">
                    Previous
                </a>
            </li>

            @* Page numbers *@
            @for (var i = 1; i <= Model.ViewModel.TotalPages; i++)
            {
                <li class="page-item @(i == Model.ViewModel.CurrentPage ? "active" : "")">
                    <a class="page-link"
                       asp-page="./Index"
                       asp-route-page="@i"
                       asp-route-category="@Model.ViewModel.Category">
                        @i
                    </a>
                </li>
            }

            @* Next button *@
            <li class="page-item @(!Model.ViewModel.HasNextPage ? "disabled" : "")">
                <a class="page-link"
                   asp-page="./Index"
                   asp-route-page="@(Model.ViewModel.CurrentPage + 1)"
                   asp-route-category="@Model.ViewModel.Category">
                    Next
                </a>
            </li>
        </ul>
    </nav>

    <p class="text-center text-muted">
        Showing @((Model.ViewModel.CurrentPage - 1) * Model.ViewModel.PageSize + 1)
        to @Math.Min(Model.ViewModel.CurrentPage * Model.ViewModel.PageSize, Model.ViewModel.TotalItems)
        of @Model.ViewModel.TotalItems items
    </p>
}
```

## Performance Tips

1. **Request only needed fields**: Use `WithFields()` to limit data transfer
2. **Reasonable page sizes**: 12-50 items per page is typical
3. **Index filtered fields**: Ensure Directus collection fields used in filters are indexed
4. **Cache first page**: First page is often requested most; consider caching it
5. **Use cursor pagination**: For very large datasets (100k+ items), cursor pagination is more efficient than offset
