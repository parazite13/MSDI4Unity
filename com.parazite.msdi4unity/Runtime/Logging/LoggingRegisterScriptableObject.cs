#if MSDI_LOGGING

using Microsoft.Extensions.Logging;
using UnityEngine;

namespace MSDI4Unity.Logging
{
    public abstract class LoggingRegisterScriptableObject : ScriptableObject, ILoggingRegister
    {
        public abstract void RegisterLogging(ILoggingBuilder builder);
    }
}
#endif