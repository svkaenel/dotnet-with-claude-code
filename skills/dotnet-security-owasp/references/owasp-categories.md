# OWASP Top 10 -- .NET Mitigation Details

## Contents

- [A01: Broken Access Control](#a01-broken-access-control)
- [A02: Cryptographic Failures](#a02-cryptographic-failures)
- [A03: Injection](#a03-injection)
- [A04: Insecure Design](#a04-insecure-design)
- [A05: Security Misconfiguration](#a05-security-misconfiguration)
- [A06: Vulnerable and Outdated Components](#a06-vulnerable-and-outdated-components)
- [A07: Identification and Authentication Failures](#a07-identification-and-authentication-failures)
- [A08: Software and Data Integrity Failures](#a08-software-and-data-integrity-failures)
- [A09: Security Logging and Monitoring Failures](#a09-security-logging-and-monitoring-failures)
- [A10: Server-Side Request Forgery](#a10-server-side-request-forgery)

---

## A01: Broken Access Control

**Vulnerability:** Users act outside their intended permissions -- accessing other users' data,
elevating privileges, or bypassing access checks.

**Risk in .NET:** Missing `[Authorize]` attributes on controllers/endpoints, insecure direct object
references (IDOR) where user IDs are taken from route parameters without ownership validation, and
CORS misconfiguration allowing unintended origins.

### Mitigation

```csharp
// 1. Apply authorization globally, then opt out explicitly
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();
app.MapControllers(); // All endpoints require auth by default

// 2. Resource-based authorization to prevent IDOR
public sealed class DocumentAuthorizationHandler
    : AuthorizationHandler<EditRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EditRequirement requirement,
        Document resource)
    {
        if (resource.OwnerId == context.User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

// In the endpoint:
app.MapPut("/documents/{id}", async (
    int id,
    DocumentDto dto,
    IAuthorizationService authService,
    ClaimsPrincipal user,
    AppDbContext db) =>
{
    var document = await db.Documents.FindAsync(id);
    if (document is null) return Results.NotFound();

    var authResult = await authService.AuthorizeAsync(user, document, "Edit");
    if (!authResult.Succeeded) return Results.Forbid();

    document.Title = dto.Title;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
```

```csharp
// 3. Restrict CORS to known origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("Strict", policy =>
    {
        policy.WithOrigins("https://app.example.com")
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "Authorization");
    });
});
```

**Gotcha:** `AllowAnyOrigin()` combined with `AllowCredentials()` is rejected at runtime by
ASP.NET Core, but `SetIsOriginAllowed(_ => true)` with `AllowCredentials()` silently allows all
origins -- never use this pattern.

---

## A02: Cryptographic Failures

**Vulnerability:** Sensitive data exposed due to weak or missing encryption -- plaintext storage,
deprecated algorithms, or improper key management.

**Risk in .NET:** Using MD5/SHA1 for hashing passwords, storing connection strings with plaintext
passwords in `appsettings.json`, transmitting sensitive data over HTTP, or using `DES`/`RC2`.

### Mitigation

```csharp
// Enforce HTTPS and HSTS
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});

var app = builder.Build();
app.UseHsts(); // Strict-Transport-Security header
app.UseHttpsRedirection();

// Never store secrets in appsettings.json -- use user secrets or env vars
// See [skill:dotnet-secrets-management] for proper secrets handling
```

```csharp
// Use Data Protection API for symmetric encryption of application data
public sealed class TokenProtector(IDataProtectionProvider provider)
{
    private readonly IDataProtector _protector =
        provider.CreateProtector("Tokens.V1");

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}
```

See [skill:dotnet-cryptography] for algorithm selection (AES-GCM, RSA, ECDSA) and key derivation.

---

## A03: Injection

**Vulnerability:** Untrusted data sent to an interpreter as part of a command or query -- SQL
injection, command injection, LDAP injection, and cross-site scripting (XSS).

**Risk in .NET:** String concatenation in SQL queries, `Process.Start` with unsanitized input,
rendering user input as raw HTML in Razor pages.

### Mitigation

```csharp
// SQL injection prevention: always use parameterized queries
// EF Core is parameterized by default via LINQ
var orders = await db.Orders
    .Where(o => o.CustomerId == customerId)
    .ToListAsync();

// When raw SQL is needed, use parameterized interpolation
var results = await db.Orders
    .FromSqlInterpolated($"SELECT * FROM Orders WHERE Status = {status}")
    .ToListAsync();

// NEVER concatenate user input into SQL:
// var bad = db.Orders.FromSqlRaw("SELECT * FROM Orders WHERE Status = '" + status + "'");
```

```csharp
// XSS prevention: Razor encodes output by default.
// Use @Html.Raw() ONLY for trusted, pre-sanitized HTML.
// In Minimal APIs, return typed results -- not raw strings:
app.MapGet("/greeting", (string name) =>
    Results.Content($"<p>Hello, {HtmlEncoder.Default.Encode(name)}</p>",
        "text/html"));

// Command injection prevention: avoid Process.Start with user input.
// If unavoidable, validate against an allowlist:
public static bool IsAllowedTool(string toolName) =>
    toolName is "dotnet" or "git" or "nuget";
```

**Gotcha:** `FromSqlRaw` with string concatenation bypasses parameterization. Always use
`FromSqlInterpolated` or pass `SqlParameter` objects to `FromSqlRaw`.

---

## A04: Insecure Design

**Vulnerability:** Flaws in design patterns that cannot be fixed by implementation alone -- missing
rate limiting, lack of defense in depth, unrestricted resource consumption.

**Risk in .NET:** APIs without rate limiting, unbounded file uploads, missing anti-forgery tokens
on state-changing operations.

### Mitigation

```csharp
// Rate limiting with built-in middleware (.NET 7+)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();
app.UseRateLimiter();

app.MapGet("/api/data", () => Results.Ok("data"))
    .RequireRateLimiting("api");
```

```csharp
// Anti-forgery for Minimal APIs (.NET 8+)
builder.Services.AddAntiforgery();

var app = builder.Build();
app.UseAntiforgery();

// Form-bound endpoint: antiforgery validated automatically
app.MapPost("/orders", async ([FromForm] string productId, AppDbContext db) =>
{
    var order = new Order { ProductId = productId };
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/orders/{order.Id}", order);
});

// JSON endpoint: opt in explicitly with RequireAntiforgery()
app.MapPost("/api/orders", async (CreateOrderDto dto, AppDbContext db) =>
{
    var order = new Order { ProductId = dto.ProductId };
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/api/orders/{order.Id}", order);
}).RequireAntiforgery();
```

**Gotcha:** `UseRateLimiter()` must be called after `UseRouting()` and before
`MapControllers()`/`MapGet()` to apply correctly.

---

## A05: Security Misconfiguration

**Vulnerability:** Insecure default configurations, incomplete configurations, open cloud storage,
unnecessary features enabled, verbose error messages.

**Risk in .NET:** Detailed exception pages in production (`UseDeveloperExceptionPage`), default
Kestrel settings exposing server headers, debug endpoints left enabled, or missing security headers.

### Mitigation

```csharp
// Remove server identity headers (configure BEFORE Build)
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Generic error handler in production -- no stack traces
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Add security headers via middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self'");
    await next();
});
```

```csharp
// Constrain request body size to prevent resource exhaustion (configure BEFORE Build)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    options.Limits.MaxRequestHeadersTotalSize = 32 * 1024; // 32 KB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});
```

**Gotcha:** `UseDeveloperExceptionPage()` leaks source code paths and stack traces. Ensure it is
gated behind `IsDevelopment()` and never enabled in production or staging.

---

## A06: Vulnerable and Outdated Components

**Vulnerability:** Using components with known vulnerabilities, unsupported frameworks, or
unpatched dependencies.

**Risk in .NET:** Running on out-of-support .NET versions, NuGet packages with known CVEs,
transitive dependency vulnerabilities not audited.

### Mitigation

```xml
<!-- Enable NuGet audit in Directory.Build.props or csproj -->
<PropertyGroup>
  <NuGetAudit>true</NuGetAudit>
  <NuGetAuditLevel>low</NuGetAuditLevel>
  <NuGetAuditMode>all</NuGetAuditMode> <!-- Audit direct + transitive -->
</PropertyGroup>
```

```bash
# Audit NuGet packages for known vulnerabilities
dotnet list package --vulnerable --include-transitive

# Keep packages up to date
dotnet outdated  # requires dotnet-outdated-tool

# Check .NET SDK/runtime support status
dotnet --info
```

**Gotcha:** `NuGetAuditMode` defaults to `direct` -- transitive vulnerabilities are hidden unless
you set `all`. Always use `all` in CI to catch deep dependency issues.

---

## A07: Identification and Authentication Failures

**Vulnerability:** Weak authentication mechanisms, credential stuffing, session fixation, missing
multi-factor authentication.

**Risk in .NET:** Default Identity password policies that are too weak, session cookies without
`Secure`/`SameSite` attributes, missing account lockout configuration.

### Mitigation

```csharp
// Configure strong Identity password and lockout policies
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Account lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
```

```csharp
// Secure cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});
```

**Gotcha:** `CookieSecurePolicy.SameAsRequest` allows cookies over HTTP in development, which is
fine. But in production behind a reverse proxy terminating TLS, the app sees HTTP -- so cookies are
sent insecurely. Always use `CookieSecurePolicy.Always` in production and configure forwarded
headers.

---

## A08: Software and Data Integrity Failures

**Vulnerability:** Code and infrastructure that does not protect against integrity violations --
unsigned packages, insecure CI/CD pipelines, deserialization of untrusted data.

**Risk in .NET:** Using `BinaryFormatter` for deserialization (arbitrary code execution), accepting
unsigned NuGet packages from untrusted feeds, missing package source mapping.

### Mitigation

```xml
<!-- NuGet package source mapping in nuget.config -->
<!-- Only allow packages from trusted sources -->
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="internal" value="https://pkgs.example.com/nuget/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="internal">
      <package pattern="MyCompany.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

```csharp
// NEVER use BinaryFormatter -- it is a critical deserialization vulnerability.
// BinaryFormatter is obsolete as error (SYSLIB0011) in .NET 8+ and removed in .NET 9+.
// Use System.Text.Json instead:
var data = JsonSerializer.Deserialize<OrderDto>(jsonString);

// For binary serialization needs, use MessagePack or Protobuf:
// <PackageReference Include="MessagePack" Version="3.*" />
var bytes = MessagePackSerializer.Serialize(order);
var restored = MessagePackSerializer.Deserialize<Order>(bytes);
```

**Gotcha:** Package source mapping uses most-specific-pattern-wins: `MyCompany.*` beats the `*`
wildcard. Always define specific patterns for internal packages to prevent dependency confusion
attacks.

---

## A09: Security Logging and Monitoring Failures

**Vulnerability:** Insufficient logging of security-relevant events, lack of monitoring for
breaches, inability to detect and respond to active attacks.

**Risk in .NET:** Not logging authentication failures, missing audit trails for sensitive
operations, logging sensitive data (passwords, tokens) in plaintext.

### Mitigation

```csharp
// Log security events with structured logging
public sealed class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var path = context.Request.Path.Value;

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["UserId"] = userId,
            ["RequestPath"] = path,
            ["RemoteIp"] = context.Connection.RemoteIpAddress?.ToString()
        }))
        {
            await next(context);

            // Log failed authentication attempts
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                logger.LogWarning("Authentication failed for {Path}", path);
            }

            // Log authorization failures
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                logger.LogWarning("Authorization denied for {Path}", path);
            }
        }
    }
}
```

```csharp
// NEVER log sensitive data -- redact credentials and PII
// Configure log filtering to exclude sensitive paths
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Warning);

// Use IHttpLoggingInterceptor (.NET 8+) to redact request/response headers
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestMethod
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    // Explicitly exclude request/response bodies and auth headers
});
```

**Gotcha:** Structured logging with `{Placeholder}` syntax is safe, but string interpolation
(`$"User {userId}"`) in log calls bypasses structured logging and may leak PII into log sinks
that do not support redaction.

---

## A10: Server-Side Request Forgery

**Vulnerability:** Application fetches a remote resource based on user-supplied URL without
validation, allowing attackers to reach internal services or metadata endpoints.

**Risk in .NET:** `HttpClient` calls with user-provided URLs, URL redirect following to internal
networks, accessing cloud metadata endpoints (169.254.169.254).

### Mitigation

```csharp
// Validate and restrict outbound URLs
public static class UrlValidator
{
    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "api.example.com",
        "cdn.example.com"
    };

    public static bool IsAllowed(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Block non-HTTPS
        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;

        // Block private/internal IPs
        if (IPAddress.TryParse(uri.Host, out var ip))
        {
            if (IsPrivateOrReserved(ip))
                return false;
        }

        // Allowlist hosts
        return AllowedHosts.Contains(uri.Host);
    }

    private static bool IsPrivateOrReserved(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();
        return bytes[0] switch
        {
            10 => true,                                         // 10.0.0.0/8
            127 => true,                                        // 127.0.0.0/8
            169 when bytes[1] == 254 => true,                   // 169.254.0.0/16 (link-local / cloud metadata)
            172 when bytes[1] >= 16 && bytes[1] <= 31 => true,  // 172.16.0.0/12
            192 when bytes[1] == 168 => true,                   // 192.168.0.0/16
            _ => false
        };
    }
}

// Usage in an endpoint
app.MapPost("/fetch", async (FetchRequest request, IHttpClientFactory factory) =>
{
    if (!UrlValidator.IsAllowed(request.Url))
        return Results.BadRequest("URL not allowed");

    var client = factory.CreateClient();
    var response = await client.GetStringAsync(request.Url);
    return Results.Ok(response);
});
```

```csharp
// Configure HttpClient to disable automatic redirect following
builder.Services.AddHttpClient("external", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    AllowAutoRedirect = false // Prevent redirect-based SSRF
});
```

**Gotcha:** DNS rebinding can bypass IP allowlists -- an attacker's domain resolves to a public IP
during validation but to an internal IP during the actual request. Pin DNS resolution or re-validate
after connection.
