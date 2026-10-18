using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;

namespace MSDI4Unity.Tests
{
    public class TransientTestService0Register : ServiceRegisterScriptableObject
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<ITestService0, TestService0>();
        }
    }
}