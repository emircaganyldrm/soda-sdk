using System;
using System.Collections.Generic;
using System.Linq;
using Soda.Runtime;
using UnityEngine;
using UnityEditor;

namespace Soda.EditorTools
{
    [CustomEditor(typeof(SodaOverrideConfigSO))]
    public class SodaOverrideConfigSOEditor : Editor
    {
        private SerializedProperty _enableOverridesProp;
        private SerializedProperty _overridesProp;
        private string _configName = "default";
        
        private Vector2 scrollPosition;
        private bool showImportFromRemote = false;
        
        private void OnEnable()
        {
            _enableOverridesProp = serializedObject.FindProperty("enableOverrides");
            _overridesProp = serializedObject.FindProperty("overrides");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            SodaOverrideConfigSO configSO = (SodaOverrideConfigSO)target;
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Override Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Override system allows you to use local values instead of remote config for rapid testing.", MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            EditorGUI.BeginChangeCheck();
            bool wasEnabled = _enableOverridesProp.boolValue;
            EditorGUILayout.PropertyField(_enableOverridesProp, new GUIContent("Enable Overrides", "When enabled, local values will be used instead of remote config"));
            
            if (EditorGUI.EndChangeCheck() && _enableOverridesProp.boolValue != wasEnabled)
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUILayout.Space(5);
            
            if (_enableOverridesProp.boolValue)
            {
                EditorGUILayout.HelpBox("🟢 Override System ACTIVE - Using local values", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("🔴 Override System INACTIVE - Using remote config", MessageType.None);
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add Override", GUILayout.Height(25)))
            {
                AddNewOverride();
            }
            
            if (GUILayout.Button("Clear All", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Clear All Overrides", "Are you sure you want to clear all override values?", "Yes", "Cancel"))
                {
                    _overridesProp.ClearArray();
                }
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Fetch Config", GUILayout.Height(30)))
            {
                FetchConfig();
            }
            
            EditorGUILayout.Space(10);
            
            showImportFromRemote = EditorGUILayout.Foldout(showImportFromRemote, "Import from Remote Config");
            if (showImportFromRemote)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Import current remote config values as override entries for easy editing.", MessageType.Info);
                
                if (GUILayout.Button("Import Remote Config Values"))
                {
                    ImportFromRemoteConfig(configSO);
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField($"Override Entries ({_overridesProp.arraySize})", EditorStyles.boldLabel);
            
            if (_overridesProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No override entries. Click 'Add Override' to create one.", MessageType.Info);
            }
            else
            {
                DrawOverrideList();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawOverrideList()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(300));
            
            for (int i = 0; i < _overridesProp.arraySize; i++)
            {
                DrawOverrideEntry(i);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawOverrideEntry(int index)
        {
            SerializedProperty overrideProp = _overridesProp.GetArrayElementAtIndex(index);
            SerializedProperty keyProp = overrideProp.FindPropertyRelative("key");
            SerializedProperty valueTypeProp = overrideProp.FindPropertyRelative("valueType");
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Override {index + 1}", EditorStyles.boldLabel);
            
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
            {
                _overridesProp.DeleteArrayElementAtIndex(index);
                return;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(keyProp, new GUIContent("Key"));
            
            EditorGUILayout.PropertyField(valueTypeProp, new GUIContent("Type"));
            
            var valueType = (SodaOverrideConfigSO.ConfigValueType)valueTypeProp.enumValueIndex;
            DrawValueField(overrideProp, valueType);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawValueField(SerializedProperty overrideProp, SodaOverrideConfigSO.ConfigValueType valueType)
        {
            switch (valueType)
            {
                case SodaOverrideConfigSO.ConfigValueType.String:
                    EditorGUILayout.PropertyField(overrideProp.FindPropertyRelative("stringValue"), new GUIContent("Value"));
                    break;
                case SodaOverrideConfigSO.ConfigValueType.Int:
                    EditorGUILayout.PropertyField(overrideProp.FindPropertyRelative("intValue"), new GUIContent("Value"));
                    break;
                case SodaOverrideConfigSO.ConfigValueType.Float:
                    EditorGUILayout.PropertyField(overrideProp.FindPropertyRelative("floatValue"), new GUIContent("Value"));
                    break;
                case SodaOverrideConfigSO.ConfigValueType.Bool:
                    EditorGUILayout.PropertyField(overrideProp.FindPropertyRelative("boolValue"), new GUIContent("Value"));
                    break;
                case SodaOverrideConfigSO.ConfigValueType.Color:
                    EditorGUILayout.PropertyField(overrideProp.FindPropertyRelative("colorValue"), new GUIContent("Value"));
                    break;
            }
        }
        
        private void AddNewOverride()
        {
            _overridesProp.InsertArrayElementAtIndex(_overridesProp.arraySize);
            SerializedProperty newOverride = _overridesProp.GetArrayElementAtIndex(_overridesProp.arraySize - 1);
            
            newOverride.FindPropertyRelative("key").stringValue = "newKey";
            newOverride.FindPropertyRelative("valueType").enumValueIndex = 0; // String
            newOverride.FindPropertyRelative("stringValue").stringValue = "";
            newOverride.FindPropertyRelative("intValue").intValue = 0;
            newOverride.FindPropertyRelative("floatValue").floatValue = 0f;
            newOverride.FindPropertyRelative("boolValue").boolValue = false;
            newOverride.FindPropertyRelative("colorValue").colorValue = Color.white;
        }

        private void FetchConfig()
        {
            if (!SodaSDK.IsInitialized)
            {
                SodaSDK.Initialize();
                return;
            }
            
            SodaSDK.RemoteConfig.FetchConfig(_configName, null, false);
        }
        
        private void ImportFromRemoteConfig(SodaOverrideConfigSO configSO)
        {
            // if (!SodaSDK.IsInitialized)
            // {
            //     SodaSDK.Initialize();
            // }
    
            var remoteConfigs = SodaSDK.RemoteConfig.GetAllConfigs();
    
            if (remoteConfigs == null || remoteConfigs.Count == 0)
            {
                EditorUtility.DisplayDialog("No Remote Config", "No remote configuration found. Make sure to fetch config first.", "OK");
                return;
            }
            
            // Clear existing overrides
            if (EditorUtility.DisplayDialog("Import Remote Config", 
                $"This will replace all {_overridesProp.arraySize} existing override entries with {remoteConfigs.Count} remote config entries. Continue?", 
                "Yes", "Cancel"))
            {
                _overridesProp.ClearArray();
                
                foreach (var kvp in remoteConfigs)
                {
                    AddOverrideFromRemoteValue(kvp.Key, kvp.Value);
                }
                
                serializedObject.ApplyModifiedProperties();
                EditorUtility.DisplayDialog("Import Complete", $"Imported {remoteConfigs.Count} remote config entries as overrides.", "OK");
            }
        }
        
        private void AddOverrideFromRemoteValue(string key, object value)
        {
            _overridesProp.InsertArrayElementAtIndex(_overridesProp.arraySize);
            SerializedProperty newOverride = _overridesProp.GetArrayElementAtIndex(_overridesProp.arraySize - 1);
            
            newOverride.FindPropertyRelative("key").stringValue = key;
            
            if (value is string)
            {
                newOverride.FindPropertyRelative("valueType").enumValueIndex = (int)SodaOverrideConfigSO.ConfigValueType.String;
                newOverride.FindPropertyRelative("stringValue").stringValue = (string)value;
            }
            else if (value is int)
            {
                newOverride.FindPropertyRelative("valueType").enumValueIndex = (int)SodaOverrideConfigSO.ConfigValueType.Int;
                newOverride.FindPropertyRelative("intValue").intValue = (int)value;
            }
            else if (value is float)
            {
                newOverride.FindPropertyRelative("valueType").enumValueIndex = (int)SodaOverrideConfigSO.ConfigValueType.Float;
                newOverride.FindPropertyRelative("floatValue").floatValue = (float)value;
            }
            else if (value is bool)
            {
                newOverride.FindPropertyRelative("valueType").enumValueIndex = (int)SodaOverrideConfigSO.ConfigValueType.Bool;
                newOverride.FindPropertyRelative("boolValue").boolValue = (bool)value;
            }
            else if (value is Color)
            {
                newOverride.FindPropertyRelative("valueType").enumValueIndex = (int)SodaOverrideConfigSO.ConfigValueType.Color;
                newOverride.FindPropertyRelative("colorValue").colorValue = (Color)value;
            }
            else
            {
                // Default to string
                newOverride.FindPropertyRelative("valueType").enumValueIndex = (int)SodaOverrideConfigSO.ConfigValueType.String;
                newOverride.FindPropertyRelative("stringValue").stringValue = value?.ToString() ?? "";
            }
        }
    }
}

