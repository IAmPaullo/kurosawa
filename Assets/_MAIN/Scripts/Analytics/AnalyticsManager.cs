using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Managers
{
    /// <summary>
    /// Central access point for logging analytics events to Amplitude.
    /// Ensures data consistecy and type safety for event propertie
    /// </summary>
    public static class AnalyticsManager
    {
        private const string EventLevelStart = "level_start";
        private const string EventLevelComplete = "level_complete";
        private const string EventLevelRestart = "level_restart";

        /// <summary>
        /// Logs when a player enters a specific level
        /// </summary>
        /// <param name="levelIndex">The index of the level</param>
        public static void LogLevelStart(int levelIndex)
        {
            var props = new Dictionary<string, object>
            {
                { "level_index", levelIndex }
            };

            LogEvent(EventLevelStart, props);
        }

        /// <summary>
        /// Logs when a player successfully completes a level.
        /// </summary>
        /// <param name="levelIndex">The index of the completed level.</param>
        /// <param name="secondsTaken">Time in seconds from start to finish.</param>
        /// <param name="totalMoves">Total number of interactions/rotations used.</param>
        public static void LogLevelComplete(int levelIndex, float secondsTaken)
        {
            var props = new Dictionary<string, object>
            {
                { "level_index", levelIndex },
                { "time_elapsed", System.Math.Round(secondsTaken, 2) }, // Round to 2 decimals for prettier data
                //{ "moves_count", totalMoves } TODO: implement move count
            };

            LogEvent(EventLevelComplete, props);
        }

        /// <summary>
        /// Logs when the player changes the visual theme of the game.
        /// Useful to track user preferences and potential accessibility choices (e.g. High Contrast, White Text
        /// </summary>
        /// <param name="themeName">The ID or Name of the selected theme (e.g., "Dark", "Neon", "Minimal").</param>
        public static void LogThemeChange(string themeName)
        {
            var props = new Dictionary<string, object>
            {
                { "selected_theme", themeName },
                { "timestamp", System.DateTime.UtcNow.ToString("O") } // might help sort if player change multiple times quickly
            };

            LogEvent("theme_changed", props);
        }

        /// <summary>
        /// Logs when a player manually restarts a level.
        /// <b>NOT IMPLEMENTED.</b>
        /// </summary>
        public static void LogLevelRestart(int levelIndex)
        {
            var props = new Dictionary<string, object>
            {
                { "level_index", levelIndex }
            };

            LogEvent(EventLevelRestart, props);
        }

        /// <summary>
        /// wrapper to safely call Amplitude instance.
        /// </summary>
        private static void LogEvent(string eventName, Dictionary<string, object> properties = null)
        {
            var amplitude = Amplitude.getInstance();
            if (amplitude == null) return;

            if (properties != null)
            {
                amplitude.logEvent(eventName, properties);
            }
            else
            {
                amplitude.logEvent(eventName);
            }

#if UNITY_EDITOR
            Debug.Log($"Analytics Event: {eventName} | Props: {properties?.Count ?? 0}");
#endif
        }
    }
}