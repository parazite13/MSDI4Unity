using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

[InitializeOnLoad]
public static class OptionalScriptingSymbolLoader
{
    // Mapping: Target Assembly Name -> Scripting Define Symbol
    private static readonly Dictionary<string, string> AssemblyToSymbolMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Microsoft.Extensions.Configuration", "MSDI_CONFIGURATION" },
        { "Microsoft.Extensions.Logging", "MSDI_LOGGING" }
    };

    static OptionalScriptingSymbolLoader()
    {
        RegisterScriptingSymbols();
    }

    internal static void RegisterScriptingSymbols()
    {
        HashSet<string> loadedAssemblies = new(
            AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name),
            StringComparer.OrdinalIgnoreCase
        );

        NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string currentDefines = PlayerSettings.GetScriptingDefineSymbols(target);
        HashSet<string> definesSet = new(currentDefines.Split(';', StringSplitOptions.RemoveEmptyEntries));

        bool isModified = false;

        foreach (var kvp in AssemblyToSymbolMap)
        {
            string assemblyName = kvp.Key;
            string defineSymbol = kvp.Value;

            bool hasAssembly = loadedAssemblies.Contains(assemblyName);

            if (hasAssembly && definesSet.Add(defineSymbol))
            {
                isModified = true;
                Debug.Log($"[DI Framework] Defined symbol: {defineSymbol}");
            }
            else if (!hasAssembly && definesSet.Remove(defineSymbol))
            {
                isModified = true;
                Debug.Log($"[DI Framework] Removed symbol: {defineSymbol}");
            }
        }

        // Only write back if symbol list actually changed
        if (isModified)
        {
            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", definesSet));
        }
    }
}