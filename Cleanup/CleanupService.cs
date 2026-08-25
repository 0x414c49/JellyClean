using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Plugin.JellyClean.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JellyClean.Cleanup;

/// <summary>
/// Applies JellyClean removal rules.
/// </summary>
public class CleanupService
{
    private readonly ILibraryManager _libraryManager;
    private readonly IUserManager _userManager;
    private readonly IUserDataManager _userDataManager;
    private readonly ILogger<CleanupService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanupService"/> class.
    /// </summary>
    public CleanupService(
        ILibraryManager libraryManager,
        IUserManager userManager,
        IUserDataManager userDataManager,
        ILogger<CleanupService> logger)
    {
        _libraryManager = libraryManager;
        _userManager = userManager;
        _userDataManager = userDataManager;
        _logger = logger;
    }

    /// <summary>
    /// Runs cleanup once.
    /// </summary>
    public Task<CleanupMetrics> RunAsync(PluginConfiguration config, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(-Math.Max(0, config.DaysAfterWatched));
        var metrics = new CleanupMetrics { TimestampUtc = now };
        var users = ResolveUsers(config).ToList();

        if (users.Count == 0)
        {
            _logger.LogWarning("JellyClean found no users to evaluate.");
            return Task.FromResult(metrics);
        }

        var query = new InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode },
            Recursive = true
        };

        var items = _libraryManager.GetItemList(query).ToList();
        var total = Math.Max(items.Count, 1);

        for (var index = 0; index < items.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = items[index];
            progress.Report(index * 95d / total);

            if (!ShouldDelete(item, users, config, cutoff, out var reason))
            {
                metrics.SkippedItems++;
                _logger.LogDebug("JellyClean skipped {ItemName}: {Reason}", item.Name, reason);
                continue;
            }

            metrics.MatchedItems++;
            var itemSize = GetItemSize(item);
            metrics.FreedBytes += itemSize;

            if (config.DryRun)
            {
                _logger.LogInformation(
                    "JellyClean dry run would delete {ItemName} ({ItemId}), potential free {Size} bytes.",
                    item.Name,
                    item.Id,
                    itemSize);
                continue;
            }

            _logger.LogInformation("JellyClean deleting {ItemName} ({ItemId}).", item.Name, item.Id);
            _libraryManager.DeleteItem(item, new DeleteOptions { DeleteFileLocation = true }, false);
            metrics.DeletedItems++;
        }

        return Task.FromResult(metrics);
    }

    private IEnumerable<User> ResolveUsers(PluginConfiguration config)
    {
        var configured = config.UserIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var user in _userManager.GetUsers())
        {
            if (configured.Count == 0 || configured.Contains(user.Id.ToString("N", CultureInfo.InvariantCulture)) || configured.Contains(user.Id.ToString()))
            {
                yield return user;
            }
        }
    }

    private bool ShouldDelete(BaseItem item, IReadOnlyCollection<User> users, PluginConfiguration config, DateTime cutoffUtc, out string reason)
    {
        var watchedDates = users
            .Select(user => _userDataManager.GetUserData(user, item)?.LastPlayedDate)
            .ToList();

        return CleanupRuleEvaluator.ShouldDelete(
            watchedDates,
            config.UserMode,
            cutoffUtc,
            config.ExcludeFavorites && IsFavoriteForAnySelectedUser(item, users),
            new[] { item.Name, item.Path },
            config.ExclusionFragments,
            out reason);
    }

    private bool IsFavoriteForAnySelectedUser(BaseItem item, IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            if (_userDataManager.GetUserData(user, item)?.IsFavorite == true)
            {
                return true;
            }

            var parent = item.GetParent();
            while (parent is not null)
            {
                if (_userDataManager.GetUserData(user, parent)?.IsFavorite == true)
                {
                    return true;
                }

                parent = parent.GetParent();
            }
        }

        return false;
    }

    private static long GetItemSize(BaseItem item)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(item.Path) && File.Exists(item.Path))
            {
                return new FileInfo(item.Path).Length;
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return 0;
    }
}
