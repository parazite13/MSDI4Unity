using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Register;
using UnityEngine;

namespace MSDI4Unity.Sample.Registers
{
    [CreateAssetMenu]
    public class SampleSceneServiceRegister : ServiceRegisterScriptableObject
    {
        public override void RegisterServices(IServiceCollection services)
        {

        }
    }
}