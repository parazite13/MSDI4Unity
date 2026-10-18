using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSDI4Unity.Register;
using MSDI4Unity.Scoping;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MSDI4Unity.Extensions;

namespace MSDI4Unity.Host
{
    public class UnityHost : IUnityHost
    {
        private readonly IServiceProvider unityServiceProviders;

        private SceneServiceScopeManager sceneServiceScopeManager;

        public IServiceProvider Services => unityServiceProviders;

        public static IUnityHost Instance { get; internal set; }

        public UnityHost(IServiceProvider unityServiceProviders)
        {
            this.unityServiceProviders = unityServiceProviders ?? throw new ArgumentNullException(nameof(unityServiceProviders));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeHost()
        {
            var settings = Resources.Load<MSDISettings>("MSDI4Unity");
            if (settings == null)
            {
                throw new InvalidOperationException("MSDISettings could not be found in Resources. Open project settings and ensure the asset is properly configured.");
            }

            if(settings.GlobalServiceRegister == null)
            {
                throw new InvalidOperationException("GlobalServiceRegister is not set in MSDISettings. Open project settings and ensure the asset is properly configured.");
            }

            Instance = CreateUnityHostBuilder()
                .ConfigureServices(services =>
                {
                    settings.GlobalServiceRegister.RegisterServices(services);
                    services.AddSingleton(sp => new SceneServiceScopeManager(
                        sp,
                        services,
                        settings,
                        sp.GetService<ILogger<SceneServiceScopeManager>>())
                    );
                })
#if MSDI_LOGGING
                .AddLogging(loggingBuilder =>
                {
                    if(settings.LoggingRegister == null)
                    {
                        Debug.LogWarning("LoggingRegister is not set in MSDISettings. Open project settings and ensure the asset is properly configured.");
                    }
                    else
                    {
                        settings.LoggingRegister.RegisterLogging(loggingBuilder);
                    }
                })
#endif
#if MSDI_CONFIGURATION
                .AddConfiguration(configurationBuilder =>
                {
                    if(settings.ConfigurationRegister == null)
                    {
                        Debug.LogWarning("ConfigurationRegister is not set in MSDISettings. Open project settings and ensure the asset is properly configured.");
                    }
                    else
                    {
                        settings.ConfigurationRegister.RegisterConfiguration(configurationBuilder);
                    }
                })
#endif
                .Build();

            Instance?.Start();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            Application.quitting += OnApplicationQuit;
#endif
        }

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                Instance?.Dispose();
            }
        }
#else
        private static void OnApplicationQuit()
        {
            Instance?.Dispose();
        }
#endif

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            // Force the creation of the SceneServiceScopeManager to ensure it is initialized before any scenes are loaded.
            sceneServiceScopeManager = Instance.Services.GetRequiredService<SceneServiceScopeManager>();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            Dispose();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (unityServiceProviders is IDisposable d)
            {
                d.Dispose();
            }
            else if (unityServiceProviders is IAsyncDisposable ad)
            {
                // fire-and-forget best-effort for async dispose to avoid blocking Unity shutdown
                _ = ad.DisposeAsync().AsTask();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (unityServiceProviders is IAsyncDisposable ad)
            {
                await ad.DisposeAsync().ConfigureAwait(false);
                return;
            }

            if (unityServiceProviders is IDisposable d)
            {
                d.Dispose();
            }
        }

        public IServiceProvider GetSceneServiceProvider(Scene scene)
        {
            return sceneServiceScopeManager.GetOrCreateSceneProvider(scene);
        }

        /// <summary>
        /// Creates a new IUnityHostBuilder configured with Unity-specific defaults.
        /// </summary>
        /// <returns>A new IUnityHostBuilder instance configured for Unity.</returns>
        public static IUnityHostBuilder CreateUnityHostBuilder()
        {
            IUnityHostBuilder builder = new UnityHostBuilder();

            // Configure default services for Unity
            builder.ConfigureServices(services =>
            {
                // Register Unity-specific host lifetime manager
                services.AddSingleton<IHostLifetime, UnityHostLifetime>();
                services.AddSingleton<IHostApplicationLifetime, UnityApplicationLifetime>();
            });

            return builder;
        }
    }
}
