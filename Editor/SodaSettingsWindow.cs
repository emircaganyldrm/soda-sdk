using UnityEngine;
using UnityEditor;
using Soda.Runtime;
using Soda.Runtime.Utils;
using System.IO;

namespace Soda.EditorTools
{
    public class SodaSettingsWindow : EditorWindow
    {
        private SodaSDKSettingsSO settings;
        private SodaOverrideConfigSO overrideConfig;
        private Vector2 scrollPosition;

        [MenuItem("SodaSDK/Settings")]
        public static void ShowWindow()
        {
            SodaSettingsWindow window = GetWindow<SodaSettingsWindow>("Soda SDK Settings");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (settings == null)
            {
                settings = Resources.Load<SodaSDKSettingsSO>("SodaSettings");
            }

            if (overrideConfig == null)
            {
                overrideConfig = Resources.Load<SodaOverrideConfigSO>("Override Config");
                settings.overrideConfig = overrideConfig;
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🍾 Soda SDK Settings", EditorStyles.largeLabel);
            EditorGUILayout.Space(5);

            DrawStatus();

            EditorGUILayout.Space(10);

            DrawSDKSettings();

            EditorGUILayout.Space(10);

            DrawOverrideConfig();

            EditorGUILayout.Space(10);

            DrawActions();

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatus()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("SDK Status", EditorStyles.boldLabel);

            bool hasSettings = settings != null;
            bool hasOverride = overrideConfig != null;
            bool isInitialized = SodaSDK.IsInitialized;

            EditorGUILayout.LabelField($"Settings: {(hasSettings ? "Found" : "Missing")}");
            EditorGUILayout.LabelField($"Override Config: {(hasOverride ? "Found" : "Missing")}");
            EditorGUILayout.LabelField($"SDK Initialized: {(isInitialized ? "Yes" : "No")}");

            if (isInitialized)
            {
                EditorGUILayout.LabelField($"Bundle ID: {SodaSDK.BundleId}");
                EditorGUILayout.LabelField($"Server URL: {SodaSDK.ServerUrl}");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSDKSettings()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("SDK Configuration", EditorStyles.boldLabel);

            if (settings == null)
            {
                EditorGUILayout.HelpBox("SodaSettings not found. Create one to configure the SDK.", MessageType.Warning);
        
                if (GUILayout.Button("Create SodaSettings", GUILayout.Height(30)))
                {
                    CreateSodaSettings();
                }
            }
            else
            {
                SerializedObject serializedSettings = new SerializedObject(settings);
                serializedSettings.Update();

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(serializedSettings.FindProperty("bundleId"), new GUIContent("Bundle ID"));
                string bundleId = serializedSettings.FindProperty("bundleId").stringValue;

                if (!string.IsNullOrEmpty(bundleId))
                {
                    var validation = BundleIdValidator.Validate(bundleId);
                    if (!validation.IsValid)
                    {
                        EditorGUILayout.HelpBox($"Invalid Bundle ID: {validation.ErrorMessage}", MessageType.Error);
                    }
                }

                EditorGUILayout.PropertyField(serializedSettings.FindProperty("serverUrl"),
                    new GUIContent("Server URL"));

                EditorGUILayout.PropertyField(serializedSettings.FindProperty("defaultConfigName"),
                    new GUIContent("Default Config Name"));
                EditorGUILayout.PropertyField(serializedSettings.FindProperty("enableLog"),
                    new GUIContent("Enable Logging"));
                EditorGUILayout.PropertyField(serializedSettings.FindProperty("overrideConfig"),
                    new GUIContent("Current Override"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedSettings.ApplyModifiedProperties();
                    EditorUtility.SetDirty(settings);
                    overrideConfig =  settings.overrideConfig;
                }

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select Settings Asset"))
                {
                    EditorGUIUtility.PingObject(settings);
                    Selection.activeObject = settings;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
        
        private void CreateSodaSettings()
        {
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
    
            SodaSDKSettingsSO newSettings = CreateInstance<SodaSDKSettingsSO>();
            newSettings.bundleId = PlayerSettings.applicationIdentifier;
            newSettings.serverUrl = "http://localhost:8080";
    
            string assetPath = "Assets/Resources/SodaSettings.asset";
            AssetDatabase.CreateAsset(newSettings, assetPath);
            AssetDatabase.SaveAssets();
    
            settings = newSettings;
    
            EditorGUIUtility.PingObject(newSettings);
            Selection.activeObject = newSettings;
    
            Debug.Log("SodaSettings created at " + assetPath);
        }

        private void DrawOverrideConfig()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Override Configuration", EditorStyles.boldLabel);

            if (overrideConfig == null)
            {
                EditorGUILayout.HelpBox("Override Config not found. Create one for local testing.", MessageType.Info);

                if (GUILayout.Button("Create Override Config", GUILayout.Height(30)))
                {
                    CreateOverrideConfig();
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Override Entries: {overrideConfig.overrides.Count}");
                EditorGUILayout.LabelField($"Overrides Enabled: {(overrideConfig.enableOverrides ? " Yes" : "No")}");

                if (overrideConfig.enableOverrides)
                {
                    EditorGUILayout.HelpBox("🟢 Override System ACTIVE - Using local values for testing",
                        MessageType.Warning);
                }

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Override Editor"))
                {
                    EditorGUIUtility.PingObject(overrideConfig);
                    Selection.activeObject = overrideConfig;
                }

                if (GUILayout.Button(overrideConfig.enableOverrides ? "Disable Overrides" : "Enable Overrides"))
                {
                    overrideConfig.enableOverrides = !overrideConfig.enableOverrides;
                    EditorUtility.SetDirty(overrideConfig);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateOverrideConfig()
        {
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            SodaOverrideConfigSO newOverrideConfig = CreateInstance<SodaOverrideConfigSO>();

            string assetPath = "Assets/Resources/Override Config.asset";
            AssetDatabase.CreateAsset(newOverrideConfig, assetPath);
            AssetDatabase.SaveAssets();

            overrideConfig = newOverrideConfig;
            settings.overrideConfig = overrideConfig;
            EditorUtility.SetDirty(settings);

            EditorGUIUtility.PingObject(newOverrideConfig);
            Selection.activeObject = newOverrideConfig;

            Debug.Log("Override Config created at " + assetPath);
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Config Monitor", GUILayout.Height(30)))
            {
                SodaConfigMonitorWindow.ShowWindow();
            }

            if (GUILayout.Button("Test Connection", GUILayout.Height(30)))
            {
                TestConnection();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Documentation", GUILayout.Height(30)))
            {
                Application.OpenURL("https://github.com/emircagan/soda-sdk");
            }

            EditorGUILayout.EndVertical();
        }

        private void TestConnection()
        {
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Test Connection", "No settings found. Create SodaSettings first.", "OK");
                return;
            }

            if (!SodaSDK.IsInitialized)
            {
                if (Application.isPlaying)
                {
                    SodaSDK.Initialize();
                }
                else
                {
                    EditorUtility.DisplayDialog("Test Connection",
                        "SDK must be initialized first. Enter Play Mode to test.", "OK");
                    return;
                }
            }

            EditorUtility.DisplayDialog("Test Connection",
                $"Testing connection to: {settings.serverUrl}\n\n" +
                "Check Console for results. Open Config Monitor to see live data.",
                "OK");

            SodaSDK.RemoteConfig.FetchConfig("default", success =>
            {
                string message = success ? "Connection successful!" : "Connection failed!";
                SodaLogger.LogEditor($"[Connection Test] {message}");
            });
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}