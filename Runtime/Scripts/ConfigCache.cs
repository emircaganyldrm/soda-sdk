using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Soda.Runtime.Utils;

namespace Soda.Runtime
{
    public class ConfigCache
    {
        public bool IsUsingOverrides => _useOverrides;
        public Dictionary<string, object> ConfigData => GetEffectiveConfigData();
        
        private Dictionary<string, object> _remoteConfigData = new Dictionary<string, object>();
        private Dictionary<string, object> _overrideConfigData = new Dictionary<string, object>();
        private bool _useOverrides = false;
        
        public void SetOverrideData(Dictionary<string, object> overrides, bool enableOverrides)
        {
            _overrideConfigData = overrides ?? new Dictionary<string, object>();
            _useOverrides = enableOverrides;
            
            SodaLogger.Log($"[ConfigCache] Override system {(enableOverrides ? "enabled" : "disabled")} with {_overrideConfigData.Count} entries");
        }
        
        private Dictionary<string, object> GetEffectiveConfigData()
        {
            if (_useOverrides && _overrideConfigData.Count > 0)
            {
                var effectiveData = new Dictionary<string, object>(_remoteConfigData);
                
                foreach (var kvp in _overrideConfigData)
                {
                    effectiveData[kvp.Key] = kvp.Value;
                }
                
                return effectiveData;
            }
            
            return _remoteConfigData;
        }
        
        public void UpdateFromJson(string jsonResponse)
        {
            if (string.IsNullOrEmpty(jsonResponse))
            {
                SodaLogger.LogWarning("[ConfigCache] Empty JSON response");
                return;
            }
            
            try
            {
                var outerResponse = JObject.Parse(jsonResponse);
                
                if (outerResponse["success"] != null && outerResponse["success"].Value<bool>() == false)
                {
                    string error = outerResponse["error"]?.Value<string>() ?? "Unknown error";
                    SodaLogger.LogWarning($"[ConfigCache] Server returned error: {error}");
                    return;
                }
                
                var configField = outerResponse["config"];
                if (configField == null || configField.Type == JTokenType.Null)
                {
                    SodaLogger.LogWarning("[ConfigCache] No config data in response");
                    return;
                }
                
                _remoteConfigData.Clear();
                
                JObject configObject = null;
                
                if (configField.Type == JTokenType.String)
                {
                    string configString = configField.Value<string>();
                    if (!string.IsNullOrEmpty(configString))
                    {
                        configObject = JObject.Parse(configString);
                    }
                }
                else if (configField.Type == JTokenType.Object)
                {
                    configObject = configField as JObject;
                }
                
                if (configObject != null)
                {
                    foreach (var property in configObject.Properties())
                    {
                        _remoteConfigData[property.Name] = ConvertJToken(property.Value);
                    }
                }
                
                string overrideStatus = _useOverrides ? $" (with {_overrideConfigData.Count} overrides)" : "";
                SodaLogger.Log($"[ConfigCache] Updated with {_remoteConfigData.Count} remote config entries{overrideStatus}");
            }
            catch (JsonReaderException e)
            {
                SodaLogger.LogError($"[ConfigCache] JSON parsing error: {e.Message}");
                SodaLogger.LogError($"[ConfigCache] Raw response: {jsonResponse}");
            }
            catch (Exception e)
            {
                SodaLogger.LogError($"[ConfigCache] Failed to parse config JSON: {e.Message}");
                SodaLogger.LogError($"[ConfigCache] Raw response: {jsonResponse}");
            }
        }
        
        private object ConvertJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.Integer:
                    return token.Value<int>();
                case JTokenType.Float:
                    return token.Value<float>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Array:
                    return token.ToObject<object[]>();
                case JTokenType.Object:
                    return token.ToObject<Dictionary<string, object>>();
                case JTokenType.Null:
                    return null;
                default:
                    return token.ToString();
            }
        }
        
        public T GetValue<T>(string key, T defaultValue = default)
        {
            var effectiveData = GetEffectiveConfigData();

            if (string.IsNullOrEmpty(key) || !effectiveData.TryGetValue(key, out var value))
            {
                SodaLogger.LogWarning($"[ConfigCache] No config data found for key {key}; Using default value: {defaultValue}");
                return defaultValue;
            }

            try
            {
                bool isFromOverride = _useOverrides && _overrideConfigData.ContainsKey(key);
                string source = isFromOverride ? "override" : "remote";
                SodaLogger.Log($"[ConfigCache] Getting '{key}' from {source}: {value}");
                
                return ConvertValue<T>(value, defaultValue);
            }
            catch (Exception e)
            {
                SodaLogger.LogWarning($"[ConfigCache] Type conversion failed for key '{key}': {e.Message}");
            }

            return defaultValue;
        }
        
        private T ConvertValue<T>(object value, T defaultValue)
        {
            if (value == null)
                return defaultValue;
            
            var targetType = typeof(T);
            
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }
            
            if (value.GetType() == targetType)
                return (T)value;
            
            if (value is string stringValue)
            {
                if (targetType == typeof(string))
                    return (T)(object)stringValue;
                
                if (targetType == typeof(bool))
                {
                    bool boolResult = stringValue.ToLower() == "true";
                    return (T)(object)boolResult;
                }
                
                if (targetType == typeof(Color))
                {
                    if (ColorUtility.TryParseHtmlString(stringValue, out Color color))
                        return (T)(object)color;
                    return defaultValue;
                }
            }
            
            try
            {
                return (T)Convert.ChangeType(value, targetType);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}