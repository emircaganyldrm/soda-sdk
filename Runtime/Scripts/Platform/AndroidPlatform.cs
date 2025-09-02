using System;
using Soda.Runtime.Platform;
using Soda.Runtime.Utils;
using UnityEngine;

namespace Soda.Runtime.Platform
{
    /// <summary>
    /// Android platform implementation for configuration fetching
    /// Uses native Java SodaConfigFetcher class
    /// </summary>
    public class AndroidPlatform : IConfigPlatform
    {
        private AndroidJavaObject configFetcher;

        private string deviceId;
        private string buildVersion;
        
        public AndroidPlatform(string deviceId, string buildVersion)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            this.deviceId = deviceId;
            this.buildVersion = buildVersion;

            try
            {
                configFetcher = new AndroidJavaObject("com.emircagan.sodasdk.SodaConfigFetcher");
                SodaLogger.Log("[AndroidPlatform] Native fetcher initialized");
            }
            catch (Exception e)
            {
                SodaLogger.LogError($"[AndroidPlatform] Failed to initialize: {e.Message}");
            }
#endif
        }
        
        /// <summary>
        /// Fetch configuration from remote server using native Java
        /// </summary>
        public void FetchConfig(string bundleId, string serverUrl, string configName, Action<bool, string> callback)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (configFetcher != null)
            {
                try
                {
                    SodaLogger.Log($"[AndroidPlatform] Fetching config: {bundleId}/{configName}");
                    
                    string result = configFetcher.Call<string>("SodaGetConfig", bundleId, serverUrl, configName, deviceId, buildVersion);
                    
                    if (!string.IsNullOrEmpty(result) && result != "{}")
                    {
                        SodaLogger.Log("[AndroidPlatform] Config fetched successfully");
                        callback?.Invoke(true, result);
                    }
                    else
                    {
                        SodaLogger.LogWarning("[AndroidPlatform] Empty or failed response");
                        callback?.Invoke(false, null);
                    }
                }
                catch (Exception e)
                {
                    SodaLogger.LogError($"[AndroidPlatform] Native call failed: {e.Message}");
                    callback?.Invoke(false, null);
                }
            }
            else
            {
                SodaLogger.LogError("[AndroidPlatform] Native fetcher not available");
                callback?.Invoke(false, null);
            }
#else
            SodaLogger.LogWarning("[AndroidPlatform] Not on Android, cannot fetch");
            callback?.Invoke(false, null);
#endif
        }
        
        public bool IsNetworkAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        
        public string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier ?? "android_device";
        }
    }
}