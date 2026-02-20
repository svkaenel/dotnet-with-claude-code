# C# .NET Code Standards - Quick Reference

## Naming Conventions

```csharp
// ✅ Member variables: m prefix + PascalCase
private readonly String mUserName;
private readonly Int32 mUserCount;

// ✅ Local variables: camelCase + var
var userName = "John";
var userID = 123;  // Two-letter abbreviations stay uppercase

// ✅ Types: Use .NET types
String not string, Int32 not int, Boolean not bool

// ✅ Type casting: Space after type
var count = (Int32) countAsDouble;

// ✅ Class prefixes
EvPayPalClient      // Ev = Evanto shared
JmUserViewModel     // Jm = JM project
SrsTaxService       // Srs = Other projects

// ✅ Class suffixes
EvSmtpProvider      // Infrastructure/technical
SrsTaxService       // Business logic
JmUserViewModel     // DTOs (or use record)
```

## Member Declaration Pattern

```csharp
public class EvSampleService(ILogger<EvSampleService> logger) : IEvSampleService
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   HTTP client factory. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly IHttpClientFactory mHttpFactory;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Timestamp of last cleanup. </summary>
    ///-------------------------------------------------------------------------------------------------
    private DateTime mLastCleanup = DateTime.Now;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets the user count. </summary>
    ///-------------------------------------------------------------------------------------------------
    public Int32 UserCount => mUsers.Count;
}
```

## Guard Clauses

```csharp
public void ProcessData(String data, Int32 count)
{   // check requirements
    ArgumentException.ThrowIfNullOrEmpty(data);
    ArgumentOutOfRangeException.ThrowIfNegative(count);
}

// Disposable check
ObjectDisposedException.ThrowIf(mDisposed, nameof(Service));

// Range checks
ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
ArgumentOutOfRangeException.ThrowIfGreaterThan(percentage, 100m);
```

## Logging Pattern

```csharp
// ✅ Use LoggerMessage delegates
#region LoggerMessage

[LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error processing: {UserID}")]
private partial void LogProcessingError(Exception ex, String userID);

[LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "✅ Cached: {Key}")]
private partial void LogCached(String key);

#endregion

// Usage
try { }
catch (Exception ex)
{
    LogProcessingError(ex, userID);
}

// ❌ NEVER do this
mLogger.LogError(ex, "Error processing");
```

## Async/Await Pattern

```csharp
// ✅ Always use Async suffix, CancellationToken, ConfigureAwait(false)
public async Task<User> GetUserAsync(
    Int32 id,
    CancellationToken ct = default)
{
    return await mRepo
        .FindAsync(id, ct)
        .ConfigureAwait(false);
}

// ❌ NEVER block
// .Result or .Wait()

// ❌ NEVER async void (except event handlers)

// ✅ ValueTask for frequently synchronous paths (cache hits, buffered reads)
public ValueTask<EvAsset?> GetAssetAsync(String id, CancellationToken ct = default)
{
    if (mCache.TryGetValue(id, out var cached))
        return ValueTask.FromResult<EvAsset?>(cached);
    return LoadFromStoreAsync(id, ct);
}

// ❌ NEVER await a ValueTask more than once
// ❌ NEVER use .Result on a ValueTask
// Only use ValueTask when justified by profiling
```

## Block Formatting

```csharp
public void Method()
{   // comment after opening brace
    var watch  = Stopwatch.StartNew();
    var result = String.Empty;

    if (condition)
    {   // nested comment
        DoSomething();
    }

    try
    {   // process data
        await ProcessAsync();
    }

    catch (Exception ex)
    {   // log error
        LogError(ex);
    }

    // Empty line after }, except when multiple } follow each other
}
```

## LINQ Patterns

```csharp
// ✅ Method syntax + Materialize once
var activeUsers = users
    .Where(u => u.IsActive)
    .OrderBy(u => u.LastName)
    .Select(u => new UserViewModel(u.Id, u.Name))
    .ToList();  // Materialize here

// ✅ Use Any() not Count() > 0
if (users.Any(u => u.IsActive)) { }

// ✅ Use FirstOrDefault(predicate) not Where().First()
var user = users.FirstOrDefault(u => u.Id == 123);
```

## Test Structure

```csharp
public class EvUserServiceTests
{
    private readonly Mock<IEvUserRepository> mMockRepo;
    private readonly EvUserService mSut;  // System Under Test

    public EvUserServiceTests()
    {
        mMockRepo = new Mock<IEvUserRepository>();
        mSut = new EvUserService(mMockRepo.Object);
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {   // arrange
        var expected = new EvUser { Id = 1, Name = "John" };
        mMockRepo.Setup(r => r.GetAsync(1)).ReturnsAsync(expected);

        // act
        var result = await mSut.GetAsync(1);

        // assert
        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
    }
}
```

## Modern C# Features (C# 12–14)

```csharp
// ✅ Collection expressions + spread (C# 12)
Int32[] all = [..first, ..second, 99];

// ✅ Alias any type (C# 12)
using Pair = (Int32 X, Int32 Y);
Pair point = (3, 4);

// ✅ Default lambda parameters (C# 12)
var inc = (Int32 x, Int32 step = 1) => x + step;

// ✅ params collections – not just arrays (C# 13)
public void Log(params ReadOnlySpan<String> messages) { }

// ✅ New Lock type (C# 13)
private readonly Lock mLock = new();
lock (mLock) { /* ... */ }

// ✅ Partial properties (C# 13)
public partial String Name { get; set; }          // declaration
public partial String Name { get => m; set => m = value; } // implementation

// ✅ allows ref struct (C# 13)
where T : allows ref struct

// ✅ field keyword in accessors (C# 14)
public String Name
{
    get;
    set => field = value ?? throw new ArgumentNullException(nameof(value));
}

// ✅ Extension members (C# 14) – properties, operators, static
extension(String source)
{
    public Boolean IsNullOrEmpty => String.IsNullOrEmpty(source);
}

// ✅ Null-conditional assignment (C# 14)
customer?.Order  = GetCurrentOrder();
customer?.Points += 10;

// ✅ Lambda modifiers without types (C# 14)
TryParse<Int32> p = (text, out result) => Int32.TryParse(text, out result);

// ✅ Implicit span conversions (C# 14)
Span<Int32> span         = numbers;    // no .AsSpan()
ReadOnlySpan<Char> chars = "hello";

// ✅ Unbound generics in nameof (C# 14)
var name = nameof(List<>); // "List"

// ✅ \e escape sequence (C# 13)
var ansiRed = "\e[31m";   // instead of \x1b[31m
```

## Pattern Matching

```csharp
// ✅ Property patterns – match on object properties
String GetDescription(EvPerson p) => p switch
{
    { Age: < 18 }                          => "Minor",
    { Age: >= 65, Address.Country: "DE" }  => "German Senior",
    { Address: null }                      => "No address",
    { Address.City: var city }             => $"Lives in {city}",
    _                                      => "Unknown"
};

// ✅ Relational patterns – compare values directly
String Classify(Double c) => c switch
{
    < 0              => "Freezing",
    >= 0 and < 15    => "Cold",
    >= 15 and < 25   => "Comfortable",
    >= 25            => "Warm"
};

// ✅ List patterns (C# 11+) – match on structure
String Analyze(Int32[] n) => n switch
{
    []                         => "Empty",
    [var single]               => $"One: {single}",
    [var first, .., var last]  => $"First: {first}, Last: {last}",
    _                          => "Other"
};

// ✅ Combining patterns with logical operators
Boolean IsValid(Object req) => req is
    { } and not null and
    (HttpRequest { Method: "GET" or "POST" } or GrpcRequest { IsAuthenticated: true });
```

## Common Mistakes

```csharp
// ❌ WRONG                          // ✅ CORRECT
var mTelemetryEnabled;               var telemetryEnabled;
private string mUserName;            private String mUserName;
var userId = 123;                    var userID = 123;
var count = (Int32)value;            var count = (Int32) value;
mLogger.LogInfo("text");             LogInfoMessage();
public async void Process() {}       public async Task ProcessAsync() {}
var count = items.Count() > 0;       var hasItems = items.Any();
```

## Key Reminders

- **m prefix**: Only for member variables, NOT local variables
- **.NET types**: String, Int32, Boolean (not string, int, bool)
- **Space after cast**: `(Int32) value`
- **Abbreviations**: userID, installationIP (uppercase at end)
- **LoggerMessage**: Always use delegates, never direct ILogger
- **Async suffix**: Always on async methods
- **CancellationToken**: Support in all async methods
- **ConfigureAwait**: Use `.ConfigureAwait(false)` in libraries
- **Guard clauses**: Modern throw helpers at method start
- **LINQ**: Materialize once with .ToList() to avoid multiple enumeration
- **Primary constructors**: Use wherever possible
- **Records**: Use for DTOs/ViewModels
- **Collection expressions**: `Int32[] arr = [1, 2, 3];` with spread `[..a, ..b]`
- **`field` keyword**: Use in property accessors to avoid manual backing fields (C# 14)
- **Extension members**: Use `extension` blocks for extension properties/operators (C# 14)
- **Null-conditional assignment**: `obj?.Prop = value;` (C# 14)
- **Lock type**: Prefer `System.Threading.Lock` over `object` for locking (C# 13)
- **`params` collections**: Use `params ReadOnlySpan<T>` for zero-alloc variadic methods (C# 13)
