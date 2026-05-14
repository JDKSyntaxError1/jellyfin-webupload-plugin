namespace Jellyfin.Plugin.WebUpload.Models;

/// <summary>
/// Upload response payload.
/// </summary>
public sealed record WebUploadResult(bool Success, string Message, string? Path = null);

