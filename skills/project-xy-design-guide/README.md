# Project XY Design Guide Skill

An agent skill for applying consistent design patterns to the Demo Companies website.

## Overview

This skill provides comprehensive guidance for creating UI components that follow the Project XY brand guidelines, ensuring consistency across all pages and features of the ASP.NET Razor Pages website.

## What This Skill Does

The Project XY Design Guide skill helps AI agents:
- Apply consistent color schemes (primary #282824, secondary #BC002D)
- Use proper typography (Open Sans body, Noto Sans JP headings)
- Create buttons with correct styling (!rounded-button is mandatory)
- Build responsive card layouts with proper hover effects
- Implement forms with custom controls and validation
- Add hero sections with gradient overlays
- Maintain spacing consistency using Tailwind's scale
- Include proper animations and transitions
- Follow German localization patterns

## When to Use This Skill

Use this skill when:
- Creating new Razor pages or partial views
- Adding any UI components (buttons, cards, forms)
- Styling content sections or layouts
- Implementing responsive designs
- Adding interactive elements with animations
- Ensuring brand consistency
- Working on any visual/design-related task

## Key Features

### Brand Colors
- **Primary:** `#282824` (dark charcoal) for headers, buttons, navigation
- **Secondary:** `#BC002D` (crimson red) for CTAs, highlights, active states

### Typography
- **Body:** Open Sans for all paragraph text
- **Headings:** Noto Sans JP with 2px letter spacing

### Critical Design Rule
**Always use `!rounded-button` for all buttons** - this applies an 8px border radius and is mandatory for brand consistency.

### Component Patterns
- News/Blog cards with hover lift effect
- Product cards with availability badges
- Seminar cards with date highlights
- Forms with custom radio/checkbox styling
- Hero sections with gradient overlays
- Responsive grid layouts (1/2/3/4 columns)

## File Structure

```
project-xy-design-guide/
├── SKILL.md                    # Main skill instructions
├── README.md                   # This file
└── examples/
    ├── button-examples.html    # All button variants
    ├── card-examples.html      # News, product, seminar cards
    └── form-examples.html      # Complete form examples
```

## Quick Start

### Creating a Button

```html
<!-- Primary Button -->
<button class="bg-primary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition whitespace-nowrap">
    Button Text
</button>

<!-- Secondary Button (CTA) -->
<button class="bg-secondary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition whitespace-nowrap">
    Call to Action
</button>

<!-- Outlined Button -->
<button class="border-2 border-primary px-6 py-3 !rounded-button hover:bg-primary hover:text-white transition whitespace-nowrap">
    Secondary Action
</button>
```

### Creating a Card

```html
<div class="news-card bg-white rounded-lg overflow-hidden shadow-md">
    <a href="/news/slug">
        <img src="/assets/image.jpg" alt="Title" class="w-full h-48 object-cover object-top" />
    </a>
    <div class="p-6">
        <div class="flex justify-between items-center mb-3">
            <span class="text-xs text-gray-500">17.01.2025</span>
            <span class="text-xs bg-[#DDD6CC] px-2 py-1 rounded-full">Tag</span>
        </div>
        <h3 class="text-md mb-3 font-medium">
            <a href="/news/slug">Card Title</a>
        </h3>
        <p class="text-gray-600 mb-4">Description text...</p>
        <a href="/news/slug" class="text-primary font-medium flex items-center">
            Weitere Informationen
            <i class="ri-arrow-right-line ml-1"></i>
        </a>
    </div>
</div>
```

### Creating a Form Field

```html
<div class="form-field">
    <label for="name" class="block text-sm font-medium text-gray-700 mb-2">
        Name *
    </label>
    <input type="text" id="name" name="Name" required
        class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent transition text-sm" />
    <div class="error-message">
        Bitte geben Sie Ihren Namen ein.
    </div>
</div>
```

## Interactive Examples

Open the HTML files in the `examples/` directory in a web browser to see:

1. **button-examples.html**
   - Primary, secondary, and outlined buttons
   - Buttons with icons
   - Different sizes (small, medium, large)
   - Icon-only buttons
   - Button groups

2. **card-examples.html**
   - News/blog cards with tags and hover effects
   - Product cards with pricing and availability
   - Seminar cards with date information
   - Complete code examples

3. **form-examples.html**
   - Complete contact form
   - All input types (text, email, tel, select, textarea)
   - Custom radio buttons and checkboxes
   - Error states and validation
   - Helper text patterns

## Common Mistakes to Avoid

### ❌ Don't Do This
```html
<!-- Missing !rounded-button -->
<button class="bg-primary px-6 py-3 rounded-lg">Wrong</button>

<!-- Wrong colors -->
<button class="bg-red-500">Wrong Color</button>

<!-- Missing transition -->
<div class="hover:bg-primary">No Smooth Animation</div>

<!-- Inconsistent spacing -->
<div class="gap-5">Use 4, 6, or 8 instead</div>
```

### ✅ Do This
```html
<!-- Correct button -->
<button class="bg-primary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition whitespace-nowrap">
    Correct
</button>

<!-- Brand colors -->
<button class="bg-secondary text-white px-6 py-3 !rounded-button">Correct</button>

<!-- With transition -->
<div class="hover:bg-primary transition">Smooth Animation</div>

<!-- Standard spacing -->
<div class="gap-6">Consistent Spacing</div>
```

## Responsive Breakpoints

- **Mobile:** < 768px
- **Tablet:** 768px - 1023px (`md:`)
- **Desktop:** ≥ 1024px (`lg:`)

### Responsive Pattern Examples

```html
<!-- Hide on mobile, show on desktop -->
<div class="hidden lg:block">Desktop only</div>

<!-- Grid: 1 column mobile, 2 tablet, 4 desktop -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
    <!-- Items -->
</div>

<!-- Responsive text sizing -->
<h1 class="text-3xl md:text-4xl lg:text-5xl">Responsive Heading</h1>
```

## Integration with Project

This skill integrates with:
- **ASP.NET Core 10 Razor Pages
- **Tailwind CSS 3.4** (loaded via script tag in _Layout.cshtml)
- **Remix Icon** library for icons
- **Custom CSS** in `wwwroot/css/site.css` for animations

### Key Project Files

- Layout: `web/shorin-ryu/Pages/Shared/_Layout.cshtml`
- Global styles: `web/shorin-ryu/wwwroot/css/site.css`
- Partials: `web/shorin-ryu/Pages/Partials/` and `web/shorin-ryu/Pages/Shared/Partials/`

## Validation Checklist

Before completing any UI work, verify:

- [ ] All buttons use `!rounded-button`
- [ ] Colors use primary (#282824) or secondary (#BC002D)
- [ ] Spacing uses standard scale (2, 4, 6, 8, 12)
- [ ] Hover states include `transition` class
- [ ] Responsive classes included (`md:`, `lg:`)
- [ ] Icons from Remix Icon library (`ri-*`)
- [ ] Forms have proper validation markup
- [ ] Cards include hover animations
- [ ] Text uses correct font (automatic via Tailwind)

## Reference Documentation

For complete specifications, see:
- **SKILL.md** - Full skill instructions and patterns
- **docs/DesignGuide.md** - Complete design system documentation
- **examples/** - Interactive HTML examples

## Contributing

When updating this skill:
1. Test all examples in a browser
2. Validate HTML/CSS syntax
3. Ensure examples match actual implementation
4. Update both SKILL.md and examples
5. Keep code examples copy-paste ready

## License

Proprietary - For use with Demo Companies projects only.

## Version

- **Version:** 1.0
- **Last Updated:** January 2026
- **Maintained By:** Project XY Development Team

## Questions?

For questions or clarifications about design patterns:
1. Check the complete design guide: `docs/DesignGuide.md`
2. Review existing partials in the project
3. Refer to `site.css` for animation definitions
4. Consult the validation checklist above
