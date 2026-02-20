---
name: dotnet-code-standards
description: Enforce comprehensive C# .NET coding rules, formatting conventions, and best practices. Covers naming conventions, formatting patterns, logging with LoggerMessage delegates, async/await patterns, LINQ optimization, xUnit testing, and modern C# 12–14 features.
license: Internal
compatibility: Requires .NET 10.0+ with C# 14
metadata:
  author: evanto media GmbH
  version: "1.1.0"
  framework: .NET
  target: .NET 10.0+
  language: C# 14
---

# C# .NET Code Standards

Enforce comprehensive C# .NET coding rules, formatting conventions, and best practices for the current project.

## Description

This skill enables Claude to write, review, and refactor C# .NET code according to established organizational standards. It enforces consistent naming conventions, formatting patterns, logging practices, async/await usage, LINQ patterns, testing approaches, and modern C# 12–14 features.

## Instructions

You are a C# .NET code standards expert for the current project and Evanto Media shared libraries. Your role is to ensure all C# code adheres to established organizational standards for readability, maintainability, and consistency.

### Core Responsibilities

1. **Standards Enforcement**: Apply all documented coding rules when writing or reviewing C# code
2. **Modern C# Features**: Utilize C# 12–14 features (primary constructors, collection expressions, `field` keyword, extension members, null-conditional assignment, etc.)
3. **Consistent Formatting**: Maintain precise alignment, spacing, and documentation patterns
4. **Performance Patterns**: Apply LoggerMessage delegates, ConfigureAwait, and efficient LINQ
5. **Code Quality**: Enforce guard clauses, proper error handling, and testable code structure

### Naming Conventions

**Member Variables:**
- Prefix with `m` in PascalCase: `mPascalCaseVariable`
- NEVER combine `m` prefix with `var`: ❌ `var mTelemetryEnabled`, ✅ `var telemetryEnabled`
- Use `readonly` fields wherever possible
- Start with private members, then public properties

**Local Variables:**
- Use `var` instead of explicit types wherever possible
- Use camelCase: `telemetryEnabled`, `userCount`
- Two-letter abbreviations stay uppercase at end: `userID`, `installationIP` (NOT `userId`, `installationIp`)

**Type Names:**
- Use .NET types instead of C# aliases: `String` not `string`, `Int32` not `int`, `Boolean` not `bool`
- Always add space after type casting: `(Int32) countAsDouble`

**Class Naming:**
- Shared libraries: prefix `Ev` (Evanto): `EvPayPalClient`, `EvSmtpProvider`
- Project-specific: prefix `Jm`, `Srs`, etc.: `JmRegistrationViewModel`, `SrsTaxCalculationService`
- Services (business logic): suffix `Service`: `SrsTaxCalculationService : ISrsTaxCalculationService`
- Infrastructure/Technical: suffix `Provider`: `EvSmtpProvider : IEvSmtpProvider`
- DTOs/ViewModels: suffix `ViewModel`: `JmRegistrationViewModel`
- Use records for DTOs: `public record EvUserViewModel(Int32 Id, String Name);`

**Namespaces:**
- Use short form: `namespace Evanto.Core.Sample;` (NOT block form with braces)
- NO subdirectories in namespace: `Evanto.Core.Sample.Web` NOT `Evanto.Core.Sample.Web.Controllers`
- Prefer `using` statements over fully qualified names in code
- Each class in its own file

### Member Variables and Constructors

**Primary Constructors:**
Use primary constructors directly in class declaration:

```csharp
public class WebStatisticsProvider(ILogger<WebStatisticsProvider> logger) : IWebStatisticsProvider
{
    private readonly DateTime mStartTime = DateTime.Now;

    // Direct access to injected logger parameter
    public void LogEvent() => logger.LogInformation("Event occurred");
}
```

**Member Formatting:**
Format with XML documentation, vertical alignment, and private-first ordering:

```csharp
public class EvAssetProxyService : IEvAssetProxyService
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   HTTP client factory for making requests. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly IHttpClientFactory             mHttpClientFactory;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Logger instance for diagnostics. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly ILogger<EvAssetProxyService>   mLogger;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Directus configuration settings. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly EvDirectusSettings             mSettings;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Timestamp of last cleanup operation. </summary>
    ///-------------------------------------------------------------------------------------------------
    private DateTime mLastCleanup = DateTime.Now;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets all active users. </summary>
    ///-------------------------------------------------------------------------------------------------
    public IEnumerable<WebUserStatistics> ActiveUsers { get { return mUsers.Values.Where(u => u.IsActive); } }
}
```

**Modern C# Features:**
- Collection expressions (C# 12): `Int32[] numbers = [1, 2, 3];` with spread `[..first, ..second]`
- Using statements for disposables: `using var client = factory.CreateClient();`
- Nullable reference types: `String? userName`, null-coalescing `??`, null-conditional `?.`
- Null-conditional assignment (C# 14): `customer?.Order = GetCurrentOrder();`
- `field` keyword (C# 14) in property accessors to avoid manual backing fields

### Variable Formatting

**Vertical Alignment:**
Align `=` signs when multiple consecutive assignments:

```csharp
[AllowAnonymous]
[HttpPost]
public async Task<ActionResult> Login(LoginViewModel viewModel)
{   // initial assignments
    var watch   = Stopwatch.StartNew();
    var result  = String.Empty;
    var status  = STATUS_ERROR;
    var role    = ProjectConstants.ANONYMOUS_ROLE;
    var param   = $"User: {viewModel.LoginName}";
```

**Object Initialization:**
Vertical alignment for properties:

```csharp
try
{   // create new history
    var history = new WebUserHistory()
    {
        Action          = action.ToLower(),
        InstallationIP  = installationIP,
        CommonName      = commonName,
        Parameter       = parameter,
        ExecutionTime   = executionTime,
        Status          = status,
        ErrorMessage    = errorMessage
    };
```

### Block Statements and Comments

**Block Comments:**
Place comments directly after opening brace `{` with same indentation:

```csharp
public async Task<ActionResult> LogOff()
{   // get user data and sign out
    var userData = mUserDataProvider.GetData();
    name         = userData?.LoginName ?? name;

    if (command.DoExecute() == CommandStatus.Executed)
    {   // success - redirect to home
        status = STATUS_OK;
        return View();
    }

    try
    {   // sign out and clear session
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        mUserDataProvider.Clear();
        status = STATUS_OK;
    }

    catch (Exception ex)
    {   // log error details
        result = $"Interner Fehler: {ex.Message}";
        mLogger.LogError(result);
    }

    return RedirectToAction("Index", "Home");
}
```

**Empty Line Rules:**
- Add empty line after closing block `}`
- NO empty line when multiple closing braces follow each other
- NO empty line between `catch` and previous `}`
- NO empty line between `finally` and previous `}`

### Method Documentation

**XML Documentation Comments:**
Add complete XML documentation above each method:

```csharp
///-------------------------------------------------------------------------------------------------
/// <summary>   Retrieves asset from cache or fetches from Directus if not cached. </summary>
///
/// <param name="assetRequest">       The asset request with transformation parameters. </param>
/// <param name="cancellationToken">  The cancellation token. </param>
///
/// <returns>   Stream containing the asset data. </returns>
///-------------------------------------------------------------------------------------------------
public async Task<Stream> GetAssetAsync(
    EvAssetRequest assetRequest,
    CancellationToken cancellationToken = default)
{
```

**Method Signature Formatting:**
When multiple parameters, align vertically:

```csharp
private IList<McpClientTool> DetermineToolsToTest(
    IList<McpClientTool> availableTools,
    IList<McpToolTestConfiguration> toolTests,
    Boolean quickTest)
{
```

### Guard Clauses

**Always use modern guard clause syntax:**

✅ **Correct:**
```csharp
public ViewResult EmailConfirmation(Guid userGuid, String customerEmail)
{   // check requirements
    ArgumentNullException.ThrowIfNull(userGuid);
    ArgumentException.ThrowIfNullOrEmpty(customerEmail);
    ObjectDisposedException.ThrowIf(mDisposed, nameof(EvAssetProxyService));
```

❌ **Avoid:**
```csharp
public ViewResult EmailConfirmation(Guid userGuid, String customerEmail)
{
    if (String.IsNullOrWhiteSpace(customerEmail))
        throw new ArgumentException("Customer email cannot be null or empty");
```

### Logging Pattern

**LoggerMessage Delegates (CA1848 Performance Rule):**

Always use LoggerMessage delegates in a `#region LoggerMessage` at end of file:

```csharp
#region LoggerMessage

[LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error obtaining Directus service tokens.")]
private partial void LogErrorObtainingDirectusServiceTokens(Exception ex);

[LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Failed to refresh Directus token. Status: {Status}, Error: {Error}")]
private partial void LogFailedToRefreshDirectusToken(System.Net.HttpStatusCode status, String error);

[LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "✅ Cached asset: {AssetId} to {CachePath} ({FileSize})")]
private partial void LogCachedAsset(String assetId, String cachePath, String fileSize);

#endregion
```

**Usage:**
```csharp
catch (Exception ex)
{
    LogErrorObtainingDirectusServiceTokens(ex);
    throw;
}
```

❌ **NEVER use direct ILogger methods:**
```csharp
mLogger.LogInformation("This is an information");
mLogger.LogError(ex, "This is an error");
```

### Async/Await Patterns

**Async Method Requirements:**
- Always use `Async` suffix: `GetUserAsync`, `SaveDataAsync`
- Always await or return the task
- Support `CancellationToken` with default parameter
- Use `ConfigureAwait(false)` in libraries
- NEVER use `async void` (except event handlers)
- NEVER block with `.Result` or `.Wait()`

```csharp
public async Task<EvUser> GetUserAsync(Int32 id, CancellationToken ct = default)
{   // fetch user from database
    return await mContext.Users
        .FirstOrDefaultAsync(u => u.Id == id, ct)
        .ConfigureAwait(false);
}
```

**ConfigureAwait Usage:**
Always use `ConfigureAwait(false)` in library code:

```csharp
var response = await client
    .PostAsJsonAsync("/auth/login", loginRequest, cancellationToken)
    .ConfigureAwait(false);

var json = await response.Content
    .ReadFromJsonAsync<EvEntityResult<EvLoginResponse>>(cancellationToken: cancellationToken)
    .ConfigureAwait(false);
```

**When to use `ValueTask<T>`:**
- Methods that frequently complete synchronously (cache hits, buffered reads)
- High-throughput scenarios where `Task` allocation overhead matters

```csharp
// ✅ Good candidate: cache hit returns synchronously most of the time
public ValueTask<EvAsset?> GetAssetAsync(String assetID, CancellationToken ct = default)
{
    if (mCache.TryGetValue(assetID, out var cached))
        return ValueTask.FromResult<EvAsset?>(cached);

    return LoadAssetFromStoreAsync(assetID, ct);
}
```

**When NOT to use `ValueTask<T>`:**
- When the result will be awaited multiple times
- When using `.Result` or `.GetAwaiter().GetResult()`
- When the complexity isn't justified by profiling

```csharp
// ❌ NEVER await a ValueTask more than once
var valueTask = GetAssetAsync("abc", ct);
var first  = await valueTask;
var second = await valueTask; // undefined behavior!

// ❌ NEVER block on a ValueTask
var result = GetAssetAsync("abc", ct).Result;
```

### LINQ Patterns

**Prefer Method Syntax:**
```csharp
var activeUsers = users
    .Where(u => u.IsActive)
    .OrderBy(u => u.LastName)
    .Select(u => new UserViewModel(u.Id, u.FullName))
    .ToList();
```

**Materialize Once (Avoid Multiple Enumeration):**
```csharp
// ✅ Materialize once
var activeUsers = users.Where(u => u.IsActive).ToList();
var count = activeUsers.Count;
var first = activeUsers.First();

// ❌ Enumerates multiple times
var activeUsers = users.Where(u => u.IsActive);
var count = activeUsers.Count();  // Enumeration 1
var first = activeUsers.First();  // Enumeration 2
```

### Error Handling

**Custom Exception Hierarchy:**
```csharp
public class DomainException : Exception
{
    public DomainException(String message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(String message) : base(message) { }
}

public class ValidationException : DomainException
{
    public IReadOnlyList<String> Errors { get; }

    public ValidationException(IReadOnlyList<String> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}
```

**Result Pattern (Alternative to Exceptions):**
```csharp
public class Result<T>
{
    public Boolean IsSuccess { get; }
    public T? Value { get; }
    public String? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(String error)
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(String error) => new(error);
}
```

### Testing with xUnit

**Test Class Structure:**
```csharp
public class EvUserServiceTests
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Mock repository for user data. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly Mock<IEvUserRepository> mMockRepo;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   System under test. </summary>
    ///-------------------------------------------------------------------------------------------------
    private readonly EvUserService mSut;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Initializes test dependencies. </summary>
    ///-------------------------------------------------------------------------------------------------
    public EvUserServiceTests()
    {
        mMockRepo = new Mock<IEvUserRepository>();
        mSut = new EvUserService(mMockRepo.Object);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests GetByIdAsync returns user with valid ID. </summary>
    ///-------------------------------------------------------------------------------------------------
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {   // arrange
        var expectedUser = new EvUser { Id = 1, Name = "John" };
        mMockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedUser);

        // act
        var result = await mSut.GetByIdAsync(1);

        // assert
        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
    }
}
```

**Test Naming Convention:**
`MethodName_Scenario_ExpectedBehavior`

Examples:
- `GetByIdAsync_WithValidId_ReturnsUser`
- `CreateAsync_WithInvalidData_ThrowsValidationException`
- `DeleteAsync_WhenNotFound_ReturnsNotFoundResult`

### Workflow

When writing or reviewing C# code:

1. **Check Naming**: Verify all names follow conventions (m prefix, correct suffixes, .NET types)
2. **Format Members**: Ensure proper alignment, documentation, and ordering
3. **Apply Guard Clauses**: Add modern guard clause syntax at method start
4. **Use LoggerMessage**: Replace direct ILogger calls with delegate pattern
5. **Async Patterns**: Verify Async suffix, CancellationToken, ConfigureAwait usage
6. **LINQ Optimization**: Check for multiple enumeration, prefer method syntax
7. **Documentation**: Add XML comments to all public/internal methods
8. **Test Coverage**: Ensure xUnit tests follow naming and structure patterns

### Common Mistakes to Avoid

❌ **Wrong:**
```csharp
// Mixing m prefix with var
var mTelemetryEnabled = GetTelemetry();

// C# types instead of .NET types
private string userName;
private int userCount;

// No space after type cast
var count = (Int32)countAsDouble;

// Direct logger usage
mLogger.LogInformation("User logged in");

// Missing Async suffix
public Task<User> GetUser(int id) { }

// Wrong abbreviation casing
var userId = 123;  // Should be userID

// No guard clauses
public void Process(String data)
{
    if (String.IsNullOrEmpty(data))
        throw new ArgumentException("data cannot be empty");
```

✅ **Correct:**
```csharp
// Separate declarations
var telemetryEnabled = GetTelemetry();
private Boolean mTelemetryEnabled;

// .NET types
private String mUserName;
private Int32 mUserCount;

// Space after cast
var count = (Int32) countAsDouble;

// LoggerMessage delegate
LogUserLoggedIn(userName);

// Async suffix
public async Task<User> GetUserAsync(Int32 id, CancellationToken ct = default) { }

// Correct abbreviation
var userID = 123;

// Modern guard clauses
public void Process(String data)
{   // validate input
    ArgumentException.ThrowIfNullOrEmpty(data);
```

### Modern C# Language Features (C# 12–14)

Use these features when the project targets the corresponding language version. Prefer the modern syntax over older patterns.

**Collection Expressions and Spread (C# 12):**
Use `[...]` syntax for creating arrays, lists, and spans. Use the spread operator `..` to flatten collections:

```csharp
// ✅ Collection expression syntax
Int32[] numbers      = [1, 2, 3, 4];
List<String> words   = ["one", "two", "three"];
Span<Char> chars     = ['a', 'b', 'c'];

// ✅ Spread operator to combine collections
Int32[] first  = [1, 2, 3];
Int32[] second = [4, 5, 6];
Int32[] all    = [..first, ..second]; // [1, 2, 3, 4, 5, 6]
```

**Alias Any Type (C# 12):**
Use `using` to alias tuples, arrays, generics, and other complex types:

```csharp
using Pair    = (Int32 X, Int32 Y);
using IntList = System.Collections.Generic.List<Int32>;

Pair point    = (3, 4);
IntList items = [1, 2, 3];
```

**Default Lambda Parameters (C# 12):**
Lambdas can declare default parameter values and `params`:

```csharp
var incrementBy = (Int32 source, Int32 increment = 1) => source + increment;
Console.WriteLine(incrementBy(5));    // 6 – uses default
Console.WriteLine(incrementBy(5, 2)); // 7 – uses supplied
```

**`ref readonly` Parameters (C# 12):**
Express that a reference parameter will not be mutated:

```csharp
public static void PrintLength(ref readonly String text)
{   // text is passed by reference but cannot be modified
    Console.WriteLine(text.Length);
}
```

**Inline Arrays (C# 12):**
Embed fixed-size buffers in structs safely without unsafe code:

```csharp
[System.Runtime.CompilerServices.InlineArray(10)]
public struct Buffer
{
    private Int32 _element0;
}

var buffer = new Buffer();
for (var i = 0; i < 10; i++) buffer[i] = i;
```

**`params` Collections (C# 13):**
`params` works with `Span<T>`, `ReadOnlySpan<T>`, and `IEnumerable<T>` – not just arrays:

```csharp
public void Concat<T>(params ReadOnlySpan<T> items)
{
    for (var i = 0; i < items.Length; i++)
        Console.Write(items[i]);
}
```

**New Lock Type (C# 13):**
Use `System.Threading.Lock` instead of `object` for thread synchronization:

```csharp
// ✅ Modern lock (C# 13 / .NET 9+)
private readonly Lock mLock = new();

public void CriticalSection()
{
    lock (mLock)
    {   // improved synchronization via Lock API
        DoWork();
    }
}

// ❌ Avoid older pattern
private readonly Object mLockObj = new();
```

**Implicit Index in Object Initializers (C# 13):**
Use `^` (from-the-end index) inside object/collection initializers:

```csharp
var countdown = new TimerRemaining
{
    Buffer =
    {
        [^1] = 0,
        [^2] = 1,
        [^3] = 2
    }
};
```

**Partial Properties and Indexers (C# 13):**
Declare partial properties with a defining and implementing declaration:

```csharp
public partial class EvConfigService
{   // declaration
    public partial String Name { get; set; }
}

public partial class EvConfigService
{   // implementation
    private String mName;
    public partial String Name
    {
        get => mName;
        set => mName = value;
    }
}
```

**`allows ref struct` Anti-Constraint (C# 13):**
Permit `Span<T>` and other `ref struct` types as generic type arguments:

```csharp
public class SpanConsumer<T> where T : allows ref struct
{
    public void Process(scoped T value) { /* ... */ }
}
```

**Overload Resolution Priority (C# 13):**
Guide the compiler to prefer a more efficient overload:

```csharp
[System.Reflection.Metadata.OverloadResolutionPriority(1)]
public void Process(ReadOnlySpan<Byte> data) { /* fast path */ }

public void Process(Byte[] data) { /* fallback */ }
```

**`field` Keyword in Property Accessors (C# 14):**
Access the compiler-generated backing field directly – avoids manual field declarations:

```csharp
// ✅ Use field keyword for validation in semi-auto properties
public String Message
{
    get;
    set => field = value ?? throw new ArgumentNullException(nameof(value));
}

public Int32 Count
{
    get;
    set => field = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
}
```

If the type has a member named `field`, use `@field` or `this.field` to disambiguate.

**Extension Members (C# 14):**
Define extension properties, static members, and operators using the `extension` block syntax:

```csharp
public static class EvStringExtensions
{
    extension(String source)
    {
        public Boolean IsNullOrEmpty => String.IsNullOrEmpty(source);
        public String TrimAndLower() => source.Trim().ToLowerInvariant();
    }
}

// Usage – called like instance members
var name = "  Sven  ";
var clean = name.TrimAndLower();       // "sven"
var empty = name.IsNullOrEmpty;        // false
```

**Null-Conditional Assignment (C# 14):**
Use `?.` and `?[]` on the left-hand side of assignments:

```csharp
// ✅ Assignment only executes if left side is not null
customer?.Order  = GetCurrentOrder();
customer?.Points += 10;

// Works with indexers too
customers?[0].Name = "Updated";
```

Note: `++` and `--` are not allowed with null-conditional assignment.

**Simple Lambda Parameters with Modifiers (C# 14):**
Add `ref`, `in`, `out`, `scoped` to lambda parameters without explicit types:

```csharp
delegate Boolean TryParse<T>(String text, out T result);

// ✅ Modifiers without types (C# 14)
TryParse<Int32> parse = (text, out result) => Int32.TryParse(text, out result);

// Equivalent older syntax
TryParse<Int32> parse2 = (String text, out Int32 result) => Int32.TryParse(text, out result);
```

**Partial Constructors and Events (C# 14):**
Extend the partial member pattern to constructors and events:

```csharp
public partial class EvService
{   // declarations
    public partial EvService();
    public partial event EventHandler Started;
}

public partial class EvService
{   // implementations
    private readonly ILogger mLogger;
    public partial EvService()
    {
        mLogger = new ConsoleLogger();
    }

    private event EventHandler mStarted;
    public partial event EventHandler Started
    {
        add    => mStarted += value;
        remove => mStarted -= value;
    }
}
```

**Implicit Span Conversions (C# 14):**
Arrays and strings convert implicitly to spans without `.AsSpan()`:

```csharp
Int32[] numbers          = [1, 2, 3];
Span<Int32> span         = numbers;           // array → Span
ReadOnlySpan<Int32> ro   = numbers;           // array → ReadOnlySpan
ReadOnlySpan<Int32> also = span;              // Span → ReadOnlySpan
ReadOnlySpan<Char> chars = "hello";           // string → ReadOnlySpan<char>
```

**Unbound Generic Types in `nameof` (C# 14):**
```csharp
// ✅ C# 14 – no need for a closed generic type
var name = nameof(List<>); // "List"

// Previously required:
var name = nameof(List<Int32>); // "List"
```

**User-Defined Compound Assignment (C# 14):**
Override compound assignment operators like `+=` separately from the binary operator:

```csharp
public static MyCollection operator +(MyCollection left, MyCollection right) { /* ... */ }
// The compiler calls op_AdditionAssignment if defined
```

**`\e` Escape Sequence (C# 13):**
Use `\e` for the ESCAPE character (U+001B) instead of ambiguous `\x1b`:

```csharp
// ✅ Modern escape sequence
var ansiRed = "\e[31m";

// ❌ Older, potentially ambiguous
var ansiRed = "\x1b[31m";
```

**Pattern Matching (Property, Relational, List):**
C# provides powerful pattern matching capabilities. Use switch expressions with patterns for clean, exhaustive branching:

```csharp
// Property patterns – match on object properties
public record EvPerson(String Name, Int32 Age, EvAddress? Address);
public record EvAddress(String City, String Country);

String GetDescription(EvPerson person) => person switch
{
    { Age: < 18 }                                 => "Minor",
    { Age: >= 65, Address.Country: "DE" }         => "German Senior",
    { Name: "Admin", Age: >= 18 }                 => "Adult Administrator",
    { Address: null }                              => "Person without address",
    { Address.City: var city }                     => $"Lives in {city}",
    _                                              => "Unknown"
};

// Relational patterns – compare values directly in patterns
String GetTemperatureDescription(Double celsius) => celsius switch
{
    < -20            => "Extreme cold",
    >= -20 and < 0   => "Freezing",
    >= 0 and < 15    => "Cold",
    >= 15 and < 25   => "Comfortable",
    >= 25 and < 35   => "Warm",
    >= 35            => "Hot"
};

// List patterns (C# 11+) – match on collection structure
String AnalyzeList(Int32[] numbers) => numbers switch
{
    []                          => "Empty",
    [var single]                => $"Single element: {single}",
    [var first, var second]     => $"Pair: {first}, {second}",
    [var first, .., var last]   => $"First: {first}, Last: {last}",
    [_, _, _, ..]               => "Three or more elements"
};

// Combining patterns with logical operators
Boolean IsValidRequest(Object request) => request is
    { } and not null and
    (HttpRequest { Method: "GET" or "POST" } or GrpcRequest { IsAuthenticated: true });
```

### Performance Considerations

**Use readonly for immutable fields:**
```csharp
private readonly IHttpClientFactory mHttpClientFactory;
private readonly ILogger<EvAssetProxyService> mLogger;
```

**Avoid allocations in hot paths:**
```csharp
// ✅ Reuse StringBuilder
private readonly StringBuilder mBuilder = new();

// ✅ Use ArrayPool for temporary arrays
var buffer = ArrayPool<Byte>.Shared.Rent(size);
try { /* use buffer */ }
finally { ArrayPool<Byte>.Shared.Return(buffer); }
```

**Use Span<T> for slicing:**
```csharp
// C# 14: implicit span conversion – no .AsSpan() needed
ReadOnlySpan<Char> span = text;
var firstPart = span.Slice(0, 10);

// Use params ReadOnlySpan<T> (C# 13) for allocation-free variadic methods
public void Log(params ReadOnlySpan<String> messages) { /* ... */ }
```

## Examples

See the `examples/` directory for complete reference implementations:
- `class-structure.cs` - Complete class with all patterns applied
- `async-patterns.cs` - Async/await best practices
- `logging-patterns.cs` - LoggerMessage delegate examples
- `test-patterns.cs` - xUnit test structure
- `guard-clauses.cs` - Modern guard clause patterns
- `linq-patterns.cs` - Efficient LINQ usage

## Quick Reference

See `QUICKREF.md` for a condensed reference card with the most common patterns and rules.

## Notes

- Target Framework: .NET 10.0
- C# Language Version: 14 (covers features from C# 12, 13, and 14)
- Project prefixes: `Ev` (Evanto shared), `Jm` (JM Budoacademy), `Srs` (Shorin Ryu Seibukan), ..
- Always check existing codebase for established patterns before creating new code
- When in doubt, prioritize readability and consistency over cleverness
- Performance matters, but correctness and maintainability come first

## Version

1.1.0 - Added comprehensive C# 12–14 language features (February 2025)
1.0.0 - Initial release based on evanto coding standards (January 2025)
