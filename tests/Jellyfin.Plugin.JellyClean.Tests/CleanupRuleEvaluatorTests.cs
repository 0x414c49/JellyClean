using Jellyfin.Plugin.JellyClean.Cleanup;
using Jellyfin.Plugin.JellyClean.Configuration;
using Xunit;

namespace Jellyfin.Plugin.JellyClean.Tests;

public class CleanupRuleEvaluatorTests
{
    private static readonly DateTime CutoffUtc = new(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void AnyMode_AllowsDelete_WhenOneSelectedUserWatchedBeforeCutoff()
    {
        var result = CleanupRuleEvaluator.ShouldDelete(
            new DateTime?[] { null, CutoffUtc.AddDays(-1), CutoffUtc.AddDays(1) },
            WatchedUserMode.Any,
            CutoffUtc,
            false,
            new[] { "Movie A", "D:\\Media\\Movie A.mkv" },
            Array.Empty<string>(),
            out var reason);

        Assert.True(result);
        Assert.Equal(string.Empty, reason);
    }

    [Fact]
    public void AnyMode_BlocksDelete_WhenNoSelectedUserWatchedBeforeCutoff()
    {
        var result = CleanupRuleEvaluator.ShouldDelete(
            new DateTime?[] { null, CutoffUtc.AddSeconds(1) },
            WatchedUserMode.Any,
            CutoffUtc,
            false,
            new[] { "Movie A" },
            Array.Empty<string>(),
            out var reason);

        Assert.False(result);
        Assert.Equal("no selected user watched before cutoff", reason);
    }

    [Fact]
    public void AllMode_AllowsDelete_WhenEverySelectedUserWatchedBeforeCutoff()
    {
        var result = CleanupRuleEvaluator.ShouldDelete(
            new DateTime?[] { CutoffUtc.AddDays(-2), CutoffUtc },
            WatchedUserMode.All,
            CutoffUtc,
            false,
            new[] { "Episode 1" },
            Array.Empty<string>(),
            out var reason);

        Assert.True(result);
        Assert.Equal(string.Empty, reason);
    }

    [Fact]
    public void AllMode_BlocksDelete_WhenOneSelectedUserHasNotWatched()
    {
        var result = CleanupRuleEvaluator.ShouldDelete(
            new DateTime?[] { CutoffUtc.AddDays(-2), null },
            WatchedUserMode.All,
            CutoffUtc,
            false,
            new[] { "Episode 1" },
            Array.Empty<string>(),
            out var reason);

        Assert.False(result);
        Assert.Equal("not all selected users watched before cutoff", reason);
    }

    [Fact]
    public void FavoriteExclusion_BlocksDelete_EvenWhenWatchedRuleMatches()
    {
        var result = CleanupRuleEvaluator.ShouldDelete(
            new DateTime?[] { CutoffUtc.AddDays(-2) },
            WatchedUserMode.Any,
            CutoffUtc,
            true,
            new[] { "Favorite Movie" },
            Array.Empty<string>(),
            out var reason);

        Assert.False(result);
        Assert.Equal("item or parent is favorite", reason);
    }

    [Fact]
    public void FragmentExclusion_BlocksDelete_ForCaseInsensitivePathMatch()
    {
        var result = CleanupRuleEvaluator.ShouldDelete(
            new DateTime?[] { CutoffUtc.AddDays(-2) },
            WatchedUserMode.Any,
            CutoffUtc,
            false,
            new[] { "Episode 1", "D:\\Media\\KeepThis\\Episode 1.mkv" },
            new[] { "keepthis" },
            out var reason);

        Assert.False(result);
        Assert.Equal("matched exclusion fragment", reason);
    }

    [Fact]
    public void DefaultConfiguration_IsDryRun()
    {
        var config = new PluginConfiguration();

        Assert.True(config.DryRun);
    }
}
