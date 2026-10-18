using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSDI4Unity.Register;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace MSDI4Unity.Scoping
{
    /// <summary>
    /// Manages service scopes tied to Unity scenes.
    /// When a scene is loaded, creates a service scope; when unloaded, disposes it.
    /// </summary>
    internal class SceneServiceScopeManager : IDisposable
    {
        private readonly IServiceProvider rootServiceProvider;
        private readonly IServiceCollection rootServiceCollection;
        private readonly MSDISettings settings;
        private readonly Dictionary<string, SceneServiceProvider> sceneProviders;
        private readonly ILogger logger;

        internal SceneServiceScopeManager(
            IServiceProvider rootServiceProvider,
            IServiceCollection rootServiceCollection,
            MSDISettings settings,
            ILogger<SceneServiceScopeManager> logger)
        {
            this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException(nameof(rootServiceProvider));
            this.rootServiceCollection = rootServiceCollection ?? throw new ArgumentNullException(nameof(rootServiceCollection));
            this.settings = settings;
            this.logger = logger ?? NullLogger<SceneServiceScopeManager>.Instance;

            sceneProviders = new Dictionary<string, SceneServiceProvider>();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// Get or create a service provider for a specific scene.
        /// </summary>
        public IServiceProvider GetOrCreateSceneProvider(Scene scene)
        {
            if (sceneProviders.TryGetValue(scene.path, out var sceneProvider))
            {
                return sceneProvider;
            }
            else
            {
                // Create a new scope for the loaded scene
                IServiceScope rootScope = rootServiceProvider.CreateScope();

                // Register scene-specific services if any are defined in the settings
                Action<IServiceCollection> sceneCollectionRegister = null;
                if (settings.SceneServiceRegister.FirstOrDefault(m => m.ScenePath == scene.path) is SceneServiceMapping sceneServiceMapping)
                {
                    if (sceneServiceMapping.ServiceRegister != null)
                    {
                        sceneCollectionRegister = sceneServiceMapping.ServiceRegister.RegisterServices;
                    }
                    else
                    {
                        logger.LogWarning($"No ServiceRegisterScriptableObject defined for scene {scene.path} in settings.");
                    }
                }

                SceneServiceProvider sceneServiceProvider = new(
                    rootScope.ServiceProvider,
                    rootServiceCollection,
                    sceneCollectionRegister);

                sceneProviders[scene.path] = sceneServiceProvider;

                return sceneServiceProvider;
            }
        }

        /// <summary>
        /// Disposes all scene scopes.
        /// </summary>
        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            foreach (var scope in sceneProviders.Values)
            {
                scope?.Dispose();
            }

            sceneProviders.Clear();
        }

        private void DisposeSceneScope(Scene scene)
        {
            if (sceneProviders.TryGetValue(scene.path, out var sceneProvider))
            {
                sceneProvider?.Dispose();
                sceneProviders.Remove(scene.path);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GetOrCreateSceneProvider(scene);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            DisposeSceneScope(scene);
        }
    }
}