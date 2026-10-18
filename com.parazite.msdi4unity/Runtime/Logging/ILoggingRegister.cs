#if MSDI_LOGGING
using Microsoft.Extensions.Logging;

namespace MSDI4Unity.Logging
{
    public interface ILoggingRegister
    {
        public void RegisterLogging(ILoggingBuilder builder);
    }
}
#endif