using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.WebUpload.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.WebUpload;

/// <summary>
/// The main plugin.
/// </summary>
public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "Web Upload";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("4e787d24-b22a-4fbb-8565-ad2c26e079b7");

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        // Loads the embedded config page and shows it under Dashboard -> Plugins -> Web Upload.
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.Configuration.configPage.html",
                    GetType().Namespace)
            }
        ];
    }
}

