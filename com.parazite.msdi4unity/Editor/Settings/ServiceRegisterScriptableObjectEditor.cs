using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ServiceRegisterScriptableObject), true)]
public class ServiceRegisterScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Render standard Inspector properties first
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Registered Services", EditorStyles.boldLabel);

        ServiceRegisterScriptableObject serviceScriptableObject = (ServiceRegisterScriptableObject)target;
        IServiceCollection serviceDescriptors = new ServiceCollection();
        serviceScriptableObject.RegisterServices(serviceDescriptors);

        if (serviceDescriptors.Count == 0)
        {
            EditorGUILayout.HelpBox("No services registered.", MessageType.Info);
            return;
        }

        // Calculate column widths based on current inspector width
        float totalWidth = EditorGUIUtility.currentViewWidth - 24f; // Margin padding
        float col1Width = totalWidth * 0.35f; // Service Type
        float col2Width = totalWidth * 0.45f; // Implementation Type
        float col3Width = totalWidth * 0.20f; // Lifetime

        // --- Draw Table Header ---
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Service Type", EditorStyles.boldLabel, GUILayout.Width(col1Width));
        GUILayout.Label("Implementation", EditorStyles.boldLabel, GUILayout.Width(col2Width));
        GUILayout.Label("Lifetime", EditorStyles.boldLabel, GUILayout.Width(col3Width));
        EditorGUILayout.EndHorizontal();

        // --- Draw Table Rows ---
        EditorGUILayout.BeginVertical(GUI.skin.box);

        bool isEven = false;
        foreach (var service in serviceDescriptors.OrderBy(s => s.ServiceType?.Name))
        {
            // Optional alternating row color for readability
            Rect rowRect = EditorGUILayout.BeginHorizontal();
            if (isEven)
            {
                EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 1f, 0.03f));
            }

            string serviceTypeName = service.ServiceType?.Name ?? "Null";
            string implTypeName = service.ImplementationType?.Name
                ?? (service.ImplementationInstance != null ? service.ImplementationInstance.GetType().Name : "Factory/Instance");
            string lifetimeStr = service.Lifetime.ToString();

            // Render labels with tooltip showing full type name on hover
            GUILayout.Label(new GUIContent(serviceTypeName, service.ServiceType?.FullName), GUILayout.Width(col1Width));
            GUILayout.Label(new GUIContent(implTypeName, service.ImplementationType?.FullName), GUILayout.Width(col2Width));
            GUILayout.Label(lifetimeStr, GUILayout.Width(col3Width));

            EditorGUILayout.EndHorizontal();
            isEven = !isEven;
        }

        EditorGUILayout.EndVertical();
    }
}