using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;
using MSDI4Unity.Sample.Services;
using UnityEngine;

namespace MSDI4Unity.Sample.Registers
{
    [CreateAssetMenu]
    public class ProjectServiceRegister : ServiceRegisterScriptableObject
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IHelloWorldService, HelloWorldService>();
            services.AddSingleton<DisposableService>();
        }
    }
}