# JellyClean

JellyClean is a Jellyfin server plugin that removes watched movies and episodes based on configurable cleanup rules.

Safety first: dry run is enabled by default. Until dry run is disabled in the plugin settings, JellyClean deletes nothing and only records what it would remove.

## Features

- Cron-style cleanup schedule.
- Delete watched movies and episodes after a configured number of days since watching.
- Require any selected user or all selected users to have watched an item.
- Favorite-aware exclusions for movies, episodes, seasons, and full series.
- Optional title/path exclusion fragments.
- Simple metrics for last run and total freed bytes.

## Build

```powershell
dotnet restore
dotnet build -c Release
```

## Install Manually

1. Publish the plugin:

```powershell
dotnet publish -c Release -o artifacts/publish
```

2. Copy the published files into a Jellyfin plugin folder named `JellyClean`.
3. Restart Jellyfin.
4. Configure JellyClean from the plugin settings page.

## Install From Repository

Add this repository URL in Jellyfin plugin repositories:

```text
https://raw.githubusercontent.com/0x414c49/JellyClean/main/manifest.json
```

## Release

Create a tag like `v1.2.3` and push it. The workflow builds `Jellyfin.Plugin.JellyClean_1.2.3.0.zip`, checksum files, and a Jellyfin repository manifest.
