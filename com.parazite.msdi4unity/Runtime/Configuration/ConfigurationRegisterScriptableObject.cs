#if MSDI_CONFIGURATION

using Microsoft.Extensions.Configuration;
using UnityEngine;

namespace MSDI4Unity.Configuration
{
    public abstract class ConfigurationRegisterScriptableObject : ScriptableObject, IConfigurationRegister
    {
        public abstract void RegisterConfiguration(IConfigurationBuilder builder);
    }
}
#endif