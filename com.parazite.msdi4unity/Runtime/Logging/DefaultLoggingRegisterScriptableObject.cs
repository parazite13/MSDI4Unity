#if MSDI_LOGGING

using Microsoft.Extensions.Logging;
using UnityEngine;

namespace MSDI4Unity.Logging
{
    [CreateAssetMenu(fileName = "DefaultLoggingRegister", menuName = "MSDI/Logging/DefaultLoggingRegister")]
    public class DefaultLoggingRegisterScriptableObject : LoggingRegisterScriptableObject
    {
        public override void RegisterLogging(ILoggingBuilder builder)
        {
            builder.AddProvider(new UnityLoggerProvider());
        }
    }
}
#endif