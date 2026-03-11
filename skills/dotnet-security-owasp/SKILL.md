---
name: dotnet-security-owasp
description: >
  OWASP Top 10 (2021) security hardening for .NET applications. Covers injection prevention,
  broken access control, XSS, SSRF, cryptographic failures, security misconfiguration, and
  deprecated security API warnings (CAS, APTCA, BinaryFormatter, .NET Remoting, DCOM).
  TRIGGER when: reviewing or writing .NET code for security vulnerabilities, hardening ASP.NET Core
  APIs, auditing OWASP compliance, migrating deprecated security patterns, or adding security
  headers/rate limiting/anti-forgery middleware.
  DO NOT TRIGGER when: implementing auth flows (see dotnet-api-security), Blazor auth UI
  (see dotnet-blazor-auth), cryptographic algorithm selection (see dotnet-cryptography),
  or secrets management (see dotnet-secrets-management).
license: MIT
---

# dotnet-security-owasp

OWASP Top 10 (2021) security guidance for .NET applications. Each category includes the
vulnerability description, .NET-specific risk, mitigation code examples, and common pitfalls.
This skill is the canonical owner of deprecated security pattern warnings (CAS, APTCA, .NET
Remoting, DCOM, BinaryFormatter).

## Scope

- OWASP Top 10 (2021) vulnerability categories with .NET-specific mitigations
- Injection, broken access control, XSS, SSRF prevention patterns
- Deprecated security API warnings (CAS, APTCA, BinaryFormatter, .NET Remoting)
- Security header configuration and CORS hardening
- Rate limiting and anti-forgery middleware patterns
- NuGet package audit and dependency vulnerability scanning

## Out of Scope

- Authentication/authorization implementation -- see [skill:dotnet-api-security]
- Blazor auth UI -- see [skill:dotnet-blazor-auth]
- Cryptographic algorithm selection -- see [skill:dotnet-cryptography]
- Configuration binding and Options pattern -- see [skill:dotnet-csharp-configuration]
- Secrets storage and management -- see [skill:dotnet-secrets-management]

## OWASP Top 10 Categories

Detailed code examples and mitigations for each category are in
[references/owasp-categories.md](references/owasp-categories.md):

| Category | .NET Risk |
|---|---|
| A01: Broken Access Control | Missing `[Authorize]`, IDOR, CORS misconfiguration |
| A02: Cryptographic Failures | MD5/SHA1 passwords, plaintext secrets, HTTP transport |
| A03: Injection | SQL concatenation, `Process.Start`, raw HTML rendering |
| A04: Insecure Design | No rate limiting, unbounded uploads, missing anti-forgery |
| A05: Security Misconfiguration | Dev exception page in prod, missing security headers |
| A06: Vulnerable Components | Outdated NuGet packages, unsupported .NET versions |
| A07: Auth Failures | Weak password policies, insecure cookies |
| A08: Integrity Failures | BinaryFormatter, unsigned packages, dependency confusion |
| A09: Logging Failures | Missing audit trails, logging PII/tokens |
| A10: SSRF | Unvalidated outbound URLs, redirect following |

---

## Deprecated Security Patterns

This skill is the **canonical owner** of deprecated security pattern warnings. Other skills
should cross-reference here rather than duplicating these warnings.

### Code Access Security (CAS)

CAS is **not supported** in .NET Core/.NET 5+. Code that references `System.Security.Permissions`,
`SecurityPermission`, or `[SecurityCritical]`/`[SecuritySafeCritical]` attributes for CAS purposes
must be removed or replaced with OS-level security boundaries (containers, process isolation).

### AllowPartiallyTrustedCallers (APTCA)

The `[AllowPartiallyTrustedCallers]` attribute has **no effect** in .NET Core/.NET 5+. The
partial-trust model is gone. Remove APTCA attributes during migration. Use standard authorization
and input validation instead.

### .NET Remoting

.NET Remoting is **not available** in .NET Core/.NET 5+. It was inherently insecure due to
unrestricted deserialization of remote objects. Replace with:
- gRPC for cross-process/cross-machine RPC
- Named pipes for same-machine IPC
- HTTP APIs for service-to-service communication

### DCOM

Distributed COM (DCOM) is **Windows-only and not supported** in .NET Core/.NET 5+. Replace with
gRPC, REST APIs, or message queues for distributed communication.

### BinaryFormatter

`BinaryFormatter` is **obsolete as error** (SYSLIB0011) in .NET 8 and **removed** in .NET 9+.
It enables arbitrary code execution through deserialization attacks. Replace with:
- `System.Text.Json` for JSON serialization
- MessagePack or Protocol Buffers for binary formats
- `XmlSerializer` with strict type allowlists for XML scenarios

Do **not** set `System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization` to `true`
as a workaround.

---

## Agent Gotchas

1. **Do not use `[AllowAnonymous]` without explicit justification** -- it overrides the global fallback policy. Mark each anonymous endpoint with a comment explaining why.
2. **Do not disable HTTPS redirection for convenience** -- use `dotnet dev-certs https --trust` for local development instead.
3. **Do not log raw request bodies** -- they may contain credentials, tokens, or PII. Use `HttpLoggingFields` to select safe fields.
4. **Do not rely solely on client-side validation** -- always validate on the server. Razor form validation is for UX, not security.
5. **Do not use `FromSqlRaw` with string interpolation** -- use `FromSqlInterpolated` which auto-parameterizes.
6. **Do not store secrets in `appsettings.json`** -- use user secrets for development and environment variables or managed identity for production. See [skill:dotnet-secrets-management].
7. **Do not generate security-sensitive code using deprecated patterns** -- CAS, APTCA, .NET Remoting, DCOM, and BinaryFormatter are all unsupported in modern .NET. See the Deprecated Security Patterns section above.

---

## Prerequisites

- .NET 8.0+ (LTS baseline)
- ASP.NET Core 8.0+ for security middleware, anti-forgery, and rate limiting
- Microsoft.AspNetCore.Identity for authentication/identity (if using A07 patterns)

## References

- [OWASP Top 10 (2021)](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/?view=aspnetcore-10.0)
- [Secure Coding Guidelines for .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)
- [Security in .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- [ASP.NET Core Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-10.0)
- [Rate Limiting Middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0)
- [NuGet Package Source Mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [BinaryFormatter Migration Guide](https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide/)
