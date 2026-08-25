using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.JellyClean.Configuration;

namespace Jellyfin.Plugin.JellyClean.Cleanup;

/// <summary>
/// Pure cleanup rule evaluation.
/// </summary>
public static class CleanupRuleEvaluator
{
    /// <summary>
    /// Decides whether an item is eligible for deletion.
    /// </summary>
    public static bool ShouldDelete(
        IEnumerable<DateTime?> watchedDates,
        WatchedUserMode userMode,
        DateTime cutoffUtc,
        bool isFavoriteExcluded,
        IEnumerable<string> searchableValues,
        IEnumerable<string> exclusionFragments,
        out string reason)
    {
        if (MatchesFragment(searchableValues, exclusionFragments))
        {
            reason = "matched exclusion fragment";
            return false;
        }

        if (isFavoriteExcluded)
        {
            reason = "item or parent is favorite";
            return false;
        }

        var dates = watchedDates.ToList();
        if (userMode == WatchedUserMode.Any)
        {
            var anyEligible = dates.Any(date => date.HasValue && date.Value.ToUniversalTime() <= cutoffUtc);
            reason = anyEligible ? string.Empty : "no selected user watched before cutoff";
            return anyEligible;
        }

        var allEligible = dates.Count > 0 && dates.All(date => date.HasValue && date.Value.ToUniversalTime() <= cutoffUtc);
        reason = allEligible ? string.Empty : "not all selected users watched before cutoff";
        return allEligible;
    }

    private static bool MatchesFragment(IEnumerable<string> searchableValues, IEnumerable<string> fragments)
    {
        var haystacks = searchableValues.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        foreach (var fragment in fragments.Where(fragment => !string.IsNullOrWhiteSpace(fragment)))
        {
            if (haystacks.Any(value => value.Contains(fragment.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
