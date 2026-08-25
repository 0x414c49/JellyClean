using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.JellyClean.Cleanup;
using Jellyfin.Plugin.JellyClean.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.JellyClean.Api;

/// <summary>
/// JellyClean API endpoints.
/// </summary>
[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("JellyClean")]
public class JellyCleanController : ControllerBase
{
    private readonly CleanupService _cleanupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="JellyCleanController"/> class.
    /// </summary>
    public JellyCleanController(CleanupService cleanupService)
    {
        _cleanupService = cleanupService;
    }

    /// <summary>
    /// Runs a dry-run cleanup preview.
    /// </summary>
    [HttpPost("Preview")]
    public async Task<ActionResult<CleanupPreviewResult>> Preview(CancellationToken cancellationToken)
    {
        var plugin = Plugin.Instance;
        if (plugin is null)
        {
            return StatusCode(503);
        }

        return await _cleanupService.PreviewAsync(plugin.Configuration, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs cleanup immediately. Dry-run configuration is still respected.
    /// </summary>
    [HttpPost("CleanNow")]
    public async Task<ActionResult<CleanupMetrics>> CleanNow(CancellationToken cancellationToken)
    {
        var plugin = Plugin.Instance;
        if (plugin is null)
        {
            return StatusCode(503);
        }

        var metrics = await _cleanupService.RunAsync(plugin.Configuration, new Progress<double>(), cancellationToken).ConfigureAwait(false);
        plugin.Configuration.LastRun = metrics;
        plugin.Configuration.Totals.TimestampUtc = metrics.TimestampUtc;
        plugin.Configuration.Totals.MatchedItems += metrics.MatchedItems;
        plugin.Configuration.Totals.DeletedItems += metrics.DeletedItems;
        plugin.Configuration.Totals.SkippedItems += metrics.SkippedItems;
        plugin.Configuration.Totals.FreedBytes += metrics.FreedBytes;
        plugin.SaveConfiguration();

        return metrics;
    }
}
