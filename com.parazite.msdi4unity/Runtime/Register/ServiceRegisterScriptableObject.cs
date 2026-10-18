using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

namespace MSDI4Unity.Register
{
    public abstract class ServiceRegisterScriptableObject : ScriptableObject, IServiceCollectionRegister
    {
        public abstract void RegisterServices(IServiceCollection services);
    }
}