# C# .NET Code Standards Skill

A comprehensive Agent Skill for enforcing C# .NET coding standards, formatting conventions, and best practices in the current project and Evanto Media shared libraries.

## Overview

This skill provides Claude with detailed guidance on:

- **Naming Conventions**: Member variables, types, classes, namespaces
- **Formatting Patterns**: Alignment, spacing, documentation
- **Modern C# 12–14 Features**: Primary constructors, collection expressions, records, `field` keyword, extension members, null-conditional assignment, `Lock` type, `params` collections, partial members, implicit span conversions
- **Performance Patterns**: LoggerMessage delegates, async/await, LINQ optimization
- **Testing Standards**: xUnit patterns, test structure, naming conventions
- **Code Quality**: Guard clauses, error handling, disposable patterns

## Quick Start

1. **Read the SKILL.md**: Contains complete instructions and patterns
2. **Check QUICKREF.md**: Quick reference card for common patterns
3. **Review Examples**: See `examples/` directory for working code samples

## Files

- `SKILL.md` - Complete skill instructions and guidelines
- `QUICKREF.md` - Quick reference card for common patterns
- `README.md` - This file
- `examples/` - Working code examples demonstrating all patterns:
  - `class-structure.cs` - Complete class with all conventions applied
  - `async-patterns.cs` - Async/await best practices
  - `logging-patterns.cs` - LoggerMessage delegate examples
  - `test-patterns.cs` - xUnit test structure and patterns
  - `guard-clauses.cs` - Modern guard clause patterns
  - `linq-patterns.cs` - Efficient LINQ usage examples

## Key Patterns

### Member Variables

```csharp
// ✅ m prefix + PascalCase, readonly when possible
private readonly ILogger<Service> mLogger;
private readonly String mUserName;
private DateTime mLastUpdate = DateTime.Now;

// ✅ Local variables: camelCase + var
var userName = "John";
var userID = 123;  // Uppercase abbreviations at end
```

### Logging with LoggerMessage

```csharp
#region LoggerMessage

[LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error: {UserID}")]
private partial void LogError(Exception ex, String userID);

#endregion

// Usage
LogError(ex, userID);
```

### Async/Await

```csharp
public async Task<User> GetUserAsync(
    Int32 id,
    CancellationToken ct = default)
{
    return await mRepo
        .FindAsync(id, ct)
        .ConfigureAwait(false);
}
```

### Guard Clauses

```csharp
public void Process(String data, Int32 count)
{   // check requirements
    ArgumentException.ThrowIfNullOrEmpty(data);
    ArgumentOutOfRangeException.ThrowIfNegative(count);
}
```

## When to Use This Skill

- Writing new C# classes or methods
- Reviewing or refactoring existing C# code
- Implementing services, providers, or view models
- Creating unit tests with xUnit
- Adding logging to applications
- Optimizing LINQ queries
- Ensuring code consistency across the project

## Project Context

- **Target Framework**: .NET 10.0
- **C# Version**: 14 (covers features from C# 12, 13, and 14)
- **Project Prefixes**:
  - `Ev` - Evanto Media shared libraries
  - `Jm` - JM Budoacademy project
  - `Srs` - Shorin Ryu Seibukan projects
  - ..
- **Test Framework**: xUnit v3
- **Logging**: Microsoft.Extensions.Logging with LoggerMessage delegates

## Examples Directory

Each example file demonstrates specific patterns:

1. **class-structure.cs**
   - Complete class structure with primary constructor
   - Member variable organization and documentation
   - Method implementation patterns
   - IDisposable implementation
   - LoggerMessage region

2. **async-patterns.cs**
   - Correct async method signatures
   - CancellationToken usage
   - ConfigureAwait(false) in libraries
   - Parallel async operations with WhenAll
   - Timeout patterns

3. **logging-patterns.cs**
   - LoggerMessage delegate definitions
   - All log levels (Trace, Debug, Info, Warning, Error, Critical)
   - Structured logging with parameters
   - Custom formatting options

4. **test-patterns.cs**
   - xUnit test class structure
   - Test naming: MethodName_Scenario_ExpectedBehavior
   - Arrange-Act-Assert pattern
   - Mock setup with Moq
   - Theory tests with InlineData

5. **guard-clauses.cs**
   - ArgumentNullException.ThrowIfNull
   - ArgumentException.ThrowIfNullOrEmpty/WhiteSpace
   - ArgumentOutOfRangeException throw helpers
   - ObjectDisposedException.ThrowIf
   - Custom validation patterns

6. **linq-patterns.cs**
   - Efficient LINQ method syntax
   - Avoiding multiple enumeration
   - Proper materialization with ToList()
   - GroupBy, Join, SelectMany patterns
   - Deferred execution awareness

## Common Mistakes to Avoid

❌ **Wrong:**
```csharp
var mTelemetryEnabled;        // Don't mix m prefix with var
private string userName;      // Use .NET types: String
var userId = 123;             // Should be userID
var count = (Int32)value;     // No space after cast
mLogger.LogInfo("text");      // Use LoggerMessage
public async void Process()   // Should return Task
var hasItems = items.Count() > 0; // Use Any()
```

✅ **Correct:**
```csharp
private Boolean mTelemetryEnabled;
private String mUserName;
var userID = 123;
var count = (Int32) value;
LogInfoMessage();
public async Task ProcessAsync()
var hasItems = items.Any();
```

## Validation

To validate this skill structure:

```bash
/Users/svk/dev/tools/agentskills/skills-ref/skills-ref validate /Users/svk/dev/kunden/jm/.claude/skills/dotnet-code-standards
```

## Version

1.1.0 - Added comprehensive C# 12–14 language features (February 2025)
1.0.0 - Initial release based on evanto coding standards (January 2025)

## Contributing

When updating this skill:
1. Update SKILL.md with new patterns or rules
2. Add corresponding examples to the examples/ directory
3. Update QUICKREF.md if adding commonly-used patterns
4. Ensure examples compile and follow all documented rules
5. Run validation tool to ensure skill structure is correct

## Cross-References

This skill complements:
- `jm-design-guide` - Frontend design system standards
- `dotnet-colocated-localization` - Localization patterns
- `CLAUDE.md` - Project architecture and structure

## License

Internal use - Evanto Media GmbH
