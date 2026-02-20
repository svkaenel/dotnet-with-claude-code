# Directus Filter Examples

This reference shows advanced filtering patterns using `EvDirectusFilter`.

## Import Statement

Always import the static helpers:

```csharp
using static Evanto.Directus.Client.Filters.EvDirectusFilter;
```

## Basic Comparisons

### Equality

```csharp
// Simple equality
var filter = Eq("status", "published");

// Not equals
var filter = Neq("status", "archived");
```

### Numeric Comparisons

```csharp
// Greater than
var filter = Gt("price", 100);

// Greater than or equal
var filter = Gte("quantity", 0);

// Less than
var filter = Lt("age", 18);

// Less than or equal
var filter = Lte("score", 100);

// Between (inclusive)
var filter = Between("price", 10, 50);
```

### Date Comparisons

```csharp
// Format dates as ISO strings
var filter = Gte("date_created", "2025-01-01");
var filter = Lte("valid_until", DateTime.UtcNow.ToString("yyyy-MM-dd"));

// Date range
var filter = And(
    Gte("event_date", "2025-01-01"),
    Lte("event_date", "2025-12-31")
);
```

## String Operations

### Contains

```csharp
// Case-sensitive contains
var filter = Contains("description", "keyword");

// Case-insensitive contains (recommended for search)
var filter = IContains("title", "search term");
```

### Starts/Ends With

```csharp
var filter = StartsWith("email", "admin@");
var filter = EndsWith("filename", ".pdf");
```

## Array Operations

### IN Operator

```csharp
// Multiple possible values
var filter = In("status", "published", "draft");
var filter = In("category", "karate", "judo", "aikido");

// NOT IN
var filter = Nin("status", "archived", "deleted");
```

## Null Checks

```csharp
// Is null
var filter = Null("deleted_at");

// Is not null
var filter = NotNull("verified_at");

// Is empty (empty string or null)
var filter = Empty("description");

// Is not empty
var filter = NotEmpty("title");
```

## Logical Operators

### AND

```csharp
// All conditions must match
var filter = And(
    Eq("status", "published"),
    Eq("language", "de"),
    Gte("date_published", "2025-01-01")
);
```

### OR

```csharp
// Any condition can match
var filter = Or(
    Eq("category", "beginner"),
    Eq("category", "intermediate")
);
```

### Nested Logic

```csharp
// Complex: (status = published) AND (language = de OR language = en)
var filter = And(
    Eq("status", "published"),
    Or(
        Eq("language", "de"),
        Eq("language", "en")
    )
);

// Complex: (active AND verified) OR admin
var filter = Or(
    And(
        Eq("is_active", true),
        NotNull("verified_at")
    ),
    Eq("role", "admin")
);
```

## Relational Field Filtering

Filter on related collection fields using dot notation:

```csharp
// Filter by related field value
var filter = Eq("author.name", "John Doe");

// Multiple levels deep
var filter = Eq("category.parent.slug", "sports");

// Combined with other filters
var filter = And(
    Eq("status", "published"),
    Eq("author.role", "editor"),
    In("category.slug", "news", "blog")
);
```

## Complete Examples

### Search with Multiple Criteria

```csharp
public async Task<IEnumerable<JmVideo>> SearchVideosAsync(
    String? searchTerm,
    String? category,
    String? level,
    String language,
    CancellationToken ct)
{
    var filters = new List<DFilter>
    {
        Eq("status", "published")
    };

    if (!String.IsNullOrEmpty(language) && language != "all")
        filters.Add(Eq("language", language));

    if (!String.IsNullOrWhiteSpace(category) && category != "all")
        filters.Add(Eq("category", category));

    if (!String.IsNullOrWhiteSpace(level) && level != "all")
        filters.Add(Eq("level", level));

    if (!String.IsNullOrWhiteSpace(searchTerm))
    {
        filters.Add(Or(
            IContains("title", searchTerm),
            IContains("description", searchTerm)
        ));
    }

    var filter = filters.Count == 1
        ? filters[0]
        : And(filters.ToArray());

    var query = new EvQueryParameters()
        .WithFilter(filter)
        .WithSort("-date_created")
        .WithLimit(100);

    return await mDirectusClient
        .GetItemsAsync<JmVideo>(COLLECTION, query, ct);
}
```

### Date Range with Status Filter

```csharp
public async Task<IEnumerable<JmEvent>> GetUpcomingEventsAsync(
    DateTime fromDate,
    DateTime toDate,
    CancellationToken ct)
{
    var filter = And(
        Eq("status", "published"),
        Gte("event_date", fromDate.ToString("yyyy-MM-dd")),
        Lte("event_date", toDate.ToString("yyyy-MM-dd")),
        NotNull("location")  // Must have a location set
    );

    var query = new EvQueryParameters()
        .WithFilter(filter)
        .WithFields("*,location.*,organizer.*")
        .WithSort("event_date")
        .WithLimit(50);

    return await mDirectusClient
        .GetItemsAsync<JmEvent>("events", query, ct);
}
```

### Soft Delete Pattern

```csharp
// Get only non-deleted items
var filter = And(
    Null("deleted_at"),
    Eq("status", "active")
);

// Get deleted items for admin view
var filter = NotNull("deleted_at");
```

### Multi-Tenant Filter

```csharp
public async Task<IEnumerable<T>> GetForTenantAsync<T>(
    String collection,
    Guid tenantId,
    CancellationToken ct)
{
    var filter = And(
        Eq("tenant_id", tenantId.ToString()),
        Eq("status", "active")
    );

    var query = new EvQueryParameters()
        .WithFilter(filter);

    return await mDirectusClient
        .GetItemsAsync<T>(collection, query, ct);
}
```

## Filter Serialization

The filter tree can be serialized in two ways:

### JSON Format (Default)

```csharp
var query = new EvQueryParameters()
    .WithFilter(filter);
// Produces: ?filter={"_and":[{"status":{"_eq":"published"}},...]}
```

### Bracketed Format

```csharp
var query = new EvQueryParameters()
    .WithFilter(filter, asBracketed: true);
// Produces: ?filter[status][_eq]=published&filter[language][_eq]=de
```

Use bracketed format for simpler queries or when debugging URL construction.
