using MSDI4Unity.Register;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class MSDIProjectSettingsRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
        var provider = new SettingsProvider("Project/MSDI4Unity", SettingsScope.Project)
        {
            label = "MSDI4Unity",
            guiHandler = (searchContext) => MSDIProjectSettingsEditor.DrawSettingsGUI(),
            keywords = new HashSet<string>(new[] { "DI", "MSDI4Unity", "MSDI" })
        };

        return provider;
    }
}

public class MSDIProjectSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawSettingsGUI();
    }

    public static void DrawSettingsGUI()
    {
        var settings = MSDISettings.GetSerializedSettings();
        // Ensure the serialized object is up-to-date before drawing
        settings.Update();
#if MSDI_LOGGING
        EditorGUILayout.PropertyField(settings.FindProperty("loggingRegister"), new GUIContent("Logging Register"));
#endif
#if MSDI_CONFIGURATION
        EditorGUILayout.PropertyField(settings.FindProperty("configurationRegister"), new GUIContent("Configuration Register"));
#endif
        EditorGUILayout.PropertyField(settings.FindProperty("globalServiceRegister"), new GUIContent("Global Service Register"));

        // If a sceneServiceRegister list exists, show an Add button to insert a new element
        var listPropCheck = settings.FindProperty("sceneServiceRegister");
        if (listPropCheck != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Service Register", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add per Scene configuration", GUILayout.Width(240)))
            {
                int idx = listPropCheck.arraySize;
                listPropCheck.InsertArrayElementAtIndex(idx);
                var newElem = listPropCheck.GetArrayElementAtIndex(idx);
                var scenePathProp = newElem.FindPropertyRelative("scenePath");
                if (scenePathProp != null)
                    scenePathProp.stringValue = string.Empty;
                var registerProp = newElem.FindPropertyRelative("serviceRegister");
                if (registerProp != null)
                    registerProp.objectReferenceValue = null;

                // Apply and persist immediately so the new element is saved and visible
                if (settings.ApplyModifiedProperties())
                    AssetDatabase.SaveAssets();
                // Refresh the serialized object so subsequent UI draws show the new item
                settings.Update();
            }
            EditorGUILayout.EndHorizontal();
        }

        var listProp = settings.FindProperty("sceneServiceRegister");
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            var scenePathProp = element.FindPropertyRelative("scenePath");
            var registerProp = element.FindPropertyRelative("serviceRegister");

            SceneAsset current = string.IsNullOrEmpty(scenePathProp.stringValue)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProp.stringValue);

            EditorGUILayout.BeginHorizontal();
            var newScene = (SceneAsset)EditorGUILayout.ObjectField(current, typeof(SceneAsset), false);
            if (newScene != current)
                scenePathProp.stringValue = newScene ? AssetDatabase.GetAssetPath(newScene) : string.Empty;

            // Draw the service register field and a delete button at the end of the line
            EditorGUILayout.PropertyField(registerProp, GUIContent.none);

            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                // Remove the element from the array and persist
                listProp.DeleteArrayElementAtIndex(i);
                if (settings.ApplyModifiedProperties())
                    AssetDatabase.SaveAssets();
                // Refresh serialized data and stop drawing this frame to avoid index issues
                settings.Update();
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (settings.ApplyModifiedProperties())
        {
            AssetDatabase.SaveAssets();
        }
    }
}
