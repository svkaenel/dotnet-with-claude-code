# Goal

Implementation of a proxy endpoint and cache for assets from headless CMS Directus

# Why

- Currently images and downloads (assets) in websystem are directly downloaded via browser from Directus (Endpoint /assets)
- This requires that the /assets endpoint is open and public for all users
- Additionally this increases load on the headless CMS Directus
- Most assets do not change often, so it makes sense to cache them as files in a directory accessible for websystem and deliver the cached version

# Tasks

I do want to implement both 

- a proxy endpoint in the websystem for loading assets
- a cache for assets from Directus

The proxy endpoint should

- check first if an asset is already cached in a configurable directory path
- load the asset from Directus if its not cached
- save the loaded asset in cache directory
- and finally deliver it to the users browser

Additional considerations

- Directus supports URLs like this with additional meta information:

https://db.mysystem.de/assets/0e28376d-ae8e-4efc-9b02-2d53006c3216?format=webp&width=300&height=200

- To enable caching, this image should be saved as 0e28376d-ae8e-4efc-9b02-2d53006c3216_300x200.webp
- The websystem /web/my-system should also have a (proxy) /assets endpoint
- Both requests to this /assets endpoint in format
  /assets/0e28376d-ae8e-4efc-9b02-2d53006c3216?format=webp&width=300&height=200
  and also
  /assets/0e28376d-ae8e-4efc-9b02-2d53006c3216_300x200.webp
  should be possible
- If there is no ?format=<format> Parameter in the GET request, look in the cache for a cached asset of any image format, if its not available, load it from Directus and use the MIME type of answer to store the assets with correct file ending in cache directory
- assets can be both images and binary files like PDFs
- Implement also a /clear-asset-cache endpoint, if there are no added asset uid, then whole cache should be cleared, if an asset uid is added like /clear-asset-cache/0e28376d-ae8e-4efc-9b02-2d53006c3216 clear this asset in all variations (formats / sizes)

Important

- Implement necessary models and services in library lib/Evanto.Directus.Client
- Configuration settings go to EvDirectusSettings.cs in lib/Evanto.Directus.Client/Settings
- Endpoints in web/my-system
- Apply CodingRules.md
