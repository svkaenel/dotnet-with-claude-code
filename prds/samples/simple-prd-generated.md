name: "User Avatar Upload Feature - Context-Rich PRD"
description: |
  Comprehensive implementation guide for enabling user avatar upload functionality
  in the profile page with image display, claims update, and persistence.

---

## Goal

Enable authenticated users to upload and display their own avatar image in the profile page (`/Auth/profil`). The avatar should persist across sessions and be displayed throughout the application where user information is shown.

## Why

- **User Identification**: Strengthen users' identification with the system through personalized avatars
- **User Experience**: Modern web applications expect avatar functionality for user profiles
- **Visual Identity**: Provide visual distinction between users in comments, posts, and user lists
- **Engagement**: Personal avatars increase user engagement and platform attachment

## What

### User-Visible Behavior

1. **Profile Page Avatar Display**:
   - Circular avatar container shows default user icon initially
   - Camera button/click on avatar opens inline upload form
   - User can upload JPG or PNG images (max 1MB)
   - After successful upload, avatar image displays in circular container
   - Avatar persists across page refreshes and login sessions

2. **Technical Requirements**:
   - File type validation (JPG, PNG only)
   - File size validation (max 1MB)
   - Server-side upload to Directus CMS
   - User avatar ID stored in authentication claims
   - Avatar accessible via `/assets/{imageId}` URL pattern

### Success Criteria

- [ ] User can click avatar container to open upload form
- [ ] Upload form accepts only JPG/PNG files up to 5MB
- [ ] Successful upload displays image immediately in circular container
- [ ] Avatar persists after page refresh
- [ ] Avatar persists after logout and login
- [ ] Invalid file types/sizes show appropriate error messages
- [ ] Upload failures are handled gracefully with user feedback
- [ ] All code passes build, format, and style checks

## All Needed Context

### Documentation & References

```yaml
# MUST READ - Include these in your context window

- file: /dev/CodingRules.md
  why: C# .NET coding standards that MUST be followed for all code
  critical: |
    - Use 'm' prefix for member variables (e.g., mPagesRepository)
    - Use 'var' for local variables (never combine 'm' with 'var')
    - Use .NET types (String, Int32) instead of C# types (string, int)
    - Format method signatures with aligned parameters
    - Add XML documentation comments above all methods
    - Use guard clauses with ArgumentException.ThrowIfNull pattern
    - Add empty line after closing block statements

- file: /dev/web/my-system/Pages/Auth/profil.cshtml
  lines: 76-82
  why: Existing avatar container structure with ID="avatar-container"
  pattern: |
    <div class="w-32 h-32 rounded-full bg-gray-100 flex items-center justify-center overflow-hidden mx-auto"
        id="avatar-container">
        <i class="ri-user-3-line text-gray-400 text-6xl"></i>
    </div>

- file: /dev/web/my-system/Pages/Auth/profil.cshtml.cs
  why: Profile page model - where avatar upload logic will be added
  pattern: Uses ISrsPagesRepository and IEvDirectusClient dependencies

- file: /dev/web/my-system/Pages/Auth/login.cshtml.cs
  lines: 161-199
  why: SetAuthCookieAsync pattern for claims management
  critical: |
    - Avatar claim already stored at line 178: new Claim("avatar", user.Avatar ?? String.Empty)
    - Shows how to create claims list, identity, and principal
    - Shows how to call HttpContext.SignInAsync to update authentication cookie
    - Pattern MUST be replicated when updating avatar claim

- file: /dev/web/my-system/Pages/Auth/PageManagement.cshtml.cs
  lines: 23, 131-141
  why: File upload pattern using IFormFile
  pattern: |
    [BindProperty]
    public IFormFile? UploadImage { get; set; }

    // In handler method:
    Byte[]? imageData = null;
    if (UploadImage != null && UploadImage.Length > 0)
    {
        using var memoryStream = new MemoryStream();
        await UploadImage.CopyToAsync(memoryStream);
        imageData = memoryStream.ToArray();
    }

- file: /dev/web/my-system/Pages/Auth/PageManagement.cshtml
  lines: 131, 195-199
  why: Form enctype and file input pattern
  pattern: |
    <form method="post" enctype="multipart/form-data" class="space-y-6">
        <input asp-for="UploadImage" type="file" accept="image/*"
            class="w-full px-4 py-3 border border-gray-300 rounded text-sm" />
    </form>

- interface: ISrsPagesRepository.UploadUserAvatarAsync
  signature: |
    Task<EvFileResponse?> UploadUserAvatarAsync(
        String fileName,
        Byte[] imageData,
        String mimeType = "image/jpeg",
        CancellationToken cancellationToken = default);
  returns: EvFileResponse with ID property (file identifier for /assets/{ID} URL)
  location: /dev/lib/SRS.Services/Contracts/ISrsPagesRepository.cs:481-485

- model: EvFileResponse
  location: /dev/evanto/directus/src/Evanto.Directus.Client/Models/Assets/EvFileResponse.cs
  properties: |
    - ID: String (file identifier)
    - FilenameDisk: String
    - FilenameDownload: String
    - Title: String
    - Type: String
    - FileSize: String

- model: EvUserAvatarRequest
  location: /dev/evanto/directus/src/Evanto.Directus.Client/Models/Users/EvUserAvatarRequest.cs
  properties: |
    - Avatar: String? (file ID to associate with user)

- interface: IEvDirectusClient.UpdateUserAsync
  signature: |
    Task<EvUserRequest?> UpdateUserAsync(
        String userId,
        Object updateRequest,
        EvQueryParameters? queryParameters = null,
        CancellationToken cancellationToken = default);
  usage: Pass EvUserAvatarRequest with Avatar property set to file ID
  location: /dev/evanto/directus/src/Evanto.Directus.Client/Contracts/IEvDirectusClient.cs:326-331

- url: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie
  section: Claims updates
  why: Understanding how to refresh authentication cookie with updated claims
  critical: |
    - Must create new ClaimsPrincipal with ALL claims (including updated ones)
    - Call HttpContext.SignInAsync() to issue new authentication cookie
    - Cannot simply update a single claim - must reconstruct entire claims list

- url: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
  section: Upload small files with buffered model binding
  why: ASP.NET Core file upload best practices
  critical: |
    - Use IFormFile for file binding
    - Add enctype="multipart/form-data" to form
    - Validate file size and type server-side
    - Use MemoryStream to convert to byte array
```

### Current Codebase Structure

```bash
web/my-system/
├── Pages/
│   └── Auth/
│       ├── profil.cshtml           # Profile page view (MODIFY)
│       ├── profil.cshtml.cs        # Profile page model (MODIFY)
│       └── login.cshtml.cs         # Contains SetAuthCookieAsync pattern (REFERENCE)
├── Models/                          # No changes needed
├── wwwroot/
│   └── css/
│       └── site.css                 # May need avatar styles (OPTIONAL)
└── Program.cs                       # No changes needed (dependencies already registered)

lib/SRS.Services/
└── Contracts/
    └── ISrsPagesRepository.cs       # UploadUserAvatarAsync already exists (REFERENCE)

evanto/directus/
└── src/Evanto.Directus.Client/
    ├── Contracts/
    │   └── IEvDirectusClient.cs     # UpdateUserAsync already exists (REFERENCE)
    └── Models/
        ├── Assets/
        │   └── EvFileResponse.cs    # Response model (REFERENCE)
        └── Users/
            └── EvUserAvatarRequest.cs # Request model (REFERENCE)
```

### Known Gotchas & Library Quirks

```csharp
# CRITICAL: Claims Update Pattern
// ❌ WRONG - Cannot update a single claim
User.Claims.Add(new Claim("avatar", avatarId));  // This doesn't work!

// ✅ CORRECT - Must reconstruct entire claims list
var existingClaims = User.Claims.ToList();
var oldAvatarClaim = existingClaims.FirstOrDefault(c => c.Type == "avatar");
if (oldAvatarClaim != null) existingClaims.Remove(oldAvatarClaim);
existingClaims.Add(new Claim("avatar", avatarId));
var identity = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
var principal = new ClaimsPrincipal(identity);
await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { ... });

# CRITICAL: Form enctype
// Without enctype="multipart/form-data", file upload will fail silently
<form method="post" enctype="multipart/form-data">  // Required!

# CRITICAL: File validation
// Always validate server-side, never trust client-side validation alone
var allowedTypes = new[] { "image/jpeg", "image/png" };
if (!allowedTypes.Contains(AvatarUpload.ContentType.ToLower()))
    return error;

# CRITICAL: File size limits
// ASP.NET Core has default 30MB request size limit
// For smaller limits, add [RequestSizeLimit] attribute to PageModel
// PageManagement uses: [RequestSizeLimit(250_000_000)] for 250MB
// For avatars, consider [RequestSizeLimit(5_242_880)] for 5MB

# CRITICAL: Avatar URL format
// Directus serves files at /assets/{fileId}
// fileId comes from EvFileResponse.ID property
<img src="/assets/@avatarClaim" />  // Correct pattern

# CRITICAL: Existing dependencies
// ProfileModel already has these injected - DO NOT add duplicates:
// - ISrsPagesRepository mPagesRepository
// - IEvDirectusClient mDirectusClient
// - ILogger<ProfileModel> mLogger

# CRITICAL: Page handler naming
// For named handlers, method name must be: OnPost{HandlerName}Async
// Form must specify: asp-page-handler="{HandlerName}"
public async Task<IActionResult> OnPostUploadAvatarAsync()  // Handler name: "UploadAvatar"
// In view: <form asp-page-handler="UploadAvatar">
```

## Implementation Blueprint

### Data Flow Architecture

```
User clicks avatar → Toggle upload form → User selects file
    ↓
Form submit → OnPostUploadAvatarAsync handler
    ↓
Validate file (type, size)
    ↓
Convert IFormFile to Byte[]
    ↓
mPagesRepository.UploadUserAvatarAsync(fileName, imageData, mimeType)
    ↓
Directus stores file → Returns EvFileResponse with ID
    ↓
mDirectusClient.UpdateUserAsync(userId, new EvUserAvatarRequest { Avatar = fileId })
    ↓
Refresh authentication claims (reconstruct claims list with new avatar)
    ↓
HttpContext.SignInAsync() → Update cookie
    ↓
RedirectToPage() → Avatar displays in circular container
```

### Task List (Sequential Order)

```yaml
Task 1: Add avatar upload property to ProfileModel
  FILE: web/my-system/Pages/Auth/profil.cshtml.cs
  ACTION:
    - Add [BindProperty] IFormFile? AvatarUpload property
    - Add [RequestSizeLimit(5_242_880)] attribute to class (5MB limit)
  LOCATION: After line 18 (after UserProfile property)
  DEPENDENCIES: None

Task 2: Create OnPostUploadAvatarAsync handler
  FILE: web/my-system/Pages/Auth/profil.cshtml.cs
  ACTION:
    - Create new handler method OnPostUploadAvatarAsync
    - Implement: validation → upload → update user → refresh claims
  LOCATION: After OnPostAsync method (around line 111)
  DEPENDENCIES: Task 1
  REFERENCE:
    - PageManagement.cshtml.cs lines 106-198 (form handler pattern)
    - login.cshtml.cs lines 161-199 (SetAuthCookieAsync pattern)

Task 3: Create RefreshAuthCookieWithAvatarAsync helper method
  FILE: web/my-system/Pages/Auth/profil.cshtml.cs
  ACTION:
    - Create private helper method to update authentication cookie
    - Reconstruct claims list with new avatar ID
    - Call HttpContext.SignInAsync
  LOCATION: After OnPostUploadAvatarAsync method
  DEPENDENCIES: Task 2
  REFERENCE: login.cshtml.cs lines 161-199 (complete pattern)

Task 4: Add avatar display logic to profil.cshtml
  FILE: web/my-system/Pages/Auth/profil.cshtml
  ACTION:
    - Replace lines 76-82 (avatar-container section)
    - Add conditional logic to show image if avatar claim exists
    - Show default icon if no avatar claim
  LOCATION: Lines 76-82
  DEPENDENCIES: None
  REFERENCE: Conditional Razor syntax with User.FindFirstValue("avatar")

Task 5: Add avatar upload form to profil.cshtml
  FILE: web/my-system/Pages/Auth/profil.cshtml
  ACTION:
    - Add inline form below avatar-container
    - Include file input, upload/cancel buttons
    - Add JavaScript toggle function
  LOCATION: After avatar-container div (after line 82)
  DEPENDENCIES: Task 1, Task 2
  REFERENCE:
    - PageManagement.cshtml lines 131, 195-199 (form pattern)
    - UeberUns/Partials/UploadForm.cshtml lines 10-11 (file input)

Task 6: Add form toggle JavaScript
  FILE: web/my-system/Pages/Auth/profil.cshtml
  ACTION:
    - Add toggleAvatarUpload() function
    - Show/hide upload form on avatar click
  LOCATION: In <script> section at end of file (after existing script around line 167)
  DEPENDENCIES: Task 5
```

### Per-Task Pseudocode

#### Task 1: Add avatar upload property

```csharp
// FILE: web/my-system/Pages/Auth/profil.cshtml.cs
// LOCATION: After line 18

[BindProperty]
public IFormFile? AvatarUpload { get; set; }

// ALSO: Add attribute to class declaration (line 12)
// MODIFY: public class ProfileModel(...)
// TO:
[RequestSizeLimit(5_242_880)]  // 5MB limit for avatar uploads
public class ProfileModel(...)
```

#### Task 2: Create OnPostUploadAvatarAsync handler

```csharp
// FILE: web/my-system/Pages/Auth/profil.cshtml.cs
// LOCATION: After OnPostAsync method (around line 111)

///-------------------------------------------------------------------------------------------------
/// <summary>   Handles POST request to upload user avatar image. </summary>
///
/// <remarks>   SvK, 24.10.2025. </remarks>
///
/// <returns>   ActionResult indicating success or failure. </returns>
///-------------------------------------------------------------------------------------------------
public async Task<IActionResult> OnPostUploadAvatarAsync()
{   // check requirements
	var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

	ArgumentException.ThrowIfNullOrEmpty(userId, nameof(userId));

	// Validate file presence
	if (AvatarUpload == null || AvatarUpload.Length == 0)
	{
		ModelState.AddModelError(String.Empty, "Bitte wählen Sie eine Datei aus.");
		return Page();
	}

	// Validate file type
	var allowedTypes = new[] { "image/jpeg", "image/png" };

	if (!allowedTypes.Contains(AvatarUpload.ContentType.ToLower()))
	{
		ModelState.AddModelError(String.Empty, "Nur JPG und PNG Dateien sind erlaubt.");
		return Page();
	}

	// Validate file size (5MB max, already enforced by RequestSizeLimit but double-check)
	if (AvatarUpload.Length > 5 * 1024 * 1024)
	{
		ModelState.AddModelError(String.Empty, "Die Datei ist zu groß. Maximal 5MB sind erlaubt.");
		return Page();
	}

	try
	{   // Convert file to byte array
		Byte[]? imageData = null;

		if (AvatarUpload != null && AvatarUpload.Length > 0)
		{
			using var memoryStream = new MemoryStream();

			await AvatarUpload.CopyToAsync(memoryStream);

			imageData = memoryStream.ToArray();
		}

		// Upload to Directus
		var uploadResult = await mPagesRepository.UploadUserAvatarAsync(
			AvatarUpload.FileName,
			imageData!,
			AvatarUpload.ContentType
		);

		if (uploadResult == null || String.IsNullOrEmpty(uploadResult.ID))
		{
			ModelState.AddModelError(String.Empty, "Fehler beim Hochladen des Avatars.");
			return Page();
		}

		// Update user with avatar ID in Directus
		var avatarRequest = new EvUserAvatarRequest { Avatar = uploadResult.ID };
		var updateResult  = await mDirectusClient.UpdateUserAsync(userId, avatarRequest);

		if (updateResult == null)
		{
			mLogger.LogWarning("Avatar uploaded but user update failed for userId: {UserId}", userId);
		}

		// Update authentication claims with new avatar ID
		await RefreshAuthCookieWithAvatarAsync(uploadResult.ID);

		TempData["SuccessMessage"] = "Avatar erfolgreich hochgeladen!";

		return RedirectToPage();
	}

	catch (Exception ex)
	{   // Log error and show message
		mLogger.LogError(ex, "Error uploading avatar for user {UserId}", userId);
		ModelState.AddModelError(String.Empty, $"Fehler beim Hochladen: {ex.Message}");
		return Page();
	}
}
```

#### Task 3: Create RefreshAuthCookieWithAvatarAsync helper

```csharp
// FILE: web/my-system/Pages/Auth/profil.cshtml.cs
// LOCATION: After OnPostUploadAvatarAsync method

///-------------------------------------------------------------------------------------------------
/// <summary>   Refreshes the authentication cookie with updated avatar claim. </summary>
///
/// <remarks>   SvK, 24.10.2025. </remarks>
///
/// <param name="avatarId">   The avatar file identifier. </param>
///-------------------------------------------------------------------------------------------------
private async Task RefreshAuthCookieWithAvatarAsync(String avatarId)
{   // check requirements
	ArgumentException.ThrowIfNullOrEmpty(avatarId, nameof(avatarId));

	try
	{   // Get all existing claims
		var existingClaims = User.Claims.ToList();

		// Remove old avatar claim if exists
		var oldAvatarClaim = existingClaims.FirstOrDefault(c => c.Type == "avatar");

		if (oldAvatarClaim != null)
		{
			existingClaims.Remove(oldAvatarClaim);
		}

		// Add new avatar claim
		existingClaims.Add(new Claim("avatar", avatarId));

		// Create new identity and principal
		var identity  = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
		var principal = new ClaimsPrincipal(identity);

		// Determine if this is a persistent cookie (from remember_me claim)
		var rememberMe = existingClaims.Any(c => c.Type == "remember_me" && c.Value.Equals("True", StringComparison.OrdinalIgnoreCase));

		// Sign in with new principal to update cookie
		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			principal,
			new AuthenticationProperties
			{
				IsPersistent = rememberMe,
				ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(1)
			}
		);

		mLogger.LogInformation("Avatar claim updated for user. New avatar ID: {AvatarId}", avatarId);
	}

	catch (Exception ex)
	{
		mLogger.LogError(ex, "Error refreshing auth cookie with avatar ID: {AvatarId}", avatarId);
		throw;
	}
}
```

#### Task 4: Add avatar display logic

```html
<!-- FILE: web/my-system/Pages/Auth/profil.cshtml -->
<!-- LOCATION: Replace lines 76-82 -->

<div class="text-center mb-8">
    <div class="relative inline-block">
        <div class="w-32 h-32 rounded-full bg-gray-100 flex items-center justify-center overflow-hidden mx-auto cursor-pointer hover:opacity-90 transition-opacity"
            id="avatar-container"
            onclick="toggleAvatarUpload()">
            @{
                var avatarClaim = User.FindFirstValue("avatar");
                if (!String.IsNullOrEmpty(avatarClaim))
                {
                    <img src="/assets/@avatarClaim"
                         alt="Avatar"
                         class="w-full h-full object-cover"
                         id="avatar-image" />
                }
                else
                {
                    <i class="ri-user-3-line text-gray-400 text-6xl" id="avatar-icon"></i>
                }
            }
        </div>
        <button type="button"
                onclick="toggleAvatarUpload()"
                title="Avatar ändern"
                class="absolute bottom-0 right-0 bg-primary text-white rounded-full p-2 hover:bg-opacity-90 transition-colors shadow-md">
            <i class="ri-camera-line"></i>
        </button>
    </div>
</div>
```

#### Task 5: Add avatar upload form

```html
<!-- FILE: web/my-system/Pages/Auth/profil.cshtml -->
<!-- LOCATION: After avatar-container div (after line 82, before form starts at line 83) -->

<!-- Avatar upload form (hidden by default) -->
<div id="avatar-upload-form" class="hidden mt-4 max-w-md mx-auto">
    <form method="post" asp-page-handler="UploadAvatar" enctype="multipart/form-data" class="bg-white rounded-lg border border-gray-200 p-4 space-y-3">
        @Html.AntiForgeryToken()

        <div>
            <label asp-for="AvatarUpload" class="block text-sm font-medium text-gray-700 mb-2">
                Avatar hochladen (JPG, PNG, max. 5MB)
            </label>
            <input asp-for="AvatarUpload"
                   type="file"
                   accept="image/jpeg,image/png"
                   class="w-full px-4 py-2 border border-gray-300 rounded text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent" />
            <span asp-validation-for="AvatarUpload" class="text-red-500 text-xs mt-1"></span>
        </div>

        <div class="flex gap-2">
            <button type="submit"
                    class="flex-1 bg-primary text-white px-4 py-2 rounded-button text-sm font-medium hover:bg-opacity-90 transition-colors">
                <i class="ri-upload-line mr-1"></i>
                Hochladen
            </button>
            <button type="button"
                    onclick="toggleAvatarUpload()"
                    class="flex-1 bg-gray-100 text-gray-700 px-4 py-2 rounded-button text-sm font-medium hover:bg-gray-200 transition-colors">
                Abbrechen
            </button>
        </div>
    </form>
</div>
```

#### Task 6: Add form toggle JavaScript

```html
<!-- FILE: web/my-system/Pages/Auth/profil.cshtml -->
<!-- LOCATION: In <script> section at end of file (after existing script around line 167) -->

<script>
    function toggleAvatarUpload() {
        const form = document.getElementById('avatar-upload-form');
        if (form) {
            form.classList.toggle('hidden');
        }
    }
</script>
```

## Validation Loop

### Level 1: Build, Style & Static Analysis

Run these first—fix any errors or style violations before proceeding.

1. **Restore packages**:
```bash
cd /Users/svk/dev/kunden/srs
dotnet restore
```

2. **Build with warnings as errors**:
```bash
cd /Users/svk/dev/kunden/srs
dotnet build --no-restore --warnaserror
```
Expected: Build returns exit 0 with no warnings.

### Level 2: Manual Testing

Since this is a UI feature with authentication, manual testing is required:

**Test Case 1: Upload JPG Avatar**
1. Navigate to `/Auth/profil` (must be logged in)
2. Click on avatar container or camera button
3. Upload form should appear
4. Select a JPG file < 5MB
5. Click "Hochladen"
6. Avatar image should display in circular container
7. Success message should appear

**Test Case 2: Upload PNG Avatar**
1. Follow steps 1-3 from Test Case 1
2. Select a PNG file < 5MB
3. Click "Hochladen"
4. Avatar image should display
5. Success message should appear

**Test Case 3: Avatar Persistence**
1. After uploading avatar (Test Case 1 or 2)
2. Refresh page (F5)
3. Avatar should still display (not revert to icon)
4. Logout from application
5. Login again
6. Navigate to `/Auth/profil`
7. Avatar should still display

**Test Case 4: Invalid File Type**
1. Navigate to `/Auth/profil`
2. Click avatar to open upload form
3. Try to upload a .gif or .pdf file
4. Should show error: "Nur JPG und PNG Dateien sind erlaubt."

**Test Case 5: File Too Large**
1. Navigate to `/Auth/profil`
2. Click avatar to open upload form
3. Try to upload file > 5MB
4. Should show error: "Die Datei ist zu groß. Maximal 5MB sind erlaubt."

**Test Case 6: Form Cancel**
1. Navigate to `/Auth/profil`
2. Click avatar to open upload form
3. Click "Abbrechen" button
4. Form should hide

## Final Validation Checklist

- [ ] All tests pass: `dotnet build --no-restore --warnaserror`
- [ ] Manual Test Case 1 passed (JPG upload)
- [ ] Manual Test Case 2 passed (PNG upload)
- [ ] Manual Test Case 3 passed (persistence)
- [ ] Manual Test Case 4 passed (invalid file type)
- [ ] Manual Test Case 5 passed (file too large)
- [ ] Manual Test Case 6 passed (cancel form)
- [ ] No console errors in browser DevTools
- [ ] Success messages display correctly
- [ ] Error messages are clear and helpful
- [ ] Avatar displays as circular image (not distorted)
- [ ] Claims cookie updated successfully (verify with browser DevTools → Application → Cookies)

---

## Anti-Patterns to Avoid

- ❌ Don't try to update a single claim directly—must reconstruct entire claims list
- ❌ Don't forget `enctype="multipart/form-data"` on form—file upload will fail silently
- ❌ Don't skip server-side validation—never trust client-side validation alone
- ❌ Don't use `string` or `int`—use `String` and `Int32` per coding standards
- ❌ Don't use `var` with member variables—use `var` only for local variables
- ❌ Don't forget XML documentation comments—required above all methods
- ❌ Don't skip guard clauses—use `ArgumentException.ThrowIfNull` pattern
- ❌ Don't forget error logging—use mLogger.LogError for exceptions
- ❌ Don't create new dependencies—use existing injected instances (mPagesRepository, mDirectusClient)
- ❌ Don't forget to call `RedirectToPage()` after successful upload—prevents form resubmission

---

## PRP Quality Score

**Confidence Level: 9/10**

This PRP provides:
- ✅ Complete context with all necessary file references
- ✅ Exact patterns from existing codebase
- ✅ Detailed pseudocode for each task
- ✅ Executable validation gates
- ✅ Comprehensive manual test cases
- ✅ Known gotchas and critical details
- ✅ Sequential task ordering with dependencies
- ✅ Anti-patterns to avoid
- ✅ File upload best practices
- ✅ Claims management pattern

Score reduced by 1 point because:
- Manual testing required (no automated UI tests)
- Avatar image quality/sizing may need iteration

**Recommendation**: This PRP has sufficient context for one-pass implementation success. Follow the task order strictly and refer to referenced files for exact patterns.
