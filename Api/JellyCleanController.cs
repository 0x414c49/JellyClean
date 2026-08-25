using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.JellyClean.Cleanup;
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
}
