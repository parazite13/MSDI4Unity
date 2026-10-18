using Microsoft.Extensions.DependencyInjection;
using System;

namespace MSDI4Unity.Host
{
    /// <summary>
    /// Ultra-simple Unity host builder interface.
    /// Provides minimal API for configuring services and building IUnityHost.
    /// </summary>
    public interface IUnityHostBuilder
    {
        /// <summary>
        /// Adds services to the container.
        /// </summary>
        /// <param name="configure">Configuration action for services.</param>
        /// <returns>This builder for chaining.</returns>
        IUnityHostBuilder ConfigureServices(Action<IServiceCollection> configure);

        /// <summary>
        /// Builds and returns the Unity host.
        /// </summary>
        /// <returns>The configured host.</returns>
        IUnityHost Build();
    }
}