using System;
using Cronos;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JellyClean.Cleanup;

/// <summary>
/// Cron schedule helper for cleanup runs.
/// </summary>
public static class CleanupSchedule
{
    /// <summary>
    /// Returns whether a cleanup run is due.
    /// </summary>
    public static bool IsDue(string cronExpression, DateTime? lastCleanupUtc, DateTime nowUtc, ILogger? logger = null)
    {
        try
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.Standard);
            var last = lastCleanupUtc ?? nowUtc.AddDays(-1);
            var next = expression.GetNextOccurrence(last, TimeZoneInfo.Utc, inclusive: false);
            return next.HasValue && next.Value <= nowUtc;
        }
        catch (CronFormatException ex)
        {
            logger?.LogError(ex, "JellyClean cron expression is invalid: {CronExpression}", cronExpression);
            return false;
        }
    }
}
