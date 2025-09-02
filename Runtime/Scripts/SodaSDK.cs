using System;
using Soda.Runtime.Utils;
using UnityEngine;

namespace Soda.Runtime
{
    /// <summary>
    /// Main entry point for Soda SDK
    /// Provides static access to all SDK features
    /// </summary>
    public static class SodaSDK
    {
        public static string BundleId { get; private set; } 
        public static string ServerUrl { get; private set; }
        

        public static event Action OnInitialized;
        public static event Action OnInitializationFailed;
        private static RemoteConfig _remoteConfig;
        
        /// <summary>
        /// Access to Remote Configuration features
        /// </summary>
        public static RemoteConfig RemoteConfig
        {
            get
            {
                if (_remoteConfig == null)
                {
                    _remoteConfig = new RemoteConfig();
                }
                return _remoteConfig;
            }
        }
        
        /// <summary>
        /// Initialize the SDK
        /// </summary>
        public static void Initialize()
        {
            SodaSDKSettingsSO settings = Resources.Load<SodaSDKSettingsSO>("SodaSettings");

            if (settings == null)
            {
                SodaLogger.LogError("SodaSettings not found");
                return;
            }

            if (!BundleIdValidator.IsValid(settings.bundleId))
            {
                SodaLogger.LogError(BundleIdValidator.Validate(settings.bundleId).ErrorMessage);
                return;
            }
            
            BundleId = settings.bundleId;
            ServerUrl = settings.serverUrl;
            
            SodaLogger.ToggleLogging(settings.enableLog);
            
            RemoteConfig.Initialize(BundleId, ServerUrl,settings.defaultConfigName, initalized =>
            {
                if (initalized)
                {
                    OnInitialized?.Invoke();
                    return;
                }
                
                OnInitializationFailed?.Invoke();
            });
            
            SodaLogger.Log($"Initialized for bundle: {BundleId}");
        }
        
        /// <summary>
        /// Check if the SDK has been properly initialized
        /// </summary>
        public static bool IsInitialized => _remoteConfig != null && _remoteConfig.IsInitialized;
        
        /// <summary>
        /// Get SDK version information
        /// </summary>
        public static string Version => "0.0.1";
    }
}