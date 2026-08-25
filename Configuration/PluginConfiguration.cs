using System;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.JellyClean.Configuration;

/// <summary>
/// Cleanup rule mode.
/// </summary>
public enum WatchedUserMode
{
    /// <summary>
    /// Remove when any selected user watched the item.
    /// </summary>
    Any = 0,

    /// <summary>
    /// Remove only when all selected users watched the item.
    /// </summary>
    All = 1
}

/// <summary>
/// JellyClean persisted configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether cleanup is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether deletes are simulated only.
    /// </summary>
    public bool DryRun { get; set; } = true;

    /// <summary>
    /// Gets or sets a cron expression for cleanup timing.
    /// </summary>
    public string CronExpression { get; set; } = "0 3 * * *";

    /// <summary>
    /// Gets or sets how many days must pass after watching before removal.
    /// </summary>
    public int DaysAfterWatched { get; set; } = 7;

    /// <summary>
    /// Gets or sets whether any or all selected users must have watched.
    /// </summary>
    public WatchedUserMode UserMode { get; set; } = WatchedUserMode.All;

    /// <summary>
    /// Gets or sets selected Jellyfin user ids. Empty means all users.
    /// </summary>
    public string[] UserIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether favorite items and favorite ancestors are excluded.
    /// </summary>
    public bool ExcludeFavorites { get; set; } = true;

    /// <summary>
    /// Gets or sets title or path fragments that prevent deletion.
    /// </summary>
    public string[] ExclusionFragments { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the last cron-qualified cleanup time.
    /// </summary>
    public DateTime? LastCleanupUtc { get; set; }

    /// <summary>
    /// Gets or sets the last task check time.
    /// </summary>
    public DateTime? LastCheckUtc { get; set; }

    /// <summary>
    /// Gets or sets last run metrics.
    /// </summary>
    public CleanupMetrics LastRun { get; set; } = new();

    /// <summary>
    /// Gets or sets total metrics since configuration reset.
    /// </summary>
    public CleanupMetrics Totals { get; set; } = new();
}

/// <summary>
/// Cleanup metrics stored in plugin configuration.
/// </summary>
public class CleanupMetrics
{
    /// <summary>
    /// Gets or sets when the metrics were updated.
    /// </summary>
    public DateTime? TimestampUtc { get; set; }

    /// <summary>
    /// Gets or sets matching items.
    /// </summary>
    public int MatchedItems { get; set; }

    /// <summary>
    /// Gets or sets deleted items.
    /// </summary>
    public int DeletedItems { get; set; }

    /// <summary>
    /// Gets or sets skipped items.
    /// </summary>
    public int SkippedItems { get; set; }

    /// <summary>
    /// Gets or sets freed or potential freed bytes.
    /// </summary>
    public long FreedBytes { get; set; }
}
