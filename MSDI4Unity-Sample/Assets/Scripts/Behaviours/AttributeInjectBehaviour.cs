using MSDI4Unity.Attributes;
using MSDI4Unity.Extensions;
using MSDI4Unity.Sample.Services;
using UnityEngine;

namespace MSDI4Unity.Sample.Behaviours
{
    public class AttributeInjectBehaviour : MonoBehaviour
    {
        // Retrieve services from InjectAttribute
        [Inject]
        private readonly IHelloWorldService helloWorldService;

        private void Awake()
        {
            // Fill the fields with services retrieved from service provider
            this.Inject();

            // Use your services as you want
            helloWorldService.SayHello();
        }
    }
}