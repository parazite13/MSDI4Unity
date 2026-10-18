using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;

namespace MSDI4Unity.Tests
{
    public class TransientDisposableTestServiceRegister : ServiceRegisterScriptableObject
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IDisposableTestService, DisposableTestService>();
        }
    }
}
