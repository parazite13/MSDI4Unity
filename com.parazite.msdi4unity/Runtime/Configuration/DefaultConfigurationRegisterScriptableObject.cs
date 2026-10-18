#if MSDI_CONFIGURATION

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSDI4Unity.Configuration
{
    [CreateAssetMenu(fileName = "DefaultConfigurationRegister", menuName = "MSDI4Unity/Configuration/Default Configuration Register")]
    public class DefaultConfigurationRegisterScriptableObject : ConfigurationRegisterScriptableObject
    {
        [Serializable]
        public struct ConfigEntry
        {
            [Tooltip("Configuration Key (e.g. Api:Url or TargetFrameRate)")]
            public string Key;

            [Tooltip("Configuration Value")]
            public string Value;
        }

        [SerializeField]
        private List<ConfigEntry> entries = new();

        public IReadOnlyList<ConfigEntry> Entries => entries;

        public override void RegisterConfiguration(IConfigurationBuilder builder)
        {
            builder.AddInMemoryCollection(entries.ToDictionary(entry => entry.Key, entry => entry.Value));
        }
    }
}
#endif