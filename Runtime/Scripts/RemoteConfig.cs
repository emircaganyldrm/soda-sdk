using System;
using System.Collections.Generic;
using Soda.Runtime.Platform;
using Soda.Runtime.Utils;
using UnityEngine;

namespace Soda.Runtime
{
    public class RemoteConfig
    {
        private ConfigCache _cache = new ConfigCache();
        private IConfigPlatform _platform;
        private string _currentBundleId;
        private string _currentServerUrl;
        private bool _isInitialized;
        
        private SodaOverrideConfigSO _overrideConfig;
        
        public event Action<bool> OnConfigLoaded;
        public event Action<string> OnError;
        
        public bool IsInitialized => _isInitialized;
        public bool IsUsingOverrides => _cache.IsUsingOverrides;
        
        public void Initialize(string bundleId, string serverUrl, string configName, Action<bool> callback = null)
        {
            if (string.IsNullOrEmpty(bundleId))
            {
                SodaLogger.LogError("[RemoteConfig] Bundle ID cannot be empty");
                callback?.Invoke(false);
                return;
            }
            
            _currentBundleId = bundleId;
            _currentServerUrl = serverUrl;
            
#if UNITY_EDITOR
            _platform = new EditorPlatform();
#elif UNITY_ANDROID
            _platform = new AndroidPlatform(SystemInfo.deviceUniqueIdentifier, Application.version);
#else
            _platform = new EditorPlatform();
#endif
            
            _isInitialized = true;
            
            LoadOverrideConfig();
            FetchConfig(configName, callback);
            SodaLogger.Log($"[RemoteConfig] Initialized for {bundleId} on {_platform.GetType().Name}");
        }
        
        private void LoadOverrideConfig()
        {
#if UNITY_EDITOR
            SodaSDKSettingsSO settings = Resources.Load<SodaSDKSettingsSO>("SodaSettings");
            if (settings.overrideConfig )
            {
                _overrideConfig = settings.overrideConfig;
            }

            if (_overrideConfig != null)
            {
                ApplyOverrideConfig();
            }
#endif
        }
        
        private void ApplyOverrideConfig()
        {
            if (_overrideConfig != null)
            {
                var overrideData = _overrideConfig.GetOverrideData();
                _cache.SetOverrideData(overrideData, _overrideConfig.enableOverrides);
            }
        }

        public void FetchConfig(string configName, Action<bool> callback = null, bool fetchOverrideConfig = true)
        {
            if (!_isInitialized)
            {
                SodaLogger.LogError("[RemoteConfig] Not initialized. Call Initialize() first.");
                callback?.Invoke(false);
                return;
            }
            
            if (_overrideConfig != null && _overrideConfig.enableOverrides && fetchOverrideConfig)
            {
                SodaLogger.Log("[RemoteConfig] Using override configuration, skipping server fetch");
                ApplyOverrideConfig();
                OnConfigLoaded?.Invoke(true);
                callback?.Invoke(true);
                return;
            }
            
            SodaLogger.Log("[RemoteConfig] Fetching configuration...");
            
            _platform.FetchConfig(_currentBundleId, _currentServerUrl, configName, (success, jsonResponse) =>
            {
                if (success)
                {
                    try
                    {
                        _cache.UpdateFromJson(jsonResponse);
                        ApplyOverrideConfig();
                        SodaLogger.Log("[RemoteConfig] Configuration updated successfully");
                        OnConfigLoaded?.Invoke(true);
                        callback?.Invoke(true);
                    }
                    catch (Exception e)
                    {
                        SodaLogger.LogError($"[RemoteConfig] Failed to parse config: {e.Message}");
                        OnError?.Invoke($"Failed to parse config: {e.Message}");
                        callback?.Invoke(false);
                    }
                }
                else
                {
                    SodaLogger.LogWarning("[RemoteConfig] Failed to fetch config, using cached/default values");
                    ApplyOverrideConfig();
                    OnError?.Invoke("Failed to fetch remote config");
                    callback?.Invoke(false);
                }
            });
        }

        private T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (!_isInitialized)
            {
                SodaLogger.LogWarning("[RemoteConfig] Not initialized, returning default value");
                return defaultValue;
            }
            
            if (string.IsNullOrEmpty(key))
            {
                SodaLogger.LogWarning("[RemoteConfig] Key cannot be empty");
                return defaultValue;
            }
            
            return _cache.GetValue(key, defaultValue);
        }
        
        public string GetString(string key, string defaultValue = "")
        {
            return GetValue(key, defaultValue);
        }
        
        public int GetInt(string key, int defaultValue = 0)
        {
            return GetValue(key, defaultValue);
        }
        
        public float GetFloat(string key, float defaultValue = 0f)
        {
            return GetValue(key, defaultValue);
        }
        
        public bool GetBool(string key, bool defaultValue = false)
        {
            return GetValue(key, defaultValue);
        }

        public Color GetColor(string key, Color defaultValue = default)
        {
            return GetValue(key, defaultValue);
        }

        public Dictionary<string, object> GetAllConfigs()
        {
            return _cache.ConfigData;
        }
    }
}