using System;
using Soda.Runtime;
using UnityEngine;
using UnityEditor;

namespace Soda.EditorTools
{
    public class SodaConfigMonitorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        
        [MenuItem("SodaSDK/Config Monitor")]
        public static void ShowWindow()
        {
            GetWindow<SodaConfigMonitorWindow>("Soda Config Monitor");
        }

        private void OnFocus()
        {
            SodaSDK.Initialize();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Soda SDK Configuration Monitor", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Bundle ID: {SodaSDK.BundleId}");
            EditorGUILayout.LabelField($"Server URL: {SodaSDK.ServerUrl}");
            EditorGUILayout.LabelField($"Using Overrides: {SodaSDK.RemoteConfig.IsUsingOverrides}");
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fetch Config"))
            {
                SodaSDK.RemoteConfig.FetchConfig("default", null, false);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Current Configuration Values", EditorStyles.boldLabel);
            
            var allConfigs = SodaSDK.RemoteConfig.GetAllConfigs();
            if (allConfigs == null || allConfigs.Count == 0)
            {
                EditorGUILayout.HelpBox("No configuration values loaded", MessageType.Info);
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var kvp in allConfigs)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(150));
                    EditorGUILayout.LabelField(kvp.Value?.ToString() ?? "null");
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}