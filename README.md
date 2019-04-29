# LazyMemoryCache

Provides support for lazy in-memory caching in C#

## Deployment process

1. In NuGet:
    1. Login.
    1. Go to `[Username (top-left)]` > `API Keys`.
    1. Make sure you have a key which has not expired and which relates to your MyGet user. If not, create a new key then add it to the 'Upstream sources' section of MyGet.
1. In MyGet:
    1. Login.
    1. Go to `Build services`.
    1. Click on `Edit` next to the CI build source.
    1. Reset the build counter and change the version number in the `Version format` field.
    1. Click `Save`.
    1. Click on `Build` next to the CI build source.
    1. Wait for the build to complete.
    1. Go to `Packages`.
    1. Click `Push` next to the package.
    1. Click `Edit`.
    1. Delete the `Prerelease Tag` value.
    1. Click `Push`.
    1. After about 1 minute, the package should appear on the NuGet.org page in `Validating` status.
    1. After about 5 more minutes, the package should update to `Listed` status. You can consume the package directly immediately, and after about 30 more minutes the package should be indexed and therefore visible in search results etc.
1. In GitHub:
    1. Tag the release in the format `v1.0.0`.
    1. Publish the release tag.
