#if MSDI_CONFIGURATION
using Microsoft.Extensions.Configuration;

namespace MSDI4Unity.Configuration
{
    public interface IConfigurationRegister
    {
        public void RegisterConfiguration(IConfigurationBuilder builder);
    }
}
#endif