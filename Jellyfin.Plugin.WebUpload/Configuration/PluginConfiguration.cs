using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.WebUpload.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public sealed class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        UploadBasePath = string.Empty;
        AllowedExtensions = ".mp4,.mkv,.mp3,.flac,.jpg,.png";
        MaxUploadSizeMiB = 1024;
        AllowOverwrite = false;
    }

    /// <summary>
    /// Gets or sets the base directory where uploaded files may be written.
    /// </summary>
    /// <remarks>
    /// Leave empty to disable uploads (safer default).
    /// </remarks>
    public string UploadBasePath { get; set; }

    /// <summary>
    /// Gets or sets a comma-separated list of allowed file extensions (e.g. ".mp4,.mkv").
    /// </summary>
    public string AllowedExtensions { get; set; }

    /// <summary>
    /// Gets or sets the maximum upload size in MiB.
    /// </summary>
    public int MaxUploadSizeMiB { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether existing files may be overwritten.
    /// </summary>
    public bool AllowOverwrite { get; set; }
}

