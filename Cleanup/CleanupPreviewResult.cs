using System.Collections.Generic;
using Jellyfin.Plugin.JellyClean.Configuration;

namespace Jellyfin.Plugin.JellyClean.Cleanup;

/// <summary>
/// Detailed cleanup preview result.
/// </summary>
public class CleanupPreviewResult : CleanupMetrics
{
    /// <summary>
    /// Gets or sets a value indicating whether the run deleted nothing.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Gets or sets items that would be deleted.
    /// </summary>
    public List<CleanupPreviewItem> WouldDelete { get; set; } = new();

    /// <summary>
    /// Gets or sets skipped items.
    /// </summary>
    public List<CleanupPreviewItem> Skipped { get; set; } = new();
}

/// <summary>
/// Item-level cleanup preview details.
/// </summary>
public class CleanupPreviewItem
{
    /// <summary>
    /// Gets or sets item id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets item name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets item type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets item path.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets skip reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets item size in bytes.
    /// </summary>
    public long Size { get; set; }
}
