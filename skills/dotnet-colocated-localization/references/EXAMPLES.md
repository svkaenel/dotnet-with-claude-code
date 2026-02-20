# Complete Examples

This document provides complete, working examples of implementing co-located localization in various scenarios.

## Example 1: Contact Form with Validation

This example demonstrates a complete contact form with localized labels, placeholders, error messages, and options.

### File Structure

```
Pages/Contact/
├── Index.cshtml
├── Index.cshtml.cs
└── Resources/
    ├── Index.de.resx
    └── Index.en.resx
```

### Index.cshtml

```cshtml
@page "/contact"
@model ContactModel
@{
    ViewData["Title"] = Localizer["PageTitle"];
}

<div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold mb-6">@Localizer["PageTitle"]</h1>

    <form method="post" class="space-y-6 max-w-2xl">
        @Html.AntiForgeryToken()

        <!-- Sender Type -->
        <div class="form-field">
            <label class="block text-sm font-medium mb-3">@Localizer["Label_SenderType"] *</label>
            <div class="flex space-x-6">
                <label class="flex items-center cursor-pointer">
                    <input type="radio" name="SenderType" value="individual" required />
                    <span class="ml-3">@Localizer["Option_Individual"]</span>
                </label>
                <label class="flex items-center cursor-pointer">
                    <input type="radio" name="SenderType" value="organization" required />
                    <span class="ml-3">@Localizer["Option_Organization"]</span>
                </label>
            </div>
            <div class="error-message">
                @Localizer["Error_SenderTypeRequired"]
            </div>
        </div>

        <!-- Name -->
        <div class="form-field">
            <label for="name" class="block text-sm font-medium mb-2">
                @Localizer["Label_Name"] *
            </label>
            <input type="text" id="name" name="Name" required
                   placeholder="@Localizer["Placeholder_Name"]"
                   class="w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500" />
            <div class="error-message">
                @Localizer["Error_NameRequired"]
            </div>
        </div>

        <!-- Email -->
        <div class="form-field">
            <label for="email" class="block text-sm font-medium mb-2">
                @Localizer["Label_Email"] *
            </label>
            <input type="email" id="email" name="Email" required
                   placeholder="@Localizer["Placeholder_Email"]"
                   class="w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500" />
            <div class="error-message">
                @Localizer["Error_EmailRequired"]
            </div>
        </div>

        <!-- Phone -->
        <div class="form-field">
            <label for="phone" class="block text-sm font-medium mb-2">
                @Localizer["Label_Phone"]
            </label>
            <input type="tel" id="phone" name="Phone"
                   placeholder="@Localizer["Placeholder_Phone"]"
                   class="w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500" />
        </div>

        <!-- Message -->
        <div class="form-field">
            <label for="message" class="block text-sm font-medium mb-2">
                @Localizer["Label_Message"] *
            </label>
            <textarea id="message" name="Message" rows="5" required
                      placeholder="@Localizer["Placeholder_Message"]"
                      class="w-full px-4 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500"></textarea>
            <div class="error-message">
                @Localizer["Error_MessageRequired"]
            </div>
        </div>

        <!-- Submit -->
        <button type="submit" class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
            @Localizer["Button_Submit"]
        </button>

        @if (Model.Success)
        {
            <div class="p-4 bg-green-100 text-green-800 rounded-lg">
                @Localizer["Message_Success"]
            </div>
        }
    </form>
</div>
```

### Index.de.resx

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="PageTitle" xml:space="preserve">
    <value>Kontakt</value>
  </data>
  <data name="Label_SenderType" xml:space="preserve">
    <value>Ich bin</value>
  </data>
  <data name="Option_Individual" xml:space="preserve">
    <value>Privatperson</value>
  </data>
  <data name="Option_Organization" xml:space="preserve">
    <value>Organisation</value>
  </data>
  <data name="Label_Name" xml:space="preserve">
    <value>Name</value>
  </data>
  <data name="Placeholder_Name" xml:space="preserve">
    <value>Ihr vollständiger Name</value>
  </data>
  <data name="Label_Email" xml:space="preserve">
    <value>E-Mail</value>
  </data>
  <data name="Placeholder_Email" xml:space="preserve">
    <value>ihre.email@beispiel.de</value>
  </data>
  <data name="Label_Phone" xml:space="preserve">
    <value>Telefon (optional)</value>
  </data>
  <data name="Placeholder_Phone" xml:space="preserve">
    <value>+49 123 456789</value>
  </data>
  <data name="Label_Message" xml:space="preserve">
    <value>Nachricht</value>
  </data>
  <data name="Placeholder_Message" xml:space="preserve">
    <value>Ihre Nachricht an uns...</value>
  </data>
  <data name="Error_SenderTypeRequired" xml:space="preserve">
    <value>Bitte wählen Sie eine Option aus</value>
  </data>
  <data name="Error_NameRequired" xml:space="preserve">
    <value>Name ist erforderlich</value>
  </data>
  <data name="Error_EmailRequired" xml:space="preserve">
    <value>E-Mail ist erforderlich</value>
  </data>
  <data name="Error_MessageRequired" xml:space="preserve">
    <value>Nachricht ist erforderlich</value>
  </data>
  <data name="Button_Submit" xml:space="preserve">
    <value>Nachricht senden</value>
  </data>
  <data name="Message_Success" xml:space="preserve">
    <value>Vielen Dank! Ihre Nachricht wurde erfolgreich gesendet.</value>
  </data>
</root>
```

### Index.en.resx

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="PageTitle" xml:space="preserve">
    <value>Contact</value>
  </data>
  <data name="Label_SenderType" xml:space="preserve">
    <value>I am</value>
  </data>
  <data name="Option_Individual" xml:space="preserve">
    <value>Individual</value>
  </data>
  <data name="Option_Organization" xml:space="preserve">
    <value>Organization</value>
  </data>
  <data name="Label_Name" xml:space="preserve">
    <value>Name</value>
  </data>
  <data name="Placeholder_Name" xml:space="preserve">
    <value>Your full name</value>
  </data>
  <data name="Label_Email" xml:space="preserve">
    <value>Email</value>
  </data>
  <data name="Placeholder_Email" xml:space="preserve">
    <value>your.email@example.com</value>
  </data>
  <data name="Label_Phone" xml:space="preserve">
    <value>Phone (optional)</value>
  </data>
  <data name="Placeholder_Phone" xml:space="preserve">
    <value>+1 234 567890</value>
  </data>
  <data name="Label_Message" xml:space="preserve">
    <value>Message</value>
  </data>
  <data name="Placeholder_Message" xml:space="preserve">
    <value>Your message to us...</value>
  </data>
  <data name="Error_SenderTypeRequired" xml:space="preserve">
    <value>Please select an option</value>
  </data>
  <data name="Error_NameRequired" xml:space="preserve">
    <value>Name is required</value>
  </data>
  <data name="Error_EmailRequired" xml:space="preserve">
    <value>Email is required</value>
  </data>
  <data name="Error_MessageRequired" xml:space="preserve">
    <value>Message is required</value>
  </data>
  <data name="Button_Submit" xml:space="preserve">
    <value>Send Message</value>
  </data>
  <data name="Message_Success" xml:space="preserve">
    <value>Thank you! Your message has been sent successfully.</value>
  </data>
</root>
```

## Example 2: Blog Post Page with Metadata

This example shows localization for dynamic content with metadata.

### File Structure

```
Pages/Blog/
├── [slug].cshtml
├── [slug].cshtml.cs
└── Resources/
    ├── [slug].de.resx
    └── [slug].en.resx
```

### [slug].cshtml

```cshtml
@page "/blog/{slug}"
@model BlogPostModel
@{
    ViewData["Title"] = Model.Post?.Title;
}

<article class="container mx-auto px-4 py-8 max-w-4xl">
    <!-- Metadata -->
    <div class="text-sm text-gray-600 mb-4">
        <span>@Localizer["Label_PublishedOn", Model.Post.PublishedDate.ToString("D")]</span>
        <span class="mx-2">•</span>
        <span>@Localizer["Label_ReadTime", Model.Post.ReadTimeMinutes]</span>
    </div>

    <!-- Title -->
    <h1 class="text-4xl font-bold mb-6">@Model.Post.Title</h1>

    <!-- Author -->
    <div class="flex items-center mb-8 pb-8 border-b">
        <img src="@Model.Post.AuthorImage" alt="@Model.Post.AuthorName"
             class="w-12 h-12 rounded-full mr-4" />
        <div>
            <div class="font-medium">@Model.Post.AuthorName</div>
            <div class="text-sm text-gray-600">@Localizer["Label_Author"]</div>
        </div>
    </div>

    <!-- Content -->
    <div class="prose prose-lg max-w-none">
        @Html.Raw(Model.Post.Content)
    </div>

    <!-- Tags -->
    <div class="mt-12 pt-8 border-t">
        <h3 class="text-lg font-semibold mb-4">@Localizer["Label_Tags"]</h3>
        <div class="flex flex-wrap gap-2">
            @foreach (var tag in Model.Post.Tags)
            {
                <a href="/blog?tag=@tag"
                   class="px-3 py-1 bg-gray-200 text-gray-800 rounded-full text-sm hover:bg-gray-300">
                    @tag
                </a>
            }
        </div>
    </div>

    <!-- Navigation -->
    <div class="mt-12 flex justify-between">
        @if (Model.PreviousPost != null)
        {
            <a href="@Model.PreviousPost.Url" class="text-blue-600 hover:underline">
                ← @Localizer["Link_PreviousPost"]
            </a>
        }
        else
        {
            <span></span>
        }

        @if (Model.NextPost != null)
        {
            <a href="@Model.NextPost.Url" class="text-blue-600 hover:underline">
                @Localizer["Link_NextPost"] →
            </a>
        }
    </div>
</article>
```

### [slug].de.resx

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Label_PublishedOn" xml:space="preserve">
    <value>Veröffentlicht am {0}</value>
    <comment>{0} is the date</comment>
  </data>
  <data name="Label_ReadTime" xml:space="preserve">
    <value>{0} Min. Lesezeit</value>
    <comment>{0} is the number of minutes</comment>
  </data>
  <data name="Label_Author" xml:space="preserve">
    <value>Autor</value>
  </data>
  <data name="Label_Tags" xml:space="preserve">
    <value>Schlagwörter</value>
  </data>
  <data name="Link_PreviousPost" xml:space="preserve">
    <value>Vorheriger Beitrag</value>
  </data>
  <data name="Link_NextPost" xml:space="preserve">
    <value>Nächster Beitrag</value>
  </data>
</root>
```

### [slug].en.resx

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Label_PublishedOn" xml:space="preserve">
    <value>Published on {0}</value>
    <comment>{0} is the date</comment>
  </data>
  <data name="Label_ReadTime" xml:space="preserve">
    <value>{0} min read</value>
    <comment>{0} is the number of minutes</comment>
  </data>
  <data name="Label_Author" xml:space="preserve">
    <value>Author</value>
  </data>
  <data name="Label_Tags" xml:space="preserve">
    <value>Tags</value>
  </data>
  <data name="Link_PreviousPost" xml:space="preserve">
    <value>Previous Post</value>
  </data>
  <data name="Link_NextPost" xml:space="preserve">
    <value>Next Post</value>
  </data>
</root>
```

## Example 3: Navigation Menu Component

This example shows a reusable navigation menu component with localized links.

### File Structure

```
Pages/Shared/Partials/
├── NavigationMenu.cshtml
└── Resources/
    ├── NavigationMenu.de.resx
    └── NavigationMenu.en.resx
```

### NavigationMenu.cshtml

```cshtml
@using YourNamespace.Localization
@inject PrefixLanguageSwitchHelper LanguageHelper

<nav class="bg-gray-900 text-white">
    <div class="container mx-auto px-4">
        <div class="flex items-center justify-between h-16">
            <!-- Logo -->
            <a href="/@LanguageHelper.CurrentCulture" class="text-xl font-bold">
                @Localizer["Label_Logo"]
            </a>

            <!-- Menu Items -->
            <ul class="hidden md:flex space-x-6">
                <li>
                    <a href="/@LanguageHelper.CurrentCulture"
                       class="hover:text-gray-300">
                        @Localizer["Menu_Home"]
                    </a>
                </li>
                <li>
                    <a href="/@LanguageHelper.CurrentCulture/about"
                       class="hover:text-gray-300">
                        @Localizer["Menu_About"]
                    </a>
                </li>
                <li>
                    <a href="/@LanguageHelper.CurrentCulture/services"
                       class="hover:text-gray-300">
                        @Localizer["Menu_Services"]
                    </a>
                </li>
                <li>
                    <a href="/@LanguageHelper.CurrentCulture/blog"
                       class="hover:text-gray-300">
                        @Localizer["Menu_Blog"]
                    </a>
                </li>
                <li>
                    @if (LanguageHelper.IsGerman)
                    {
                        <a href="/de/kontakt" class="hover:text-gray-300">
                            @Localizer["Menu_Contact"]
                        </a>
                    }
                    else
                    {
                        <a href="/en/contact" class="hover:text-gray-300">
                            @Localizer["Menu_Contact"]
                        </a>
                    }
                </li>
            </ul>

            <!-- Language Switcher -->
            <partial name="Partials/PrefixLanguageSwitcher" />
        </div>
    </div>
</nav>
```

### NavigationMenu.de.resx

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Label_Logo" xml:space="preserve">
    <value>Meine Firma</value>
  </data>
  <data name="Menu_Home" xml:space="preserve">
    <value>Startseite</value>
  </data>
  <data name="Menu_About" xml:space="preserve">
    <value>Über uns</value>
  </data>
  <data name="Menu_Services" xml:space="preserve">
    <value>Leistungen</value>
  </data>
  <data name="Menu_Blog" xml:space="preserve">
    <value>Blog</value>
  </data>
  <data name="Menu_Contact" xml:space="preserve">
    <value>Kontakt</value>
  </data>
</root>
```

### NavigationMenu.en.resx

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Label_Logo" xml:space="preserve">
    <value>My Company</value>
  </data>
  <data name="Menu_Home" xml:space="preserve">
    <value>Home</value>
  </data>
  <data name="Menu_About" xml:space="preserve">
    <value>About</value>
  </data>
  <data name="Menu_Services" xml:space="preserve">
    <value>Services</value>
  </data>
  <data name="Menu_Blog" xml:space="preserve">
    <value>Blog</value>
  </data>
  <data name="Menu_Contact" xml:space="preserve">
    <value>Contact</value>
  </data>
</root>
```

## Example 4: Error Page

### File Structure

```
Pages/Shared/
├── Error.cshtml
└── Resources/
    ├── Error.de.resx
    └── Error.en.resx
```

### Error.cshtml

```cshtml
@page
@model ErrorModel
@{
    ViewData["Title"] = Localizer["PageTitle"];
}

<div class="container mx-auto px-4 py-16 text-center">
    <h1 class="text-6xl font-bold text-gray-800 mb-4">
        @(Model.StatusCode ?? 500)
    </h1>

    <h2 class="text-2xl font-semibold text-gray-700 mb-6">
        @Localizer[$"Error_{Model.StatusCode ?? 500}_Title"]
    </h2>

    <p class="text-gray-600 mb-8">
        @Localizer[$"Error_{Model.StatusCode ?? 500}_Message"]
    </p>

    <a href="/@LanguageHelper.CurrentCulture"
       class="inline-block px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
        @Localizer["Button_BackToHome"]
    </a>
</div>
```

### Error.de.resx

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="PageTitle" xml:space="preserve">
    <value>Fehler</value>
  </data>
  <data name="Error_404_Title" xml:space="preserve">
    <value>Seite nicht gefunden</value>
  </data>
  <data name="Error_404_Message" xml:space="preserve">
    <value>Die angeforderte Seite existiert nicht oder wurde verschoben.</value>
  </data>
  <data name="Error_500_Title" xml:space="preserve">
    <value>Serverfehler</value>
  </data>
  <data name="Error_500_Message" xml:space="preserve">
    <value>Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es später erneut.</value>
  </data>
  <data name="Button_BackToHome" xml:space="preserve">
    <value>Zur Startseite</value>
  </data>
</root>
```

### Error.en.resx

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="PageTitle" xml:space="preserve">
    <value>Error</value>
  </data>
  <data name="Error_404_Title" xml:space="preserve">
    <value>Page Not Found</value>
  </data>
  <data name="Error_404_Message" xml:space="preserve">
    <value>The requested page does not exist or has been moved.</value>
  </data>
  <data name="Error_500_Title" xml:space="preserve">
    <value>Server Error</value>
  </data>
  <data name="Error_500_Message" xml:space="preserve">
    <value>An unexpected error occurred. Please try again later.</value>
  </data>
  <data name="Button_BackToHome" xml:space="preserve">
    <value>Back to Home</value>
  </data>
</root>
```

## Tips for These Examples

1. **Contact Form**: Great starting point for user input with validation messages
2. **Blog Post**: Shows how to use format strings with dynamic data
3. **Navigation**: Demonstrates integration with PrefixLanguageSwitchHelper for culture-aware URLs
4. **Error Page**: Uses dynamic resource keys based on status code

All examples follow the naming conventions and file structure described in the main SKILL.md file.
