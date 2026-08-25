using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.JellyClean.Cleanup;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.JellyClean.Tasks;

/// <summary>
/// Scheduled entry point for JellyClean.
/// </summary>
public class JellyCleanScheduledTask : IScheduledTask
{
    private readonly CleanupService _cleanupService;
    private readonly ILogger<JellyCleanScheduledTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JellyCleanScheduledTask"/> class.
    /// </summary>
    public JellyCleanScheduledTask(CleanupService cleanupService, ILogger<JellyCleanScheduledTask> logger)
    {
        _cleanupService = cleanupService;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "JellyClean cleanup";

    /// <inheritdoc />
    public string Key => "JellyCleanCleanup";

    /// <inheritdoc />
    public string Description => "Removes watched movies and episodes according to JellyClean rules.";

    /// <inheritdoc />
    public string Category => "JellyClean";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);

        var plugin = Plugin.Instance;
        if (plugin is null)
        {
            _logger.LogWarning("JellyClean plugin instance is unavailable.");
            return;
        }

        var config = plugin.Configuration;
        var now = DateTime.UtcNow;
        config.LastCheckUtc = now;

        if (!config.Enabled)
        {
            plugin.SaveConfiguration();
            progress.Report(100);
            return;
        }

        if (!CleanupSchedule.IsDue(config.CronExpression, config.LastCleanupUtc, now, _logger))
        {
            plugin.SaveConfiguration();
            progress.Report(100);
            return;
        }

        var metrics = await _cleanupService.RunAsync(config, progress, cancellationToken).ConfigureAwait(false);
        config.LastCleanupUtc = now;
        config.LastRun = metrics;
        config.Totals.TimestampUtc = now;
        config.Totals.MatchedItems += metrics.MatchedItems;
        config.Totals.DeletedItems += metrics.DeletedItems;
        config.Totals.SkippedItems += metrics.SkippedItems;
        config.Totals.FreedBytes += metrics.FreedBytes;
        plugin.SaveConfiguration();

        progress.Report(100);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo
        {
            Type = TaskTriggerInfoType.IntervalTrigger,
            IntervalTicks = TimeSpan.FromMinutes(15).Ticks
        };
    }
}
