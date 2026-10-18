using Microsoft.Extensions.DependencyInjection;
using MSDI4Unity.Host;
using System;
#if MSDI_LOGGING
using Microsoft.Extensions.Logging;
using MSDI4Unity.Logging;
#endif
#if MSDI_CONFIGURATION
using Microsoft.Extensions.Configuration;
#endif

namespace MSDI4Unity.Extensions
{
    /// <summary>
    /// Extension methods for creating and configuring a UnityHostBuilder.
    /// </summary>
    public static class UnityHostBuilderExtensions
    {
#if MSDI_LOGGING
        /// <summary>
        /// Adds the Unity logger provider to the host builder.
        /// </summary>
        public static IUnityHostBuilder AddLogging(this IUnityHostBuilder builder, Action<ILoggingBuilder> loggingBuilderAction = null)
        {
            builder.ConfigureServices(services =>
            {
                loggingBuilderAction ??= builder => { };
                services.AddLogging(loggingBuilderAction);
            });

            return builder;
        }
#endif

#if MSDI_CONFIGURATION
        /// <summary>
        /// Adds Configuration support to the Unity host builder. Optionally accepts a ConfigurationBuilder to customize configuration sources.
        /// </summary>
        public static IUnityHostBuilder AddConfiguration(this IUnityHostBuilder builder, Action<IConfigurationBuilder> configureBuilderAction = null)
        {
            builder.ConfigureServices(services =>
            {
                var configurationBuilder = new ConfigurationBuilder();
                configureBuilderAction?.Invoke(configurationBuilder);
                IConfigurationRoot configurationRoot = configurationBuilder.Build();

                services.AddSingleton<IConfiguration>(configurationRoot);
                services.AddSingleton<IConfigurationRoot>(sp => (IConfigurationRoot)sp.GetRequiredService<IConfiguration>());
            });

            return builder;
        }
#endif

            /// <summary>
            /// Configures services.
            /// </summary>
            /// <param name="builder">The host builder.</param>
            /// <param name="configure">Action to configure services for this scope.</param>
            /// <returns>The host builder for chaining.</returns>
        public static IUnityHostBuilder ConfigureServices(this IUnityHostBuilder builder, Action<IServiceCollection> configure)
        {
            builder.ConfigureServices(configure);
            return builder;
        }
    }
}