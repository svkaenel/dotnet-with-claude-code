name: "Base PRD Template (Python) - Context-Rich with Validation Loops"
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

- file: [path/to/example.py]
  why: [Pattern to follow, gotchas to avoid]

- doc: [Library documentation URL]
  section: [Specific section about common pitfalls]
  critical: [Key insight that prevents common errors]

- docfile: [PRDs/ai_docs/file.md]
  why: [docs that the user has pasted in to the project]

```

### Current Codebase tree (run `tree` in the root of the project) to get an overview of the codebase
```bash

```

### Desired Codebase tree with files to be added and responsibility of file
```bash

```

### Known Gotchas of our codebase & Library Quirks
```python
# CRITICAL: [Library name] requires [specific setup]
# Example: BeautifulSoup4 requires an explicit parser argument (use "lxml" or "html.parser")
# Example: exchangelib Autodiscover can be slow - cache credentials object
# Example: We use Pydantic v2 - use model_validate() not parse_obj()
# Example: litellm provider strings must be prefixed (e.g. "anthropic/claude-...")
```

## Implementation Blueprint

### Data models and structure

Create the core data models, we ensure type safety and consistency.
```python
# Examples:
#  - Pydantic models (BaseModel)
#  - dataclasses
#  - SQLAlchemy / SQLModel ORM models
#  - TypedDict for lightweight typing

```

### list of tasks to be completed to fullfill the PRD in the order they should be completed

```yaml
Task 1:
MODIFY src/mypackage/module.py:
  - FIND pattern: "class OldImplementation"
  - INJECT after line containing "__init__"
  - PRESERVE existing method signatures

CREATE src/mypackage/new_module.py:
  - MIRROR pattern from: src/mypackage/other_module.py
  - MODIFY class name and core logic
  - KEEP error handling pattern identical

...(...)

Task N:
...

```


### Per task pseudocode as needed added to each task
```python

# Task 1
# Pseudocode with CRITICAL details - dont write entire code

class NewsletterProcessor:
    """Process newsletter emails into Obsidian markdown.

    # PATTERN: Always add docstrings to classes and public methods.
    # PATTERN: Use Google-style docstrings consistently.
    """

    # PATTERN: Use type hints on all function signatures
    # PATTERN: Use default values via field() for mutable defaults
    def __init__(
        self,
        config: AppConfig,
        llm_client: LLMClient,
        logger: logging.Logger | None = None,
    ) -> None:
        # PATTERN: Always validate inputs first
        if not config.senders:
            raise ValueError("At least one sender must be configured")

        self._config = config
        self._llm = llm_client
        self._logger = logger or logging.getLogger(__name__)

    async def process_newsletter(
        self,
        email: RawEmail,
        *,
        timeout_seconds: int = 120,
    ) -> ProcessingResult:
        """Fetch and convert a single newsletter email.

        Args:
            email: The raw email to process.
            timeout_seconds: Timeout for LLM calls. Defaults to 120.

        Returns:
            ProcessingResult with status, markdown content, and metadata.

        Raises:
            ProcessingError: If the email cannot be processed after retries.
        """
        # PATTERN: Always use structured error handling
        try:
            html_body = self._extract_html(email)
            cleaned = self._clean_html(html_body)
            markdown = self._convert_to_markdown(cleaned)

            # PATTERN: Use async for I/O-bound operations
            result = await asyncio.wait_for(
                self._llm.analyze_and_polish(markdown),
                timeout=timeout_seconds,
            )

            self._logger.info(
                "Processed newsletter from %s: %s",
                email.sender,
                result.title,
            )
            return result

        except asyncio.TimeoutError:
            self._logger.error(
                "LLM timeout processing email from %s", email.sender
            )
            raise ProcessingError(f"Timeout after {timeout_seconds}s")

        except Exception as ex:
            self._logger.exception(
                "Failed to process email from %s", email.sender
            )
            raise ProcessingError(str(ex)) from ex
```

## Validation Loop

### Level 1: Dependency Install & Static Analysis

Run these first - fix any errors or style violations before writing tests.

1. Install dependencies (use a virtual environment)

```bash
python -m venv .venv
source .venv/bin/activate       # macOS/Linux
pip install -e ".[dev]"         # install project + dev dependencies
```

2. Type checking with mypy or pyright
- Catches type errors, missing annotations, and incorrect usage
- Ensures all function signatures have proper type hints

```bash
mypy src/ --strict
# or
pyright src/
```

3. Linting with ruff (replaces flake8, isort, pyflakes, and more)
- Auto-fix what you can (imports, formatting, style)
- Fails if there are any remaining violations

```bash
ruff check src/ tests/ --fix    # lint + auto-fix
ruff format src/ tests/         # format (replaces black)
```

4. Verify formatting is clean (CI mode)

```bash
ruff check src/ tests/
ruff format src/ tests/ --check
```

Expected:
- mypy/pyright returns exit 0 with no errors.
- ruff check and ruff format --check both return exit 0.

If either fails, read the errors, apply fixes (or update your pyproject.toml config), and re-run.


### Level 2: Unit Tests

Follow your existing Python test conventions, using pytest plus standard mocking patterns.

1. Create test files in `tests/` mirroring the `src/` structure.
2. Reference your modules and use `unittest.mock` or `pytest-mock` for external dependencies.

```python
"""Tests for newsletter processing."""

import pytest
from unittest.mock import AsyncMock, MagicMock, patch

from mypackage.processing import NewsletterProcessor
from mypackage.config import AppConfig
from mypackage.models import RawEmail, ProcessingResult


class TestNewsletterProcessor:
    """Tests for the NewsletterProcessor class."""

    @pytest.fixture
    def config(self) -> AppConfig:
        """Create a test configuration."""
        return AppConfig(
            senders=[{"address": "test@example.com", "name": "Test"}],
            llm={"default_provider": "openai"},
        )

    @pytest.fixture
    def processor(self, config: AppConfig) -> NewsletterProcessor:
        """Create a processor instance with mocked LLM client."""
        llm_client = AsyncMock()
        return NewsletterProcessor(config=config, llm_client=llm_client)

    def test_happy_path_returns_success(self, processor: NewsletterProcessor) -> None:
        """Valid input produces a successful result."""
        email = RawEmail(sender="test@example.com", html="<p>Hello</p>")
        result = processor.process_newsletter(email)

        assert result.status == "success"

    def test_empty_config_raises_validation_error(self) -> None:
        """Empty sender list raises ValueError."""
        with pytest.raises(ValueError, match="at least one sender"):
            NewsletterProcessor(
                config=AppConfig(senders=[], llm={}),
                llm_client=AsyncMock(),
            )

    @pytest.mark.asyncio
    async def test_llm_timeout_handled_gracefully(
        self, processor: NewsletterProcessor
    ) -> None:
        """LLM timeout produces a ProcessingError, not an unhandled exception."""
        processor._llm.analyze_and_polish = AsyncMock(
            side_effect=asyncio.TimeoutError
        )
        email = RawEmail(sender="test@example.com", html="<p>Hello</p>")

        with pytest.raises(ProcessingError, match="Timeout"):
            await processor.process_newsletter(email, timeout_seconds=1)
```

3. Run tests via the CLI:

```bash
pytest tests/ -v --tb=short
```

4. Run tests with coverage:

```bash
pytest tests/ -v --cov=src/mypackage --cov-report=term-missing
```

Expected:
- All tests pass.
- Any failure indicates either a logic bug or a missing test case pattern.

Best Practices & Notes

- **Dependency Injection**: Depend on protocols/ABCs (e.g. `MailProvider` ABC) so you can mock in tests.
- **Project Structure**: Mirror your `src/` folder structure in `tests/` (e.g. `src/mypackage/mail/` -> `tests/mail/`).
- **Naming**:
  - Test functions as `test_[scenario]_[expected_outcome]` (e.g. `test_empty_input_raises_validation_error`).
  - Test files as `test_[module_name].py` matching the source module.
  - Group related tests in classes: `class TestClassName:`.
- **Async Tests**: Use `pytest-asyncio` with `@pytest.mark.asyncio` for async test functions.
- **Fixtures**: Use `@pytest.fixture` for reusable setup; prefer fixtures over `setUp`/`tearDown`.
- **Coverage**: Aim for >= 80% coverage on new features; catch edge cases (None, empty strings, large inputs, concurrency).
- **CI Integration**: Hook these commands into your pipeline (GitHub Actions, etc.) so no PR can merge with failures.

## Final validation Checklist
- [ ] All tests pass: `pytest tests/ -v`
- [ ] Type checks pass: `mypy src/ --strict`
- [ ] No lint errors: `ruff check src/ tests/`
- [ ] Formatting clean: `ruff format src/ tests/ --check`
- [ ] Manual test successful: [specific CLI command or curl]
- [ ] Error cases handled gracefully
- [ ] Logs are informative but not verbose
- [ ] Documentation updated if needed
- [ ] CLAUDE.md updated (important!)

---

## Anti-Patterns to Avoid
- Don't create new patterns when existing ones work
- Don't skip validation because "it should work"
- Don't ignore failing tests - fix them
- Don't use sync calls inside async functions (blocks the event loop)
- Don't hardcode values that should be config
- Don't use bare `except:` - always catch specific exceptions
- Don't use mutable default arguments (`def f(items=[])`)
- Don't ignore type hints - use `mypy --strict` to enforce
- Don't mix `print()` and `logging` - use logging consistently
- Don't forget `if __name__ == "__main__":` guards in executable modules
