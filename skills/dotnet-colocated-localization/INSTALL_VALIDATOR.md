# Installing and Using the Skills Validation Tool

This guide explains how to install and use the official `skills-ref` validation tool for Agent Skills.

## About skills-ref

`skills-ref` is the official Python-based reference implementation and validation tool for the Agent Skills specification. It's maintained in the [agentskills/agentskills](https://github.com/agentskills/agentskills/tree/main/skills-ref) repository.

**Note:** The validation tool is a Python package, **not** an NPM package. There is no `@agentskills/skills-ref` on NPM.

## Installation

### Option 1: From GitHub (Recommended)

```bash
# Clone the repository
git clone https://github.com/agentskills/agentskills.git
cd agentskills/skills-ref

# Create virtual environment
python -m venv .venv

# Activate virtual environment
# On macOS/Linux:
source .venv/bin/activate
# On Windows PowerShell:
.venv\Scripts\Activate.ps1
# On Windows Command Prompt:
.venv\Scripts\activate.bat

# Install skills-ref
pip install -e .
```

### Option 2: Using uv (Alternative)

```bash
cd agentskills/skills-ref
uv sync
source .venv/bin/activate
```

### Option 3: From PyPI (When Available)

```bash
pip install skills-ref
```

**Note:** As of January 2026, the package may not be published to PyPI yet. Use Option 1 for the most reliable installation.

## Usage

Once installed, the `skills-ref` command will be available in your activated virtual environment.

### 1. Validate a Skill

Validates the SKILL.md frontmatter and structure:

```bash
skills-ref validate path/to/your-skill
```

**Example:**
```bash
skills-ref validate /Users/svk/dev/kunden/jm/dotnet-colocated-localization
```

**What it checks:**
- ✅ SKILL.md exists
- ✅ Valid YAML frontmatter
- ✅ Required fields present (`name`, `description`)
- ✅ Name follows format rules (lowercase, hyphens, no consecutive hyphens)
- ✅ Name matches directory name
- ✅ Description within length limits (<1024 chars)
- ✅ Optional fields properly formatted

### 2. Read Skill Properties

Outputs skill metadata as JSON:

```bash
skills-ref read-properties path/to/your-skill
```

**Example output:**
```json
{
  "name": "dotnet-colocated-localization",
  "description": "Implements co-located resource localization...",
  "license": "MIT",
  "compatibility": "Requires .NET 6.0+ ASP.NET Core application",
  "metadata": {
    "author": "JM.BudoAcademy",
    "version": "1.0.0",
    "framework": "ASP.NET Core"
  }
}
```

### 3. Generate Agent Prompts

Converts skills to XML format for agent system prompts:

```bash
skills-ref to-prompt path/to/skill-a path/to/skill-b
```

**Example:**
```bash
skills-ref to-prompt ./dotnet-colocated-localization ./another-skill
```

**Output format:**
```xml
<available_skills>
  <skill>
    <skill_name>dotnet-colocated-localization</skill_name>
    <description>Implements co-located resource localization...</description>
    <skill_path>./dotnet-colocated-localization</skill_path>
  </skill>
  ...
</available_skills>
```

## Python API Usage

You can also use skills-ref programmatically in Python:

```python
from pathlib import Path
from skills_ref import validate, read_properties, to_prompt

# Validate a skill
skill_path = Path("./dotnet-colocated-localization")
is_valid = validate(skill_path)

# Read properties
properties = read_properties(skill_path)
print(properties["name"])
print(properties["description"])

# Generate prompt XML
skills = [Path("./skill-a"), Path("./skill-b")]
xml_prompt = to_prompt(skills)
print(xml_prompt)
```

## Validating This Skill

To validate the `dotnet-colocated-localization` skill:

```bash
# Install skills-ref (see above)

# Navigate to the skill directory
cd /Users/svk/dev/kunden/jm

# Validate
skills-ref validate dotnet-colocated-localization
```

**Expected output:**
```
✅ Skill is valid
```

## Troubleshooting

### Command not found

**Issue:** `bash: skills-ref: command not found`

**Solution:** Make sure your virtual environment is activated:
```bash
source .venv/bin/activate  # macOS/Linux
.venv\Scripts\Activate.ps1  # Windows
```

### Python not found

**Issue:** `python: command not found`

**Solution:** Try `python3` instead:
```bash
python3 -m venv .venv
```

### Import errors

**Issue:** Module import errors when running skills-ref

**Solution:** Reinstall in editable mode:
```bash
pip install -e .
```

## Alternative Validation Methods

While `skills-ref` is the official tool, you can also manually validate by checking:

1. **SKILL.md exists** in the skill directory
2. **Valid YAML frontmatter** between `---` delimiters
3. **Required fields:**
   - `name`: lowercase, hyphens only, <64 chars, matches directory
   - `description`: non-empty, <1024 chars
4. **File structure:** Organized with `scripts/`, `references/`, `assets/` (optional)
5. **Relative paths:** All file references use relative paths from skill root

## Resources

- **GitHub Repository**: [agentskills/agentskills](https://github.com/agentskills/agentskills)
- **Specification**: [agentskills.io/specification](https://agentskills.io/specification)
- **Rust Implementation**: [skills-ref-rs](https://crates.io/crates/skills-ref-rs) (alternative)

## Other Tools

### openskills (NPM)

Universal skills loader for AI coding agents:
```bash
npm i -g openskills
```

This is a different tool that works with Agent Skills but is not the official validator.

---

**Last Updated:** January 2026
**Tool Version:** skills-ref (Python reference implementation)
