using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Extensions;
using MSDI4Unity.Sample.Services;
using System;
using UnityEngine;

namespace MSDI4Unity.Sample.Behaviours
{
    public class HelloWorldBehaviour : MonoBehaviour
    {
        // Retrieve services from either Awake or Start
        private void Awake()
        {
            // Retrieve the IServiceProvider using extensions methods
            IServiceProvider serviceProvider = this.GetServiceProvider();
            // Retrieve services like you would do with MSDI
            IHelloWorldService helloWorldService = serviceProvider.GetRequiredService<IHelloWorldService>();

            // Use your services as you want
            helloWorldService.SayHello();
        }
    }
}