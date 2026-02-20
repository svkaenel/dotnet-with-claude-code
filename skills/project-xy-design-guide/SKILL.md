---
name: project-xy-design-guide
description: Apply consistent design patterns for the Shorin-Ryu Seibukan Karate-Union ASP.NET Razor Pages website using Tailwind CSS. Use when creating new pages, components, forms, cards, buttons, hero sections, or any UI elements. Ensures brand consistency with Japanese martial arts aesthetics, proper color usage (primary #282824, secondary #BC002D), typography (Open Sans body, Noto Sans JP headings), and responsive design patterns.
license: Proprietary
compatibility: Requires ASP.NET Core 10, Tailwind CSS 3.4, Remix Icon library
metadata:
  author: Project XY Development Team
  version: "1.0"
  category: web-design
  project: shorin-ryu-website
---

# Project XY Design Guide Skill

This skill helps you create consistent, brand-aligned UI components for the Shorin-Ryu Seibukan Karate-Union Deutschland e.V. website using established design patterns, color schemes, and component structures.

## When to Use This Skill

Use this skill when:
- Creating new Razor pages or partials
- Adding UI components (buttons, cards, forms, navigation)
- Styling content sections or hero banners
- Implementing responsive layouts
- Adding animations or transitions
- Ensuring brand consistency across the website
- The user mentions design, styling, UI, components, or layout work

## Core Design Principles

### 1. Brand Colors

**Primary Colors:**
- **Primary:** `#282824` (Dark Charcoal) - Use for headers, primary buttons, navigation, footer
- **Secondary:** `#BC002D` (Crimson Red) - Use for CTAs, highlights, active states, links

**Application in Tailwind:**
```html
<button class="bg-primary text-white hover:bg-opacity-90">Primary Button</button>
<button class="bg-secondary text-white hover:bg-opacity-90">Secondary Button</button>
<a href="#" class="text-secondary hover:underline">Link Text</a>
```

**Background Colors:**
- Main background: `#EEECE6` (Warm Off-White) - `bg-[#EEECE6]`
- Card background: `#FFFFFF` (White) - `bg-white`
- Accent beige: `#DDD6CC` - Use for tags: `bg-[#DDD6CC]`

### 2. Typography

**Font Families:**
```css
/* Body text */
font-family: 'Open Sans', sans-serif;

/* All headings */
font-family: 'Noto Sans JP', sans-serif;
letter-spacing: 2px;
```

**Heading Hierarchy:**
```html
<!-- H1 - Hero titles only -->
<h1 class="text-5xl font-light mb-6 leading-tight">
    Main Title <span class="text-secondary font-medium">Highlight</span>
</h1>

<!-- H2 - Section headers -->
<h2 class="text-3xl font-medium mb-6">Section Title</h2>

<!-- H3 - Subsection headers -->
<h3 class="text-xl font-medium mb-4">Subsection Title</h3>

<!-- H4 - Card titles -->
<h4 class="text-md font-medium mb-2">Card Title</h4>
```

### 3. Border Radius - CRITICAL

**Always use `!rounded-button` for all buttons:**
```html
<!-- CORRECT -->
<button class="bg-primary px-6 py-3 !rounded-button">Button</button>

<!-- INCORRECT - Will look wrong -->
<button class="bg-primary px-6 py-3 rounded-lg">Button</button>
```

**Other border radius values:**
- Cards: `rounded-lg` (12px)
- Pills/Tags: `rounded-full`
- Inputs: `rounded-lg`

### 4. Spacing System

Use consistent Tailwind spacing:
- Micro: `gap-2`, `p-2`, `m-2` (8px)
- Small: `gap-4`, `p-4`, `m-4` (16px)
- Medium: `gap-6`, `p-6`, `m-6` (24px)
- Large: `gap-8`, `p-8`, `m-8` (32px)
- XL: `gap-12`, `p-12`, `m-12` (48px)

## Component Patterns

### Buttons

**Primary Button:**
```html
<button class="bg-primary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition whitespace-nowrap">
    Button Text
</button>
```

**Secondary Button:**
```html
<button class="bg-secondary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition whitespace-nowrap">
    Call to Action
</button>
```

**Outlined Button:**
```html
<button class="border-2 border-primary px-6 py-3 !rounded-button hover:bg-primary hover:text-white transition whitespace-nowrap">
    Outlined Action
</button>
```

**Button with Icon (using Remix Icon):**
```html
<button class="bg-primary text-white px-4 py-2 !rounded-button hover:bg-opacity-90 transition flex items-center gap-2">
    <i class="ri-send-plane-line"></i>
    <span>Send</span>
</button>
```

### Cards

**News/Blog Card Pattern:**
```html
<div class="news-card bg-white rounded-lg overflow-hidden shadow-md">
    <a href="/news/@Model.Slug">
        <img src="/assets/@Model.Image" alt="@Model.Title" class="w-full h-48 object-cover object-top" />
    </a>
    <div class="p-6">
        <div class="flex justify-between items-center mb-3">
            <span class="text-xs text-gray-500">@Model.CreatedAtFormatted</span>
            <span class="text-xs bg-[#DDD6CC] px-2 py-1 rounded-full">Tag</span>
        </div>
        <h3 class="text-md mb-3 font-medium">
            <a href="/news/@Model.Slug">@Model.Title</a>
        </h3>
        <p class="text-gray-600 mb-4">@Model.Description</p>
        <a href="/news/@Model.Slug" class="text-primary font-medium flex items-center">
            Weitere Informationen
            <i class="ri-arrow-right-line ml-1"></i>
        </a>
    </div>
</div>
```

**Card CSS (automatically applied via site.css):**
```css
.news-card {
    transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.news-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 12px 20px rgba(0, 0, 0, 0.1);
}
```

### Forms

**Form Structure:**
```html
<form method="post" class="space-y-6">
    @Html.AntiForgeryToken()

    <!-- Text Input -->
    <div class="form-field">
        <label for="name" class="block text-sm font-medium text-gray-700 mb-2">
            Name *
        </label>
        <input type="text" id="name" name="Name" required
            class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent transition text-sm" />
        <div class="error-message">
            Please enter your name.
        </div>
    </div>

    <!-- Radio Button Group -->
    <div class="form-field">
        <label class="block text-sm font-medium text-gray-700 mb-3">Choose Option *</label>
        <div class="flex space-x-6">
            <label class="flex items-center cursor-pointer">
                <input type="radio" name="option" value="1" class="custom-radio" required />
                <span class="ml-3">Option 1</span>
            </label>
            <label class="flex items-center cursor-pointer">
                <input type="radio" name="option" value="2" class="custom-radio" required />
                <span class="ml-3">Option 2</span>
            </label>
        </div>
    </div>

    <!-- Checkbox Group -->
    <div class="form-field">
        <label class="block text-sm font-medium text-gray-700 mb-3">Interests</label>
        <div class="space-y-3">
            <label class="flex items-center cursor-pointer">
                <input type="checkbox" name="interests" value="karate" class="custom-checkbox" />
                <span class="ml-3">Karate</span>
            </label>
            <label class="flex items-center cursor-pointer">
                <input type="checkbox" name="interests" value="kobudo" class="custom-checkbox" />
                <span class="ml-3">Kobudo</span>
            </label>
        </div>
    </div>

    <!-- Textarea -->
    <div class="form-field">
        <label for="message" class="block text-sm font-medium text-gray-700 mb-2">
            Message *
        </label>
        <textarea id="message" name="Message" rows="6" required
            class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent transition resize-vertical text-sm"
            placeholder="Enter your message..."></textarea>
        <div class="error-message">
            Please enter your message.
        </div>
    </div>

    <!-- Submit Button -->
    <div class="pt-4">
        <button type="submit"
            class="w-full bg-primary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition duration-200 font-medium whitespace-nowrap">
            <span class="flex items-center justify-center">
                <i class="ri-send-plane-line mr-2"></i>
                Submit Form
            </span>
        </button>
    </div>
</form>
```

### Hero Sections

**Standard Hero with Gradient Overlay:**
```html
<section class="hero-section h-[600px] flex items-center relative"
    style="background-image: url('/assets/hero-image-id');">
    <div class="container mx-auto px-4">
        <div class="hero-content max-w-2xl">
            <h1 class="text-5xl font-light mb-6 leading-tight">
                Main Title <span class="text-secondary font-medium">Highlighted</span>
            </h1>
            <p class="text-lg mb-8 max-w-xl">
                Hero description that explains the page purpose.
            </p>
            <div class="flex space-x-4">
                <a href="/link1" class="bg-primary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition whitespace-nowrap">
                    Primary Action
                </a>
                <a href="/link2" class="border-2 border-primary px-6 py-3 !rounded-button hover:bg-primary hover:text-white transition whitespace-nowrap">
                    Secondary Action
                </a>
            </div>
        </div>
    </div>
</section>
```

**Dark Hero Variant (for shop pages):**
```html
<section class="hero-section dark h-[400px] flex items-center relative"
    style="background-image: url('/assets/dark-hero.jpg');">
    <!-- Same content structure -->
</section>
```

### Responsive Grid Layouts

**4-Column Grid (standard for card displays):**
```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
    <!-- Card items -->
</div>
```

**3-Column Grid:**
```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <!-- Items -->
</div>
```

**2-Column Grid:**
```html
<div class="grid grid-cols-1 md:grid-cols-2 gap-8">
    <!-- Items -->
</div>
```

## Responsive Design Patterns

**Breakpoints (Tailwind):**
- `sm:` 640px
- `md:` 768px (tablet)
- `lg:` 1024px (desktop)
- `xl:` 1280px
- `2xl:` 1536px

**Common Patterns:**
```html
<!-- Hide on mobile, show on desktop -->
<div class="hidden lg:block">Desktop content</div>

<!-- Show on mobile, hide on desktop -->
<div class="block lg:hidden">Mobile content</div>

<!-- Responsive text sizing -->
<h1 class="text-3xl md:text-4xl lg:text-5xl">Responsive Heading</h1>

<!-- Vertical on mobile, horizontal on desktop -->
<div class="flex flex-col md:flex-row gap-4">
    <!-- Items -->
</div>
```

## Animations & Transitions

**Standard Transition:**
```html
<div class="transition">Element</div>
```

**Custom Duration:**
```html
<div class="transition duration-300">Slower transition</div>
```

**Hover Effects:**
```html
<!-- Lift card -->
<div class="transform hover:-translate-y-1 transition">Card</div>

<!-- Scale -->
<div class="transform hover:scale-105 transition">Element</div>

<!-- Opacity -->
<button class="hover:bg-opacity-90 transition">Button</button>
```

## Icons (Remix Icon)

**Common Icons:**
- Navigation: `ri-arrow-right-line`, `ri-arrow-left-line`, `ri-arrow-down-s-line`
- Actions: `ri-send-plane-line`, `ri-add-line`, `ri-close-line`
- Social: `ri-facebook-fill`, `ri-instagram-line`, `ri-youtube-line`
- Contact: `ri-mail-line`, `ri-phone-line`, `ri-map-pin-line`
- UI: `ri-menu-line`, `ri-search-line`, `ri-shopping-cart-line`

**Usage:**
```html
<i class="ri-arrow-right-line"></i>
```

## Special Components

### Japanese Pattern Separator

```html
<div class="japanese-pattern w-full"></div>
```

Use in footer or section dividers for traditional aesthetic.

### Tags/Pills

```html
<span class="text-xs bg-[#DDD6CC] px-2 py-1 rounded-full">Tag Text</span>
```

### Back to Top Button

```html
<button class="back-to-top">
    <i class="ri-arrow-up-line"></i>
</button>
```

Shows automatically when user scrolls down (JavaScript-controlled).

## Step-by-Step Instructions

### Creating a New Page with Components

1. **Start with Layout Structure:**
   ```html
   @page
   @model YourPageModel
   @{
       ViewData["Title"] = "Page Title";
       ViewData["Description"] = "SEO description";
   }
   ```

2. **Add Hero Section:**
   - Use standard hero template
   - Replace background image URL
   - Update title with `text-secondary` span for highlights
   - Add 2-3 CTA buttons in `flex space-x-4` container

3. **Add Content Sections:**
   - Wrap in `container mx-auto px-4` for consistent margins
   - Use `py-12` or `py-16` for vertical section spacing
   - Add section title with `text-3xl font-medium mb-6`

4. **Add Grid of Cards:**
   - Choose appropriate grid columns (4 for news/products, 3 for general)
   - Use `gap-8` for card spacing
   - Include proper card partial or inline card HTML

5. **Add Forms (if needed):**
   - Use `space-y-6` on form element
   - Include `@Html.AntiForgeryToken()`
   - Use `form-field` wrapper for each field
   - Add proper labels and error messages
   - Submit button at bottom with icon

### Customizing an Existing Component

1. **Locate the partial** in `Pages/Partials/` or `Pages/Shared/Partials/`
2. **Copy the structure** - never modify core patterns
3. **Adjust content** while maintaining:
   - Color scheme (primary/secondary)
   - Spacing (gap-4, gap-6, gap-8)
   - Border radius (!rounded-button for buttons)
   - Transition classes
4. **Test responsive behavior** at mobile, tablet, and desktop breakpoints

### Adding Interactive Elements

1. **Buttons:** Always use `!rounded-button` and appropriate bg color
2. **Hover states:** Include `hover:` variants and `transition`
3. **Icons:** Use Remix Icon classes, typically in `flex items-center` containers
4. **Forms:** Use custom-radio and custom-checkbox classes
5. **Cards:** Include hover transform effects via CSS classes

## Common Mistakes to Avoid

### ❌ Don't Do This

```html
<!-- Missing !rounded-button -->
<button class="bg-primary px-6 py-3 rounded-lg">Wrong</button>

<!-- Using wrong colors -->
<button class="bg-red-500">Wrong Color</button>

<!-- Missing transition -->
<div class="hover:bg-primary">No Transition</div>

<!-- Wrong font on heading -->
<h2 style="font-family: Arial">Wrong Font</h2>

<!-- Inconsistent spacing -->
<div class="gap-5">Inconsistent Gap</div>
```

### ✅ Do This

```html
<!-- Correct button styling -->
<button class="bg-primary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition whitespace-nowrap">
    Correct
</button>

<!-- Using brand colors -->
<button class="bg-secondary text-white px-6 py-3 !rounded-button hover:bg-opacity-90 transition">
    Correct Color
</button>

<!-- Including transitions -->
<div class="hover:bg-primary transition">With Transition</div>

<!-- Using Tailwind classes -->
<h2 class="text-3xl font-medium">Correct Font</h2>

<!-- Standard spacing -->
<div class="gap-6">Standard Gap</div>
```

## Edge Cases and Special Considerations

### Mobile Navigation (< 1024px)

- Desktop nav uses `hidden lg:flex`
- Mobile menu uses `lg:hidden`
- Breakpoint is exactly 1023px

### Image Optimization

- Always use Directus resize parameters: `/assets/IMAGE_ID?width=400&height=400`
- Card images: `h-48` (192px height)
- Object-fit: `object-cover` for images, `object-top` for product/news images

### German Localization

- Button text typically in German: "In den Warenkorb", "Weitere Informationen"
- Form labels in German with asterisk for required: "Name *"
- Date format: DD.MM.YYYY

### Z-Index Layers

- Header: `z-50`
- Sidebar overlays: `z-40` (overlay), `z-50` (content)
- Modals: `z-50`
- Dropdown menus: `z-100`
- Fixed badges: `z-40`

## Reference Files

For detailed specifications and additional examples, refer to:

- **Complete Design Guide:** `docs/DesignGuide.md`
- **Layout Template:** `web/shorin-ryu/Pages/Shared/_Layout.cshtml`
- **Hero Examples:** `web/shorin-ryu/Pages/Partials/HomeTeaserPartial.cshtml`
- **Card Components:** `web/shorin-ryu/Pages/Shared/Partials/NewsCardPartial.cshtml`
- **Form Patterns:** `web/shorin-ryu/Pages/Kontakt/Partials/ContactForm.cshtml`
- **Global Styles:** `web/shorin-ryu/wwwroot/css/site.css`

## Examples

See the `examples/` directory for complete component examples:
- `button-examples.html` - All button variants
- `card-examples.html` - Different card types
- `form-examples.html` - Complete form with all field types
- `hero-examples.html` - Hero section variants
- `layout-examples.html` - Page layout patterns

## Validation Checklist

Before completing a UI task, verify:

- [ ] All buttons use `!rounded-button`
- [ ] Colors use primary (#282824) or secondary (#BC002D)
- [ ] Headings use Noto Sans JP font (automatic via Tailwind classes)
- [ ] Spacing uses standard scale (2, 4, 6, 8, 12)
- [ ] Hover states include `transition` class
- [ ] Responsive classes included (`md:`, `lg:` breakpoints)
- [ ] Icons are from Remix Icon library
- [ ] Forms include proper validation markup
- [ ] Cards include hover animations
- [ ] Images use proper Directus resize parameters

## Getting Help

If uncertain about a design pattern:
1. Search for similar components in existing partials
2. Refer to the complete design guide in `docs/DesignGuide.md`
3. Check site.css for existing CSS classes
4. Follow the validation checklist above

## Summary

This skill ensures all UI components follow the Project XY brand guidelines with:
- **Consistent colors** (primary #282824, secondary #BC002D)
- **Proper typography** (Open Sans body, Noto Sans JP headings)
- **Standard spacing** (4, 6, 8 scale)
- **Correct button styling** (!rounded-button mandatory)
- **Responsive patterns** (mobile-first design)
- **Smooth animations** (transitions on interactions)
- **Brand aesthetics** (Japanese martial arts theme)

Always prioritize consistency over creativity. Use existing patterns and components before creating new ones.
