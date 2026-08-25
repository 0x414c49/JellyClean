using Jellyfin.Plugin.JellyClean.Cleanup;
using Xunit;

namespace Jellyfin.Plugin.JellyClean.Tests;

public class CleanupScheduleTests
{
    [Fact]
    public void IsDue_ReturnsTrue_WhenCronOccurrencePassedSinceLastCleanup()
    {
        var result = CleanupSchedule.IsDue(
            "0 3 * * *",
            new DateTime(2026, 8, 24, 3, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 25, 3, 1, 0, DateTimeKind.Utc));

        Assert.True(result);
    }

    [Fact]
    public void IsDue_ReturnsFalse_WhenNextCronOccurrenceIsFuture()
    {
        var result = CleanupSchedule.IsDue(
            "0 3 * * *",
            new DateTime(2026, 8, 25, 3, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 25, 3, 1, 0, DateTimeKind.Utc));

        Assert.False(result);
    }

    [Fact]
    public void IsDue_ReturnsFalse_WhenCronExpressionIsInvalid()
    {
        var result = CleanupSchedule.IsDue(
            "not cron",
            new DateTime(2026, 8, 24, 3, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 8, 25, 3, 1, 0, DateTimeKind.Utc));

        Assert.False(result);
    }
}
