using System;

namespace Jellyfin.Plugin.JellyClean.Cleanup;

/// <summary>
/// Per-user playback state used by cleanup rules.
/// </summary>
public readonly record struct CleanupWatchState(bool Played, DateTime? LastPlayedDate);
