# Goal

Enable the user to upload and display his own avatar image in profile page.

# Why

Strengthen users identification with the system

# Tasks

- In web/shorin-ryu/Pages/Auth/profil.cshtml there is already an #avatar-container which displays an icon symbolizing the user
- Enable the user to upload an image when he clicks this avatar icon by displaying an inline form for uploading an image (JPG, PNG only)
- Implement the necessary backend logic in page model, The image can be stored via mPagesRepository.UploadUserAvatarAsync()
- If this is successful please display the image instead the icon (still as circle)
- The Image URL is /assets/<imageid> (returned by UploadUserAvatarAsync)
- Store the avatar id also as user claim "avatar" to be available later on
- The method SethAuthCookieAsync() in web/shorin-ryu/Pages/Auth/login.cshtml.cs already stores the avatar image id during login
- display in profil.cshtml always the avatar image instead of the icon if the "avatar" claim is present and valid


