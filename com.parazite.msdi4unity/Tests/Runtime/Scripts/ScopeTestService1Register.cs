using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;

namespace MSDI4Unity.Tests
{
    public class ScopeTestService1Register : ServiceRegisterScriptableObject
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ITestService1, TestService1>();
        }
    }
}