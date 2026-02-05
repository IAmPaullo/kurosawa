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
        public static void LogLevelComplete(int levelIndex, float secondsTaken, int totalMoves)
        {
            var props = new Dictionary<string, object>
            {
                { "level_index", levelIndex },
                { "time_elapsed", System.Math.Round(secondsTaken, 2) }, // Round to 2 decimals for cleaner data
                { "moves_count", totalMoves }
            };

            LogEvent(EventLevelComplete, props);
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
        /// Internal wrapper to safely call Amplitude instance.
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