using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.WebUpload.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.WebUpload.Api;

/// <summary>
/// Simple authenticated upload endpoint for the Jellyfin web UI.
/// </summary>
[ApiController]
[Route("WebUpload")]
public sealed class WebUploadController : ControllerBase
{
    private readonly ILogger<WebUploadController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebUploadController"/> class.
    /// </summary>
    public WebUploadController(ILogger<WebUploadController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simple landing page for quick manual verification that the plugin is loaded.
    /// </summary>
    /// <remarks>
    /// This endpoint is intentionally unauthenticated so it can be opened directly in a browser.
    /// It does not expose any sensitive data.
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    public ActionResult Index()
    {
        const string html = """
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <title>Jellyfin Web Upload</title>
              <style>
                body{font-family:system-ui,Segoe UI,Roboto,Arial,sans-serif;max-width:900px;margin:40px auto;padding:0 16px;line-height:1.4}
                code{background:#f3f3f3;padding:2px 6px;border-radius:4px}
              </style>
            </head>
            <body>
              <h1>Web Upload plugin is installed</h1>
              <p>This is a simple verification page served by the Jellyfin Web Upload plugin.</p>
              <p>Use the Jellyfin dashboard to configure and upload files:</p>
              <ul>
                <li><code>Dashboard → Plugins → Web Upload</code></li>
              </ul>
              <p>Health check endpoint: <code>/WebUpload/Ping</code></p>
            </body>
            </html>
            """;

        return Content(html, "text/html");
    }

    /// <summary>
    /// Lightweight health check endpoint.
    /// </summary>
    [HttpGet("Ping")]
    [AllowAnonymous]
    public ActionResult<WebUploadPingResult> Ping()
    {
        var enabled = !string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.UploadBasePath);
        return Ok(new WebUploadPingResult(true, enabled));
    }

    /// <summary>
    /// Uploads a file to the configured upload directory.
    /// </summary>
    /// <param name="file">Multipart form file field named <c>file</c>.</param>
    /// <param name="relativePath">Optional subdirectory path (relative to the configured base path).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result.</returns>
    [HttpPost("Upload")]
    [Authorize]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<ActionResult<WebUploadResult>> Upload(
        [FromForm] IFormFile? file,
        [FromForm] string? relativePath,
        CancellationToken cancellationToken)
    {
        var plugin = Plugin.Instance;
        if (plugin is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new WebUploadResult(false, "Plugin instance not available."));
        }

        var config = plugin.Configuration;

        if (string.IsNullOrWhiteSpace(config.UploadBasePath))
        {
            return BadRequest(new WebUploadResult(false, "Uploads are disabled. Set 'Upload base path' in the plugin settings first."));
        }

        if (file is null || file.Length <= 0)
        {
            return BadRequest(new WebUploadResult(false, "No file provided."));
        }

        var maxBytes = (long)config.MaxUploadSizeMiB * 1024L * 1024L;
        if (maxBytes > 0 && file.Length > maxBytes)
        {
            return BadRequest(new WebUploadResult(false, string.Format(
                CultureInfo.InvariantCulture,
                "File exceeds max size ({0} MiB).",
                config.MaxUploadSizeMiB)));
        }

        var extension = Path.GetExtension(file.FileName ?? string.Empty);
        if (!IsExtensionAllowed(extension, config.AllowedExtensions))
        {
            return BadRequest(new WebUploadResult(false, "File extension not allowed."));
        }

        var basePath = Path.GetFullPath(config.UploadBasePath);
        var safeRelative = (relativePath ?? string.Empty).Replace('\\', '/').Trim('/');
        if (safeRelative.Contains("..", StringComparison.Ordinal))
        {
            return BadRequest(new WebUploadResult(false, "Invalid relative path."));
        }

        var targetDir = Path.GetFullPath(Path.Combine(basePath, safeRelative));
        if (!targetDir.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new WebUploadResult(false, "Invalid target path."));
        }

        Directory.CreateDirectory(targetDir);

        var fileName = Path.GetFileName(file.FileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new WebUploadResult(false, "Invalid file name."));
        }

        var targetPath = Path.Combine(targetDir, fileName);

        if (!config.AllowOverwrite && System.IO.File.Exists(targetPath))
        {
            return Conflict(new WebUploadResult(false, "File already exists."));
        }

        try
        {
            await using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var input = file.OpenReadStream();
            await input.CopyToAsync(fs, cancellationToken);

            _logger.LogInformation("Uploaded '{FileName}' to '{TargetPath}'", fileName, targetPath);
            return Ok(new WebUploadResult(true, "Uploaded.", targetPath));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest, new WebUploadResult(false, "Upload canceled."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed for '{FileName}'", fileName);
            return StatusCode(StatusCodes.Status500InternalServerError, new WebUploadResult(false, "Upload failed."));
        }
    }

    private static bool IsExtensionAllowed(string extension, string allowedExtensionsCsv)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        var allowed = allowedExtensionsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.StartsWith(".", StringComparison.Ordinal) ? s : "." + s)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return allowed.Contains(extension);
    }
}
