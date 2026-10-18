using System;
using UnityEngine;

namespace MSDI4Unity.Sample.Services
{
    public class DisposableService : IDisposable
    {
        // Called when the service is initialized 
        public DisposableService()
        {
            Debug.Log("DisposableService is being initialized!");
        }

        // Called when the service is disposed (for example, when the scene is unloaded or the application is closed)
        public void Dispose()
        {
            Debug.Log("DisposableService is being disposed!");
        }
    }
}