# Create PRD

## Feature file: $ARGUMENTS

Generate a complete PRD for general feature implementation with thorough research. Ensure context is passed to the AI agent to enable self-validation and iterative refinement. Read the feature file first to understand what needs to be created, how the examples provided help, and any other considerations.

The AI agent only gets the context you are appending to the PRD and training data. Assuma the AI agent has access to the codebase and the same knowledge cutoff as you, so its important that your research findings are included or referenced in the PRD. The Agent has Websearch capabilities, so pass urls to documentation and examples.

## Generally important

Always use:
- serena for semantic code retrieval and editing tools
- context7 for up to date documentation on third party code
- playwright for UI analysis and testing
- sequential thinking for any decision making
- dotnet-code-standards skill for source code formatting and documenting (if .NET / C# project)
- dotnet-colocated-localization for localization tasks (if .NET / C# project)
- directus-dotnet-repository for data access functionality using Directus CMS
- evanto-paypal-client for PayPal payment and subscription related tasks
- check also for other helpful plugins and skills

Read the CLAUDE.md root file before you do anything.

## Research Process

1. **Codebase Analysis**
   - Search for similar features/patterns in the codebase
   - Identify files to reference in PRD
   - Note existing conventions to follow
   - Check test patterns for validation approach

2. **External Research**
   - Search for similar features/patterns online
   - Library documentation (include specific URLs)
   - Implementation examples (GitHub/StackOverflow/blogs)
   - Best practices and common pitfalls

3. **User Clarification** (if needed)
   - Specific patterns to mirror and where to find them?
   - Integration requirements and where to find them?

## PRD Generation

Using prds/templates/prd-base-python.md (Python projects) or prds/templates/prd-base-csharp.md (.NET projects) as template:

### Critical Context to Include and pass to the AI agent as part of the PRD
- **Documentation**: URLs with specific sections
- **Code Examples**: Real snippets from codebase
- **Gotchas**: Library quirks, version issues
- **Patterns**: Existing approaches to follow

### Implementation Blueprint
- Start with pseudocode showing approach
- Reference real files for patterns
- Include error handling strategy
- list tasks to be completed to fullfill the PRD in the order they should be completed
- Always use dotnet-code-standards skill for any code implementation
- If there are multiple implementation options, then prefer performant (fast) and clear (simple) solutions 

### Validation Gates (Must be Executable) eg for 

.NET:

```bash
# Syntax/Style
dotnet build --no-restore --warnaserror

# Unit Tests
dotnet test --no-build 

```

Python:

```python
# Syntax/Style
ruff check --fix && mypy .

# Unit Tests
uv run pytest tests/ -v

```

*** CRITICAL AFTER YOU ARE DONE RESEARCHING AND EXPLORING THE CODEBASE BEFORE YOU START WRITING THE PRD ***

*** ULTRATHINK ABOUT THE PRD AND PLAN YOUR APPROACH THEN START WRITING THE PRD ***

## Output
Save as: `prds/{feature-name}-prd.md`

## Quality Checklist
- [ ] All necessary context included
- [ ] Validation gates are executable by AI
- [ ] References existing patterns
- [ ] Clear implementation path
- [ ] Error handling documented
- [ ] dotnet-code-standards skill applied

Score the PRD on a scale of 1-10 (confidence level to succeed in one-pass implementation using claude codes)

Remember: The goal is one-pass implementation success through comprehensive context.