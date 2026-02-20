# Infrastructure Classes

This file contains the complete implementation of all infrastructure classes needed for the co-located localization system.

## File 1: PrefixColocatedViewLocalizer.cs

```csharp
using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;

namespace YourNamespace.Localization;

/// <summary>
/// Custom view localizer that resolves resources from co-located ./Resources/ subfolder.
/// </summary>
public class PrefixColocatedViewLocalizer : IViewLocalizer, IViewContextAware
{
    private readonly IStringLocalizerFactory mLocalizerFactory;
    private readonly String mAssemblyName;
    private IStringLocalizer? mLocalizer;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="localizerFactory">The string localizer factory.</param>
    /// <param name="assemblyName">The assembly name for resource lookup.</param>
    public PrefixColocatedViewLocalizer(IStringLocalizerFactory localizerFactory, String assemblyName)
    {
        mLocalizerFactory = localizerFactory;
        mAssemblyName     = assemblyName;
    }

    /// <summary>
    /// Gets the localized HTML string for the given key.
    /// </summary>
    /// <param name="name">The resource key name.</param>
    /// <returns>Localized HTML string.</returns>
    public LocalizedHtmlString this[String name]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(mLocalizer);
            var localizedString = mLocalizer[name];
            return new LocalizedHtmlString(name, localizedString.Value, localizedString.ResourceNotFound);
        }
    }

    /// <summary>
    /// Gets the localized HTML string for the given key with arguments.
    /// </summary>
    /// <param name="name">The resource key name.</param>
    /// <param name="arguments">Format arguments.</param>
    /// <returns>Localized HTML string with formatted arguments.</returns>
    public LocalizedHtmlString this[String name, params Object[] arguments]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(mLocalizer);
            var localizedString = mLocalizer[name];
            return new LocalizedHtmlString(name, localizedString.Value, localizedString.ResourceNotFound, arguments);
        }
    }

    /// <summary>
    /// Gets the string resource for the given key.
    /// </summary>
    /// <param name="name">The resource key name.</param>
    /// <returns>Localized string.</returns>
    public LocalizedString GetString(String name)
    {
        ArgumentNullException.ThrowIfNull(mLocalizer);
        return mLocalizer[name];
    }

    /// <summary>
    /// Gets the string resource for the given key with arguments.
    /// </summary>
    /// <param name="name">The resource key name.</param>
    /// <param name="arguments">Format arguments.</param>
    /// <returns>Localized string with formatted arguments.</returns>
    public LocalizedString GetString(String name, params Object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(mLocalizer);
        return mLocalizer[name, arguments];
    }

    /// <summary>
    /// Gets all string resources.
    /// </summary>
    /// <param name="includeParentCultures">Whether to include parent culture resources.</param>
    /// <returns>All localized strings.</returns>
    public IEnumerable<LocalizedString> GetAllStrings(Boolean includeParentCultures)
    {
        ArgumentNullException.ThrowIfNull(mLocalizer);
        return mLocalizer.GetAllStrings(includeParentCultures);
    }

    /// <summary>
    /// Contextualizes the localizer based on the current view.
    /// </summary>
    /// <param name="viewContext">The view context.</param>
    public void Contextualize(ViewContext viewContext)
    {
        ArgumentNullException.ThrowIfNull(viewContext);

        var viewPath = viewContext.ExecutingFilePath ?? viewContext.View?.Path;

        if (String.IsNullOrEmpty(viewPath))
        {
            throw new InvalidOperationException("Cannot determine view path for localization.");
        }

        // Transform: Pages/Shared/Partials/Contact/PrefixContactForm.cshtml
        // To:        Pages.Shared.Partials.Contact.Resources.PrefixContactForm
        var directory    = Path.GetDirectoryName(viewPath)?.Replace('/', '.').Replace('\\', '.').TrimStart('.');
        var resourceName = Path.GetFileNameWithoutExtension(viewPath);
        var baseName     = $"{directory}.Resources.{resourceName}";

        mLocalizer = mLocalizerFactory.Create(baseName, mAssemblyName);
    }
}
```

## File 2: PrefixRouteSegmentRequestCultureProvider.cs

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace YourNamespace.Localization;

/// <summary>
/// Request culture provider that extracts culture from route segment (e.g., /de/, /en/).
/// </summary>
public class PrefixRouteSegmentRequestCultureProvider : RequestCultureProvider
{
    private readonly HashSet<String> mSupportedCultures;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="supportedCultures">The supported culture codes (e.g., "de", "en").</param>
    public PrefixRouteSegmentRequestCultureProvider(String[] supportedCultures)
    {
        mSupportedCultures = new HashSet<String>(supportedCultures, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines the culture from the request path.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The provider culture result, or null if not determined.</returns>
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var path     = httpContext.Request.Path.Value;
        var segments = path?.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments is { Length: > 0 })
        {
            var firstSegment = segments[0];

            if (mSupportedCultures.Contains(firstSegment))
            {
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(firstSegment));
            }
        }

        // No culture found in route, return null to let other providers handle it
        return Task.FromResult<ProviderCultureResult?>(null);
    }
}
```

## File 3: PrefixLanguageSwitchHelper.cs

```csharp
using Microsoft.AspNetCore.Http;

namespace YourNamespace.Localization;

/// <summary>
/// Helper for generating language switch URLs.
/// </summary>
public class PrefixLanguageSwitchHelper(IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// Route mappings for localized page slugs (German to English).
    /// </summary>
    private static readonly Dictionary<String, String> mRouteMapDeToEn = new(StringComparer.OrdinalIgnoreCase)
    {
        { "/de/impressum",    "/en/imprint" },
        { "/de/kontakt",      "/en/contact" },
        { "/de/datenschutz",  "/en/privacy" }
    };

    /// <summary>
    /// Route mappings for localized page slugs (English to German).
    /// </summary>
    private static readonly Dictionary<String, String> mRouteMapEnToDe = new(StringComparer.OrdinalIgnoreCase)
    {
        { "/en/imprint",  "/de/impressum" },
        { "/en/contact",  "/de/kontakt" },
        { "/en/privacy",  "/de/datenschutz" }
    };

    /// <summary>
    /// Gets the current culture from the request path.
    /// </summary>
    public String CurrentCulture
    {
        get
        {
            var path     = httpContextAccessor.HttpContext?.Request.Path.Value ?? "/";
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length > 0 && (segments[0] == "de" || segments[0] == "en"))
            {
                return segments[0];
            }

            return "de"; // default culture
        }
    }

    /// <summary>
    /// Gets the URL for the alternate language.
    /// </summary>
    /// <param name="targetCulture">The target culture code ("de" or "en").</param>
    /// <returns>The translated URL for the target language.</returns>
    public String GetLanguageUrl(String targetCulture)
    {
        var path        = httpContextAccessor.HttpContext?.Request.Path.Value ?? "/";
        var queryString = httpContextAccessor.HttpContext?.Request.QueryString.Value ?? "";

        // If on home page or no culture prefix, redirect to target culture home
        if (path == "/" || path == "")
        {
            return $"/{targetCulture}{queryString}";
        }

        // Check for direct route mapping (localized slugs)
        var routeMap = targetCulture == "en" ? mRouteMapDeToEn : mRouteMapEnToDe;

        if (routeMap.TryGetValue(path, out var mappedPath))
        {
            return $"{mappedPath}{queryString}";
        }

        // Generic swap: replace culture segment
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length > 0 && (segments[0] == "de" || segments[0] == "en"))
        {
            segments[0] = targetCulture;

            var url = $"/{String.Join("/", segments)}{queryString}";

            if (targetCulture == "de" && url.Contains("-en"))
            {
                // Handle slug suffix e.g. for blog articles
                url = url.Replace("-en", "-de");
            }
            else if (targetCulture == "en" && url.Contains("-de"))
            {
                // Handle slug suffix e.g. for blog articles
                url = url.Replace("-de", "-en");
            }

            return url;
        }

        // Fallback: prefix with target culture
        return $"/{targetCulture}{path}{queryString}";
    }

    /// <summary>
    /// Gets the URL for the German language.
    /// </summary>
    public String GermanUrl => GetLanguageUrl("de");

    /// <summary>
    /// Gets the URL for the English language.
    /// </summary>
    public String EnglishUrl => GetLanguageUrl("en");

    /// <summary>
    /// Gets whether the current culture is German.
    /// </summary>
    public Boolean IsGerman => CurrentCulture == "de";

    /// <summary>
    /// Gets whether the current culture is English.
    /// </summary>
    public Boolean IsEnglish => CurrentCulture == "en";
}
```

## File 4: PrefixLocalizationExtensions.cs

```csharp
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace YourNamespace.Localization;

/// <summary>
/// Extension methods for registering localization services.
/// </summary>
public static class PrefixLocalizationExtensions
{
    /// <summary>
    /// Adds co-located view localization services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="callingAssembly">The calling assembly (for resource lookup).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddColocatedViewLocalization(this IServiceCollection services, Assembly callingAssembly)
    {
        var assemblyName = callingAssembly.GetName().Name!;

        services.AddLocalization();
        services.AddTransient<IViewLocalizer>(sp =>
        {
            var factory = sp.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizerFactory>();
            return new PrefixColocatedViewLocalizer(factory, assemblyName);
        });

        // Add language switch helper for URL translation
        services.AddScoped<PrefixLanguageSwitchHelper>();

        return services;
    }

    /// <summary>
    /// Configures request localization with route-based culture detection.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="defaultCulture">The default culture (e.g., "de").</param>
    /// <param name="supportedCultures">The supported cultures (e.g., ["de", "en"]).</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseColocatedRequestLocalization(
        this IApplicationBuilder app,
        String defaultCulture,
        String[] supportedCultures)
    {
        app.UseRequestLocalization(options =>
        {
            options.SetDefaultCulture(defaultCulture)
                   .AddSupportedCultures(supportedCultures)
                   .AddSupportedUICultures(supportedCultures);

            // Use route segment as primary culture provider (e.g., /de/, /en/)
            options.RequestCultureProviders.Insert(0, new PrefixRouteSegmentRequestCultureProvider(supportedCultures));
        });

        return app;
    }
}
```

## Installation Instructions

1. Create a `Localization/` folder in your infrastructure/shared library project
2. Create four new files with the names and content shown above
3. Replace `YourNamespace` with your actual project namespace
4. Build the project to ensure no compilation errors
5. The classes are now ready to be used in your application

## Namespace Customization

Make sure to update the namespace in all four files to match your project structure. Common patterns:

- `MyProject.Infrastructure.Localization`
- `MyCompany.Web.Localization`
- `Core.Localization`

## Dependencies

These classes require the following NuGet packages (typically already included in ASP.NET Core projects):

- `Microsoft.AspNetCore.Mvc.Localization`
- `Microsoft.Extensions.Localization`
- `Microsoft.AspNetCore.Localization`
- `Microsoft.AspNetCore.Http`

No additional third-party packages are required.
