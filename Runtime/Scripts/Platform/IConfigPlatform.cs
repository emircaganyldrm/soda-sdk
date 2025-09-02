using System;

namespace Soda.Runtime.Platform
{
    /// <summary>
    /// Platform-specific interface for configuration fetching
    /// Different implementations for Editor, Android, iOS, etc.
    /// </summary>
    public interface IConfigPlatform
    {
        /// <summary>
        /// Fetch configuration from remote server
        /// </summary>
        /// <param name="bundleId">Game bundle identifier</param>
        /// <param name="serverUrl">Server URL for API requests</param>
        /// <param name="callback">Callback with success status and JSON response</param>
        void FetchConfig(string bundleId, string serverUrl, string configName, Action<bool, string> callback);
        
        /// <summary>
        /// Check if network connection is available
        /// </summary>
        /// <returns>True if network is available</returns>
        bool IsNetworkAvailable();
        
        /// <summary>
        /// Get unique device identifier
        /// </summary>
        /// <returns>Device ID string</returns>
        string GetDeviceId();
    }
}