namespace Jellyfin.Plugin.WebUpload.Models;

/// <summary>
/// Ping response payload.
/// </summary>
public sealed record WebUploadPingResult(bool Ok, bool UploadsEnabled);

