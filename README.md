# Jellyfin Web Upload Plugin (WIP)

This repository contains a small Jellyfin server plugin that adds an authenticated upload endpoint and a dashboard page to upload files from the browser.

## Add as Jellyfin plugin repository

In Jellyfin Dashboard → Plugins → Repositories → Add:

`https://raw.githubusercontent.com/JDKSyntaxError1/jellyfin-webupload-plugin/main/manifest.json`

## Build

1. Install the .NET SDK (the Jellyfin plugin template currently references the .NET SDK 9.0).
2. Make sure the `Jellyfin.Controller` / `Jellyfin.Model` package versions in `Jellyfin.Plugin.WebUpload/Jellyfin.Plugin.WebUpload.csproj` match your Jellyfin Server version (you said `10.11.8`).
3. Build:

```powershell
dotnet build .\Jellyfin.Plugin.WebUpload.sln -c Release
```

## Install (manual)

Copy the built plugin output (DLL + deps) into a subfolder under your Jellyfin plugins directory, for example on Windows:

`%LocalAppData%\jellyfin\plugins\WebUpload\`

Restart Jellyfin.

## Use

1. Open Jellyfin Dashboard → Plugins → Web Upload.
2. Set **Upload base path** (absolute path on the server) and save.
3. Use the upload form on the same page.

## Security notes

- Uploads are disabled until you set an upload base path.
- The endpoint requires authentication (`[Authenticated]`).
- Keep the upload directory on a dedicated volume/path with strict permissions.
