using System;
using System.Collections.Generic;
using UnityEngine;

namespace Soda.Runtime
{
    [CreateAssetMenu(fileName = "Override Config", menuName = "SodaSDK/Override Config", order = 1)]
    public class SodaOverrideConfigSO : ScriptableObject
    {
        [Header("Override Settings")]
        [Tooltip("Enable to use local values instead of remote config")]
        public bool enableOverrides = false;
        
        [Header("Override Values")]
        public List<ConfigOverride> overrides = new List<ConfigOverride>();
        
        [Serializable]
        public class ConfigOverride
        {
            public string key;
            public ConfigValueType valueType = ConfigValueType.String;
            
            [SerializeField] private string stringValue;
            [SerializeField] private int intValue;
            [SerializeField] private float floatValue;
            [SerializeField] private bool boolValue;
            [SerializeField] private Color colorValue = Color.white;
            
            public object GetValue()
            {
                return valueType switch
                {
                    ConfigValueType.String => stringValue,
                    ConfigValueType.Int => intValue,
                    ConfigValueType.Float => floatValue,
                    ConfigValueType.Bool => boolValue,
                    ConfigValueType.Color => colorValue,
                    _ => stringValue
                };
            }
            
            public void SetValue(object value)
            {
                switch (valueType)
                {
                    case ConfigValueType.String:
                        stringValue = value?.ToString() ?? "";
                        break;
                    case ConfigValueType.Int:
                        intValue = Convert.ToInt32(value);
                        break;
                    case ConfigValueType.Float:
                        floatValue = Convert.ToSingle(value);
                        break;
                    case ConfigValueType.Bool:
                        boolValue = Convert.ToBoolean(value);
                        break;
                    case ConfigValueType.Color:
                        colorValue = (Color)value;
                        break;
                }
            }
        }
        
        public enum ConfigValueType
        {
            String,
            Int,
            Float,
            Bool,
            Color
        }
        
        public Dictionary<string, object> GetOverrideData()
        {
            var data = new Dictionary<string, object>();
            foreach (ConfigOverride configOverride in overrides)
            {
                if (!string.IsNullOrEmpty(configOverride.key))
                {
                    data[configOverride.key] = configOverride.GetValue();
                }
            }
            return data;
        }
    }
}