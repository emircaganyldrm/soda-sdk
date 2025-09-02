using UnityEngine;

namespace Soda.Runtime
{
    [CreateAssetMenu(fileName = "SodaSettings", menuName = "SodaSDK/Settings Asset", order = 0)]
    public class SodaSDKSettingsSO : ScriptableObject
    {
        [Header("SDK Configuration")]
        public string bundleId = "com.mycompany.mygame";
        public string serverUrl = "http://localhost:8080";
    
        [Header("Remote Config")]
        public string defaultConfigName = "default";
        
        [Header("Debug")]
        public bool enableLog = true;
        
        public SodaOverrideConfigSO overrideConfig;
    }
}