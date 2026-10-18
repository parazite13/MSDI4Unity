using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if MSDI_LOGGING
using MSDI4Unity.Logging;
#endif
#if MSDI_CONFIGURATION
using MSDI4Unity.Configuration;
#endif

namespace MSDI4Unity.Register
{
    public class MSDISettings : ScriptableObject
    {
        public const string msdiDefaultSettingsPath = "Assets/Settings/Resources/MSDI4Unity.asset";

        [SerializeField]
        internal ServiceRegisterScriptableObject globalServiceRegister;
        public IServiceCollectionRegister GlobalServiceRegister => globalServiceRegister;

        [SerializeField]
        internal List<SceneServiceMapping> sceneServiceRegister = new();
        public IReadOnlyList<SceneServiceMapping> SceneServiceRegister => sceneServiceRegister;

#if MSDI_LOGGING
        [SerializeField]
        internal LoggingRegisterScriptableObject loggingRegister;
        public ILoggingRegister LoggingRegister => loggingRegister;
#endif

#if MSDI_CONFIGURATION
        [SerializeField]
        internal ConfigurationRegisterScriptableObject configurationRegister;
        public IConfigurationRegister ConfigurationRegister => configurationRegister;
#endif

        public ServiceRegisterScriptableObject GetRegisterForScenePath(string scenePath)
        {
            foreach (var m in sceneServiceRegister)
            {
                if (string.Equals(m.ScenePath, scenePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    return m.ServiceRegister;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        internal static MSDISettings GetOrCreateSettings()
        {
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<MSDISettings>(msdiDefaultSettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<MSDISettings>();
                string settingsFullPath = Directory.GetParent(msdiDefaultSettingsPath).FullName;
                if (!Directory.Exists(settingsFullPath))
                {
                    Directory.CreateDirectory(settingsFullPath);
                }
                UnityEditor.AssetDatabase.CreateAsset(settings, msdiDefaultSettingsPath);
                UnityEditor.AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static UnityEditor.SerializedObject GetSerializedSettings()
        {
            return new UnityEditor.SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}