using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;

namespace MSDI4Unity.Tests
{
    public class ScopeTestService2Register : ServiceRegisterScriptableObject
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ITestService2, TestService2>();
        }
    }
}