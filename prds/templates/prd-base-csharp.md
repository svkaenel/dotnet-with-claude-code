name: "Base PRD Template - Context-Rich with Validation Loops"
description: |

## Purpose
Template optimized for AI agents to implement features with sufficient context and self-validation capabilities to achieve working code through iterative refinement.

## Core Principles
1. **Context is King**: Include ALL necessary documentation, examples, and caveats
2. **Validation Loops**: Provide executable tests/lints the AI can run and fix
3. **Information Dense**: Use keywords and patterns from the codebase
4. **Progressive Success**: Start simple, validate, then enhance
5. **Global rules**: Be sure to follow all rules in CLAUDE.md and CodingRules.md

---

## Goal
[What needs to be built - be specific about the end state and desires]

## Why
- [Business value and user impact]
- [Integration with existing features]
- [Problems this solves and for whom]

## What
[User-visible behavior and technical requirements]

### Success Criteria
- [ ] [Specific measurable outcomes]

## All Needed Context

### Documentation & References (list all context needed to implement the feature)
```yaml
# MUST READ - Include these in your context window
- url: [Official API docs URL]
  why: [Specific sections/methods you'll need]
  
- file: [path/to/example.cs]
  why: [Pattern to follow, gotchas to avoid]
  
- doc: [Library documentation URL] 
  section: [Specific section about common pitfalls]
  critical: [Key insight that prevents common errors]

- docfile: [PRPs/ai_docs/file.md]
  why: [docs that the user has pasted in to the project]

```

### Current Codebase tree (run `tree` in the root of the project) to get an overview of the codebase
```bash

```

### Desired Codebase tree with files to be added and responsibility of file
```bash

```

### Known Gotchas of our codebase & Library Quirks
```csharp
# CRITICAL: [Library name] requires [specific setup]
# Example: FastAPI requires async functions for endpoints
# Example: This ORM doesn't support batch inserts over 1000 records
# Example: We use pydantic v2 and  
```

## Implementation Blueprint

### Data models and structure

Create the core data models, we ensure type safety and consistency.
```csharp
Examples: 
 - orm models

```

### list of tasks to be completed to fullfill the PRD in the order they should be completed

```yaml
Task 1:
MODIFY lib/LibName/MyClass.cs:
  - FIND pattern: "class OldImplementation"
  - INJECT after line containing "constructor"
  - PRESERVE existing method signatures

CREATE lib/LibName/NewClass.cs:
  - MIRROR pattern from: lib/MyLib/OtherClass.cs
  - MODIFY class name and core logic
  - KEEP error handling pattern identical

...(...)

Task N:
...

```


### Per task pseudocode as needed added to each task
```csharp

# Task 1
# Pseudocode with CRITICAL details dont write entire code

  ///-------------------------------------------------------------------------------------------------
  /// <summary>   # PATTERN: Always create a meaningful comment above each method. </summary>
  ///
  /// <remarks>   SvK, # PATTERN replace with current date. </remarks>
  ///
  /// <param name="chatClient">        The chat client to test. </param>
  /// <param name="cancellationToken"> Cancellation token. </param>
  /// <param name="timeoutSeconds">    Optional. The timeout in seconds for the test connection. 
  ///   Default is 60 seconds. </param>
  ///
  /// <returns>   True if the connection is successful. </returns>
  ///-------------------------------------------------------------------------------------------------
# PATTERN Format multiple parameters like this
public async Task<Boolean> TestConnectionAsync(
    IChatClient         chatClient,
    CancellationToken   cancellationToken = default,
    Int32               timeoutSeconds    = 60)
{   # PATTERN: Always validate input first
    // check requirements
    ArgumentNullException.ThrowIfNull(chatClient);
    ArgumentNullException.ThrowIfNull(mLogger);

    # PATTERN Always add a try / catch block
    try
    {   
        var testMessages = new[]
        {
            new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, "You are a helpful assistant."),
            new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, "Hello! Just say 'Hi' briefly.")
        };

        // Test without tools first with timeout
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var response = await chatClient.GetResponseAsync(
            testMessages,
            new ChatOptions(),
            cancellationToken: cts.Token
        );

        mLogger.LogInformation("✅ {Provider} connection test successful: {Response}",
            chatClient.ToString(), response.Text?.Trim());

        return true;
    }

    catch (Exception ex)
    {
        mLogger.LogError(ex, "❌ {Provider} connection test failed", chatClient?.ToString());
        return false;
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
- Fails if there are build or type errors
- MSBuild’s TreatWarningsAsErrors ensures no compiler warnings slip through  

```bash
dotnet build --no-restore --warnaserror
```

3. Apply and/or verify code style from .editorconfig

- Auto-fix what you can (whitespace, naming, ordering)
- Fails if there are any remaining violations when using --verify-no-changes  

```bash
dotnet tool install -g dotnet-format         # if not already installed
dotnet format ./MySolution.sln --verify-no-changes
```

Expected:
- Build returns exit 0 with no warnings.
- dotnet format returns exit 0.

If either fails, read the errors, apply fixes (or update your .editorconfig), and re-run.


### Level 2: Unit Tests

Follow your existing C# test conventions (e.g. an *.Tests project next to your feature project), using xUnit plus Moq (or another test‐double library).

1.	Create a test class NewFeatureTests.cs in your MyProject.Tests project.
2.	Reference your feature assembly and any interfaces for external dependencies.

```csharp
using System;
using Moq;                            // for mocking external APIs
using Xunit;                          // xUnit.net framework 
using MyProject;                      // your feature namespace

public class NewFeatureTests
{
    [Fact]
    public void HappyPath_ReturnsSuccess()
    {
        var feature = new NewFeature();  
        var result  = feature.Execute("valid_input");

        Assert.Equal("success", result.Status);
    }

    [Fact]
    public void EmptyInput_ThrowsValidationException()
    {
        var feature = new NewFeature();
        Assert.Throws<ValidationException>(() => feature.Execute(""));
    }

    [Fact]
    public void ExternalApiTimeout_IsHandledGracefully()
    {
        // Arrange: mock the external API interface to throw
        var apiMock = new Mock<IExternalApi>();
        apiMock
          .Setup(api => api.Call(It.IsAny<string>()))
          .Throws<TimeoutException>();

        var feature = new NewFeature(apiMock.Object);

        // Act
        var result = feature.Execute("valid");

        // Assert
        Assert.Equal("error", result.Status);
        Assert.Contains("timeout", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
```

3.	Run tests via the CLI:

Uses all test frameworks in solution (xUnit here)

```bash
dotnet test --no-build 
```

Expected:
- All three tests pass.
- Any failure indicates either a logic bug or a missing test case pattern.

Best Practices & Notes

- Dependency Injection: Always depend on interfaces (e.g. IExternalApi) so you can mock/fake in tests.
- Project Structure: Mirror your production code folder structure in your Tests project.
- Naming:
- Test methods as [MethodUnderTest]_[Scenario]_[ExpectedOutcome].
- Test class per production class (NewFeatureTests vs. NewFeature).
- Coverage: Aim for ≥ 80 % coverage on new features; catch edge cases (nulls, large inputs, concurrency).
- CI Integration: Hook these commands into your pipeline (GitHub Actions, Azure Pipelines, etc.) so no PR can merge with failures.

## Final validation Checklist
- [ ] All tests pass: `dotnet test --no-build `
- [ ] No errors: `dotnet build --no-restore --warnaserror`
- [ ] Manual test successful: [specific curl/command]
- [ ] Error cases handled gracefully
- [ ] Logs are informative but not verbose
- [ ] Documentation updated if needed
- [ ] CLAUDE.md updated (important!)

---

## Anti-Patterns to Avoid
- ❌ Don't create new patterns when existing ones work
- ❌ Don't skip validation because "it should work"  
- ❌ Don't ignore failing tests - fix them
- ❌ Don't use sync functions in async context
- ❌ Don't hardcode values that should be config
- ❌ Don't catch all exceptions - be specific