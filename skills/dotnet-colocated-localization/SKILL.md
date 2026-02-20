---
name: dotnet-colocated-localization
description: Implements co-located resource localization for .NET ASP.NET Core applications with route-based culture detection and language switching. Use when adding internationalization with resource files placed next to views, URL-based language detection (/de/, /en/), and language switcher UI components.
license: MIT
compatibility: Requires .NET 6.0+ ASP.NET Core application (Razor Pages or Blazor)
metadata:
  author: evanto media GmbH
  version: "1.0.0"
  framework: ASP.NET Core
  target: .NET 6.0+
---

# .NET Co-Located Resource Localization

This skill implements a custom localization system for ASP.NET Core applications that keeps `.resx` resource files co-located with their corresponding views, provides route-based culture detection from URL segments (e.g., `/de/kontakt`, `/en/contact`), and includes a language switcher component with intelligent URL translation.

## When to Use This Skill

Use this skill when you need to:
- Add internationalization to a .NET ASP.NET Core Razor Pages or Blazor application
- Keep localization resources organized next to their views instead of in a central location
- Detect the user's language from the URL path (e.g., `/de/` for German, `/en/` for English)
- Provide a language switcher that correctly translates localized route slugs
- Support multiple languages with route-based culture selection

## Core Features

1. **Co-Located Resources**: Resource files live in a `Resources/` subfolder next to each view
2. **Route-Based Culture Detection**: Culture determined from first URL segment (`/de/`, `/en/`)
3. **Custom View Localizer**: Automatically resolves resources based on view path
4. **Language Switch Helper**: Generates correct URLs when switching languages, handling:
   - Direct route mappings (e.g., `/de/kontakt` ↔ `/en/contact`)
   - Generic culture swaps (e.g., `/de/blog` → `/en/blog`)
   - Slug suffix handling (e.g., `/de/blog/article-de` → `/en/blog/article-en`)
5. **Reusable UI Component**: Language switcher partial view

## Implementation Steps

### Step 1: Create Infrastructure Classes

Create a new folder `Localization/` in your infrastructure or shared library project. Add the following four classes:

1. **PrefixColocatedViewLocalizer.cs** - Custom `IViewLocalizer` implementation
2. **PrefixRouteSegmentRequestCultureProvider.cs** - Route-based culture detection
3. **PrefixLanguageSwitchHelper.cs** - URL translation for language switching
4. **PrefixLocalizationExtensions.cs** - Service registration extensions

See `scripts/infrastructure-classes.md` for the complete implementations.

### Step 2: Register Services in Program.cs

Add the localization services to your application startup:

```csharp
using YourNamespace.Localization;

// Add services
builder.Services
    .AddColocatedViewLocalization(typeof(Program).Assembly);

// Configure middleware
app.UseColocatedRequestLocalization("de", ["de", "en"]);
```

**Important**: Place `UseColocatedRequestLocalization()` before `UseStaticFiles()` and `UseRouting()` in your middleware pipeline.

### Step 3: Configure Localized Routes

For Razor Pages, configure route mappings in `Program.cs`:

```csharp
builder.Services
    .AddRazorPages(options =>
    {
        // Map both cultures to the same page
        options.Conventions.AddPageRoute("/Contact/Index", "/en/contact");
        options.Conventions.AddPageRoute("/Contact/Index", "/de/kontakt");

        options.Conventions.AddPageRoute("/Imprint/Index", "/en/imprint");
        options.Conventions.AddPageRoute("/Imprint/Index", "/de/impressum");
    });
```

### Step 4: Update Language Switch Helper Route Mappings

Edit `PrefixLanguageSwitchHelper.cs` to match your localized routes:

```csharp
private static readonly Dictionary<String, String> mRouteMapDeToEn = new(StringComparer.OrdinalIgnoreCase)
{
    { "/de/kontakt",      "/en/contact" },
    { "/de/impressum",    "/en/imprint" },
    { "/de/datenschutz",  "/en/privacy" }
    // Add your routes here
};

private static readonly Dictionary<String, String> mRouteMapEnToDe = new(StringComparer.OrdinalIgnoreCase)
{
    { "/en/contact",  "/de/kontakt" },
    { "/en/imprint",  "/de/impressum" },
    { "/en/privacy",  "/de/datenschutz" }
    // Add your routes here
};
```

### Step 5: Add IViewLocalizer to Views

In your `_ViewImports.cshtml`, add:

```cshtml
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer
```

### Step 6: Create Resource Files

For each view that needs localization:

1. Create a `Resources/` subfolder next to the view
2. Add `.resx` files with the view name and culture suffix

**Example structure:**
```
Pages/Contact/
├── Index.cshtml
└── Resources/
    ├── Index.de.resx
    └── Index.en.resx
```

**Partial views:**
```
Pages/Shared/Partials/
├── PrefixContactForm.cshtml
└── Resources/
    ├── PrefixContactForm.de.resx
    └── PrefixContactForm.en.resx
```

### Step 7: Use Localizer in Views

Access localized strings using the `Localizer` injected in Step 5:

```cshtml
<h1>@Localizer["PageTitle"]</h1>
<label>@Localizer["Label_Name"] *</label>
<button>@Localizer["Button_Submit"]</button>
<div class="error">@Localizer["Error_Required"]</div>
```

With format arguments:
```cshtml
<p>@Localizer["Greeting", Model.UserName]</p>
```

### Step 8: Add Language Switcher Component

Create a partial view for the language switcher:

**Location:** `Pages/Shared/Partials/PrefixLanguageSwitcher.cshtml`

```cshtml
@using YourNamespace.Localization
@inject PrefixLanguageSwitchHelper LanguageHelper

<div class="flex items-center gap-1">
    <a href="@LanguageHelper.GermanUrl"
       class="px-2 py-1 @(LanguageHelper.IsGerman ? "bg-gray-800 text-white" : "text-gray-500")">
        DE
    </a>
    <a href="@LanguageHelper.EnglishUrl"
       class="px-2 py-1 @(LanguageHelper.IsEnglish ? "bg-gray-800 text-white" : "text-gray-500")">
        EN
    </a>
</div>
```

Include it in your layout:
```cshtml
<partial name="Partials/PrefixLanguageSwitcher" />
```

## Resource Key Naming Conventions

Use consistent prefixes for easy organization:

- **Labels:** `Label_FieldName` (e.g., `Label_Email`, `Label_Name`)
- **Options:** `Option_Value` (e.g., `Option_Dojo`, `Option_Individual`)
- **Errors:** `Error_Description` (e.g., `Error_NameRequired`, `Error_EmailInvalid`)
- **Buttons:** `Button_Action` (e.g., `Button_Submit`, `Button_Cancel`)
- **Placeholders:** `Placeholder_Field` (e.g., `Placeholder_Message`)
- **Titles:** `Title_Section` (e.g., `Title_ContactForm`)

## Adding New Languages

To add a new language (e.g., French):

1. Update service registration:
```csharp
app.UseColocatedRequestLocalization("de", ["de", "en", "fr"]);
```

2. Add route mappings for the new culture
3. Create `.fr.resx` files for all views
4. Update `PrefixLanguageSwitchHelper` to support the new culture
5. Add language switcher button for French

## Architecture Overview

### Request Flow

1. **Request arrives** (e.g., `/de/kontakt`)
2. **PrefixRouteSegmentRequestCultureProvider** extracts `"de"` from URL
3. **RequestLocalizationMiddleware** sets `CultureInfo.CurrentCulture`
4. **View renders** and uses `IViewLocalizer`
5. **PrefixColocatedViewLocalizer** determines view path
6. **Resource lookup** in `Pages.Contact.Resources.Index` with culture `"de"`
7. **String returned** from `Index.de.resx`

### Component Responsibilities

- **PrefixColocatedViewLocalizer**: Maps view paths to resource locations
- **PrefixRouteSegmentRequestCultureProvider**: Extracts culture from URL
- **PrefixLanguageSwitchHelper**: Translates current URL to target language
- **PrefixLocalizationExtensions**: Simplifies service registration

## Troubleshooting

### Resources Not Found

**Symptom:** Keys appear in brackets like `[Label_Name]`

**Solutions:**
- Verify `.resx` file naming matches view name exactly
- Check that `Resources/` folder is at the correct level
- Ensure `.resx` files have "Embedded Resource" build action
- Confirm the view path matches the resource namespace structure

### Wrong Culture Displayed

**Symptom:** Always shows default language

**Solutions:**
- Verify `UseColocatedRequestLocalization()` is before `UseRouting()`
- Check URL contains culture segment (e.g., `/de/` not just `/`)
- Confirm culture code is in supported cultures array
- Check browser is not forcing culture via Accept-Language header

### Language Switcher URL Incorrect

**Symptom:** Language switch goes to wrong page or 404

**Solutions:**
- Update route mappings in `PrefixLanguageSwitchHelper`
- Ensure both directions (de→en and en→de) are mapped
- Add route conventions in `Program.cs` for both cultures
- Check for typos in route strings

## Examples

See `references/EXAMPLES.md` for complete examples including:
- Full contact form with validation messages
- Blog post page with localized metadata
- Navigation menu with translated links
- Error pages with culture-aware messages

## File References

- **Infrastructure code**: `scripts/infrastructure-classes.md`
- **Complete examples**: `references/EXAMPLES.md`
- **Resource file templates**: `assets/template.de.resx`, `assets/template.en.resx`

## Best Practices

1. **Keep resource files small**: One file per view/component
2. **Use descriptive keys**: Prefix with context (Label_, Error_, etc.)
3. **Consistent naming**: Match view name exactly in `.resx` filename
4. **Document route mappings**: Comment your language helper mappings
5. **Test both directions**: Verify language switching works de→en and en→de
6. **Organize by feature**: Group related views and resources together
7. **Default to fallback**: Always provide a default culture

## Additional Notes

- This implementation uses standard .NET resource files (`.resx`)
- Resource files compile into satellite assemblies
- The system gracefully falls back to keys if resources are missing
- Works with both Razor Pages and Blazor Server applications
- Can be extended to support more than two languages
- Thread-safe and request-scoped culture handling

For questions or contributions, refer to the reference implementation at JM.BudoAcademy project.
