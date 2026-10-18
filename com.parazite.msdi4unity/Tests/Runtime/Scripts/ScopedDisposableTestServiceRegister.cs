using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;

namespace MSDI4Unity.Tests
{
    public class ScopedDisposableTestServiceRegister : ServiceRegisterScriptableObject
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IDisposableTestService, DisposableTestService>();
        }
    }
}
