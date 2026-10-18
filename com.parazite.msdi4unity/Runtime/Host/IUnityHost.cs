using Microsoft.Extensions.Hosting;
using System;
using UnityEngine.SceneManagement;

namespace MSDI4Unity.Host
{
    public interface IUnityHost : IHost
    {
        IServiceProvider GetSceneServiceProvider(Scene scene);
    }
}