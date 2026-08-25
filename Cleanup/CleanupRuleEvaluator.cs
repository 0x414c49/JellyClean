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
        IEnumerable<CleanupWatchState> watchStates,
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

        var states = watchStates.ToList();
        if (userMode == WatchedUserMode.Any)
        {
            var anyEligible = states.Any(state => IsEligible(state, cutoffUtc));
            reason = anyEligible ? string.Empty : "no selected user is marked played before cutoff";
            return anyEligible;
        }

        var allEligible = states.Count > 0 && states.All(state => IsEligible(state, cutoffUtc));
        reason = allEligible ? string.Empty : "not all selected users are marked played before cutoff";
        return allEligible;
    }

    private static bool IsEligible(CleanupWatchState state, DateTime cutoffUtc)
    {
        return state.Played && state.LastPlayedDate.HasValue && state.LastPlayedDate.Value.ToUniversalTime() <= cutoffUtc;
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
