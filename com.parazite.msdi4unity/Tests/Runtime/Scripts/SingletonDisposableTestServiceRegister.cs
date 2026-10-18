using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;

namespace MSDI4Unity.Tests
{
    public class SingletonDisposableTestServiceRegister : ServiceRegisterScriptableObject
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IDisposableTestService, DisposableTestService>();
        }
    }
}
