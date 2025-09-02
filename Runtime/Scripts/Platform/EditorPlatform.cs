using System;
using System.Collections;
using Soda.Runtime.Platform;
using Soda.Runtime.Utils;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace Soda.Runtime
{
    /// <summary>
    /// Unity Editor platform implementation for configuration fetching
    /// Uses UnityWebRequest with EditorCoroutines for network requests
    /// </summary>
    public class EditorPlatform : IConfigPlatform
    {
        private const int REQUEST_TIMEOUT = 10; // seconds
        
        /// <summary>
        /// Fetch configuration from remote server using UnityWebRequest
        /// </summary>
        /// <param name="bundleId">Game bundle identifier</param>
        /// <param name="serverUrl">Server URL for API requests</param>
        /// <param name="callback">Callback with success status and JSON response</param>
        public void FetchConfig(string bundleId, string serverUrl, string configName, Action<bool, string> callback)
        {
#if UNITY_EDITOR
            if (!IsNetworkAvailable())
            {
                SodaLogger.LogWarning("[EditorPlatform] Network not available");
                callback?.Invoke(false, null);
                return;
            }
            
            EditorCoroutineUtility.StartCoroutine(FetchConfigCoroutine(bundleId, serverUrl, configName, callback), this);
#endif
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Coroutine for fetching configuration data
        /// </summary>
        /// <param name="bundleId">Game bundle identifier</param>
        /// <param name="serverUrl">Server URL</param>
        /// <param name="callback">Result callback</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator FetchConfigCoroutine(string bundleId, string serverUrl, string configName, Action<bool, string> callback)
        {
            string url = $"{serverUrl}/api/games/{bundleId}/configs/{configName}";
            
            SodaLogger.Log($"[EditorPlatform] Fetching config from: {url}");
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = REQUEST_TIMEOUT;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("User-Agent", $"SodaSDK-Unity/{Application.unityVersion}");
                request.SetRequestHeader("X-Bundle-ID", bundleId);
                request.SetRequestHeader("X-Build-Version", Application.version);
                request.SetRequestHeader("X-Device-ID", GetDeviceId());
                request.SetRequestHeader("X-Platform", "UnityEditor");
                
                yield return request.SendWebRequest();
                
                HandleWebRequestResult(request, callback);
            }
        }
        
        /// <summary>
        /// Handle UnityWebRequest result
        /// </summary>
        /// <param name="request">Completed web request</param>
        /// <param name="callback">Result callback</param>
        private void HandleWebRequestResult(UnityWebRequest request, Action<bool, string> callback)
        {
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    SodaLogger.Log($"[EditorPlatform] Config fetched successfully");
                    callback?.Invoke(true, request.downloadHandler.text);
                    break;
                    
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    SodaLogger.LogError($"[EditorPlatform] Network error: {request.error}");
                    callback?.Invoke(false, null);
                    break;
                    
                case UnityWebRequest.Result.ProtocolError:
                    string errorMessage = $"HTTP {request.responseCode}: {request.error}";
                    
                    if (request.responseCode == 404)
                    {
                        SodaLogger.LogWarning($"[EditorPlatform] Config not found (404)");
                    }
                    else if (request.responseCode == 500)
                    {
                        SodaLogger.LogError($"[EditorPlatform] Server error (500): {request.error}");
                    }
                    else
                    {
                        SodaLogger.LogError($"[EditorPlatform] Protocol error: {errorMessage}");
                    }
                    
                    callback?.Invoke(false, null);
                    break;
                    
                default:
                    SodaLogger.LogError($"[EditorPlatform] Unknown error: {request.error}");
                    callback?.Invoke(false, null);
                    break;
            }
        }
#endif
        
        /// <summary>
        /// Check if network connection is available
        /// </summary>
        /// <returns>True if network is available</returns>
        public bool IsNetworkAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        
        /// <summary>
        /// Get unique device identifier for the editor
        /// </summary>
        /// <returns>Device ID string</returns>
        public string GetDeviceId()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = $"editor_{SystemInfo.deviceName}_{SystemInfo.operatingSystem}".GetHashCode().ToString();
            }
            
            return deviceId;
        }
    }
}