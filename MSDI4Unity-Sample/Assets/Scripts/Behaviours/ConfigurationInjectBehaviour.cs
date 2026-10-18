using Microsoft.Extensions.Configuration;
using MSDI4Unity.Attributes;
using MSDI4Unity.Extensions;
using UnityEngine;

namespace MSDI4Unity.Sample.Behaviours
{
    public class ConfigurationInjectBehaviour : MonoBehaviour
    {
#if MSDI_CONFIGURATION
        [Inject]
        private readonly IConfiguration configuration;

        private void Awake()
        {
            this.Inject();
            foreach (var kvp in configuration.AsEnumerable())
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}");
            }
        }
#endif
    }
}