using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Hosting;

namespace MSDI4Unity.Host
{
    /// <summary>
    /// Ultra-simple Unity Host Builder.
    /// Creates an IServiceProvider with Unity-specific defaults (logger, lifetime).
    /// </summary>
    public class UnityHostBuilder : IUnityHostBuilder
    {
        private readonly ServiceCollection services;
        private bool built;

        public UnityHostBuilder()
        {
            services = new ServiceCollection();
            built = false;
        }

        /// <summary>
        /// Configures services in the container.
        /// </summary>
        public IUnityHostBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            if (built)
            {
                throw new InvalidOperationException("Cannot configure services after Build() has been called.");
            }

            configure?.Invoke(services);
            return this;
        }

        /// <summary>
        /// Builds the host with configured services.
        /// </summary>
        public IUnityHost Build()
        {
            if (built)
            {
                throw new InvalidOperationException("Build() can only be called once.");
            }

            built = true;

            if (!ServiceExists<IHostLifetime>())
            {
                services.AddSingleton<IHostLifetime, UnityHostLifetime>();
            }

            // Build the service provider
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Return wrapped in UnityHost
            return new UnityHost(serviceProvider);
        }

        /// <summary>
        /// Checks if a service type is already registered.
        /// </summary>
        private bool ServiceExists<T>() where T : class
        {
            foreach (var descriptor in services)
            {
                if (descriptor.ServiceType == typeof(T))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Factory extension for creating a UnityHostBuilder.
    /// </summary>
    public static class UnityHostBuilderFactory
    {
        /// <summary>
        /// Creates a new Unity host builder.
        /// </summary>
        public static IUnityHostBuilder CreateUnityHostBuilder()
        {
            return new UnityHostBuilder();
        }
    }
}
