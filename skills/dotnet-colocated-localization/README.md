# .NET Co-Located Resource Localization Agent Skill

An Agent Skill for implementing a custom co-located localization system in ASP.NET Core applications (Razor Pages and Blazor).

## What This Skill Does

This skill helps AI agents implement a complete localization system that:

- Keeps `.resx` resource files next to their corresponding views (co-located pattern)
- Detects user language from URL path segments (e.g., `/de/kontakt`, `/en/contact`)
- Provides a custom `IViewLocalizer` implementation that automatically resolves resources
- Includes a language switcher helper with intelligent URL translation
- Handles localized route slugs (e.g., `/de/kontakt` ↔ `/en/contact`)

## Quick Start

### For AI Agents

This skill is designed to be loaded by compatible AI agents (Claude, Cursor, etc.) to help them implement localization in .NET projects.

**Agent command:**
```
Use the dotnet-colocated-localization skill to add internationalization to this ASP.NET Core application.
```

### For Developers

1. Copy the `scripts/infrastructure-classes.md` implementations to your project
2. Follow the step-by-step instructions in `SKILL.md`
3. Use the examples in `references/EXAMPLES.md` as templates
4. Customize route mappings in `PrefixLanguageSwitchHelper.cs`

## What's Included

```
dotnet-colocated-localization/
├── SKILL.md                          # Main skill definition
├── README.md                         # This file
├── scripts/
│   └── infrastructure-classes.md     # Complete C# implementations
├── references/
│   └── EXAMPLES.md                   # Complete working examples
└── assets/
    ├── template.de.resx              # German resource template
    └── template.en.resx              # English resource template
```

## Key Features

### 1. Co-Located Resources
Resource files live next to their views for better organization:
```
Pages/Contact/
├── Index.cshtml
└── Resources/
    ├── Index.de.resx
    └── Index.en.resx
```

### 2. Route-Based Culture Detection
Culture is automatically detected from the URL:
- `/de/kontakt` → German culture
- `/en/contact` → English culture

### 3. Intelligent Language Switching
The language switcher handles three scenarios:
- **Direct mappings**: `/de/kontakt` ↔ `/en/contact`
- **Generic swaps**: `/de/blog` → `/en/blog`
- **Slug suffixes**: `/de/blog/article-de` → `/en/blog/article-en`

### 4. Clean API
Simple service registration and middleware configuration:
```csharp
builder.Services.AddColocatedViewLocalization(typeof(Program).Assembly);
app.UseColocatedRequestLocalization("de", ["de", "en"]);
```

## Requirements

- .NET 6.0 or higher
- ASP.NET Core (Razor Pages or Blazor)
- Standard localization packages (included in ASP.NET Core)

## Usage in Views

```cshtml
@inject IViewLocalizer Localizer

<h1>@Localizer["PageTitle"]</h1>
<label>@Localizer["Label_Email"]</label>
<button>@Localizer["Button_Submit"]</button>
```

With format arguments:
```cshtml
<p>@Localizer["Greeting", Model.UserName]</p>
```

## Agent Skill Compatibility

This skill follows the [Agent Skills](https://agentskills.io) specification and is compatible with:
- Claude Code
- Claude Desktop
- Cursor
- Other Agent Skills-compatible tools

## Examples Included

The skill includes complete examples for:
1. **Contact Form** - Form with validation messages
2. **Blog Post** - Dynamic content with metadata
3. **Navigation Menu** - Localized menu with language switcher
4. **Error Page** - Culture-aware error messages

See `references/EXAMPLES.md` for full implementations.

## Resource Key Naming Conventions

The skill uses consistent prefixes:
- `Label_*` - Form labels and field names
- `Error_*` - Validation and error messages
- `Button_*` - Buttons and actions
- `Option_*` - Radio/checkbox options
- `Placeholder_*` - Input placeholders
- `Menu_*` - Navigation items

## Architecture

### Components

1. **PrefixColocatedViewLocalizer** - Maps view paths to resource locations
2. **PrefixRouteSegmentRequestCultureProvider** - Extracts culture from URL
3. **PrefixLanguageSwitchHelper** - Translates URLs between languages
4. **PrefixLocalizationExtensions** - Simplifies service registration

### Request Flow

```
Request: /de/kontakt
  ↓
PrefixRouteSegmentRequestCultureProvider extracts "de"
  ↓
RequestLocalizationMiddleware sets culture
  ↓
View renders with IViewLocalizer
  ↓
PrefixColocatedViewLocalizer resolves resources
  ↓
String loaded from Index.de.resx
```

## Customization

### Adding New Languages

1. Update supported cultures: `["de", "en", "fr"]`
2. Add route mappings for the new culture
3. Create `.fr.resx` files for all views
4. Update language switcher component

### Customizing Route Mappings

Edit the dictionaries in `PrefixLanguageSwitchHelper.cs`:
```csharp
private static readonly Dictionary<String, String> mRouteMapDeToEn = new()
{
    { "/de/kontakt", "/en/contact" },
    // Add your mappings
};
```

## Skill Validation

To validate this skill using the official `skills-ref` tool:

```bash
# Clone and install skills-ref (Python-based)
git clone https://github.com/agentskills/agentskills.git
cd agentskills/skills-ref
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\Activate.ps1
pip install -e .

# Validate the skill
skills-ref validate path/to/dotnet-colocated-localization
```

## Testing

The implementation is production-tested in the JM.BudoAcademy project with:
- Multiple localized pages (Contact, Imprint, Privacy, Blog)
- Dynamic routing with culture segments
- Language switching across all pages
- Partial views and components

## License

MIT License - Feel free to use in your projects

## Credits

Originally developed for JM.BudoAcademy project and packaged as a reusable Agent Skill.

## Support

For issues or questions about this skill:
1. Check the troubleshooting section in `SKILL.md`
2. Review the complete examples in `references/EXAMPLES.md`
3. Refer to the reference implementation in the JM.BudoAcademy project

## Version

1.0.0 - Initial release

## Contributing

This skill is open for improvements and extensions. Common enhancements:
- Support for more than two languages
- RTL language support
- Database-backed translations
- Translation management UI
