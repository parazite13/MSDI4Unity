using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSDI4Unity.Host;
using MSDI4Unity.Register;
using MSDI4Unity.Scoping;
using MSDI4Unity.Extensions;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MSDI4Unity.Tests
{
    public class MSDI4UnityTests
    {
        private const string bootstrapScenePath = "Packages/com.parazite.msdi4unity/Tests/Runtime/Scenes/MSDITestBootstrap.unity";
        private const string testScene1Path = "Packages/com.parazite.msdi4unity/Tests/Runtime/Scenes/MSDITestScene1.unity";
        private const string testScene2Path = "Packages/com.parazite.msdi4unity/Tests/Runtime/Scenes/MSDITestScene2.unity";
        
        [UnityTest]
        public IEnumerator ProjectSingletonServices_AreSharedAcrossScenes()
        {
            // Arrange
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<SingletonTestService0Register>();
            settings.sceneServiceRegister = new();
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load first scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var sceneA = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(sceneA.isLoaded, "Scene A should be loaded");
            var providerA = UnityHost.Instance.GetSceneServiceProvider(sceneA);
            Assert.IsNotNull(providerA, "Provider for scene A should not be null");
            var projServiceA = providerA.GetRequiredService<ITestService0>();

            Assert.IsNotNull(projServiceA, "Project scoped service should resolve in scene A");

            // Load second scene additively
            yield return LoadScene(testScene2Path, LoadSceneMode.Additive);
            var sceneB = SceneManager.GetSceneByPath(testScene2Path);
            Assert.IsTrue(sceneB.isLoaded, "Scene B should be loaded");
            var providerB = UnityHost.Instance.GetSceneServiceProvider(sceneB);
            Assert.IsNotNull(providerB, "Provider for scene B should not be null");
            var projServiceB = providerB.GetRequiredService<ITestService0>();

            Assert.IsNotNull(projServiceB, "Project scoped service should resolve in scene B");

            // Singleton services registered at project level are created per-scene scope -> should be same instances
            Assert.IsTrue(ReferenceEquals(projServiceA, projServiceB), "Project-singleton services should be the same instance across scene scopes");

            // Cleanup: unload scenes
            yield return SceneManager.UnloadSceneAsync(sceneA);
            yield return SceneManager.UnloadSceneAsync(sceneB);
        }

        [UnityTest]
        public IEnumerator ProjectScopedServices_AreInstantiatedAcrossScenes()
        {
            // Arrange
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<ScopeTestService0Register>();
            settings.sceneServiceRegister = new();
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load first scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var sceneA = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(sceneA.isLoaded, "Scene A should be loaded");
            var providerA = UnityHost.Instance.GetSceneServiceProvider(sceneA);
            Assert.IsNotNull(providerA, "Provider for scene A should not be null");
            var projServiceA = providerA.GetRequiredService<ITestService0>();

            Assert.IsNotNull(projServiceA, "Project scoped service should resolve in scene A");

            // Load second scene additively
            yield return LoadScene(testScene2Path, LoadSceneMode.Additive);
            var sceneB = SceneManager.GetSceneByPath(testScene2Path);
            Assert.IsTrue(sceneB.isLoaded, "Scene B should be loaded");
            var providerB = UnityHost.Instance.GetSceneServiceProvider(sceneB);
            Assert.IsNotNull(providerB, "Provider for scene B should not be null");
            var projServiceB = providerB.GetRequiredService<ITestService0>();

            Assert.IsNotNull(projServiceB, "Project scoped service should resolve in scene B");

            // Scoped services registered at project level are created per-scene scope -> should be different instances
            Assert.IsFalse(ReferenceEquals(projServiceA, projServiceB), "Project-scoped services should be different instances across scene scopes");

            // Cleanup: unload scenes
            yield return SceneManager.UnloadSceneAsync(sceneA);
            yield return SceneManager.UnloadSceneAsync(sceneB);
        }

        [UnityTest]
        public IEnumerator ProjectTransientServices_AreInstantiatedAcrossScenes()
        {
            // Arrange
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<TransientTestService0Register>();
            settings.sceneServiceRegister = new();
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load first scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var sceneA = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(sceneA.isLoaded, "Scene A should be loaded");
            var providerA = UnityHost.Instance.GetSceneServiceProvider(sceneA);
            Assert.IsNotNull(providerA, "Provider for scene A should not be null");
            var projServiceA = providerA.GetRequiredService<ITestService0>();

            Assert.IsNotNull(projServiceA, "Project scoped service should resolve in scene A");

            // Load second scene additively
            yield return LoadScene(testScene2Path, LoadSceneMode.Additive);
            var sceneB = SceneManager.GetSceneByPath(testScene2Path);
            Assert.IsTrue(sceneB.isLoaded, "Scene B should be loaded");
            var providerB = UnityHost.Instance.GetSceneServiceProvider(sceneB);
            Assert.IsNotNull(providerB, "Provider for scene B should not be null");
            var projServiceB = providerB.GetRequiredService<ITestService0>();

            Assert.IsNotNull(projServiceB, "Project transient service should resolve in scene B");

            // Transient services registered at project level are created per-scene scope -> should be different instances
            Assert.IsFalse(ReferenceEquals(projServiceA, projServiceB), "Project-transient services should be different instances across scene scopes");

            // Cleanup: unload scenes
            yield return SceneManager.UnloadSceneAsync(sceneA);
            yield return SceneManager.UnloadSceneAsync(sceneB);
        }

        [UnityTest]
        public IEnumerator SceneScopedServices_ResolveFromSceneContainer()
        {
            // Arrange
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<SingletonTestService0Register>();
            settings.sceneServiceRegister = new System.Collections.Generic.List<SceneServiceMapping>
            {
                new()
                {
                    scenePath = testScene1Path,
                    serviceRegister = ScriptableObject.CreateInstance<ScopeTestService1Register>()
                },
                new()
                {
                    scenePath = testScene2Path,
                    serviceRegister = ScriptableObject.CreateInstance<ScopeTestService2Register>()
                }
            };
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load first scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var sceneA = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(sceneA.isLoaded, "Scene A should be loaded");

            var providerA = UnityHost.Instance.GetSceneServiceProvider(sceneA);
            Assert.IsNotNull(providerA, "Provider for scene A should not be null");
            var sceneServiceA1 = providerA.GetRequiredService<ITestService1>();
            var sceneServiceA2 = providerA.GetService<ITestService2>();

            Assert.IsNotNull(sceneServiceA1, "Scene service should resolve in scene A");
            Assert.IsNull(sceneServiceA2, "Scene service should not resolve in scene A if not registered");

            // Load second scene additively
            yield return LoadScene(testScene2Path, LoadSceneMode.Additive);
            var sceneB = SceneManager.GetSceneByPath(testScene2Path);
            Assert.IsTrue(sceneB.isLoaded, "Scene B should be loaded");

            var providerB = UnityHost.Instance.GetSceneServiceProvider(sceneB);
            Assert.IsNotNull(providerB, "Provider for scene B should not be null");
            var sceneServiceB1 = providerB.GetService<ITestService1>();
            var sceneServiceB2 = providerB.GetRequiredService<ITestService2>();

            Assert.IsNotNull(sceneServiceB2, "Scene service should resolve in scene B");
            Assert.IsNull(sceneServiceB1, "Scene service should not resolve in scene B if not registered");

            // Cleanup: unload scenes
            yield return SceneManager.UnloadSceneAsync(sceneA);
            yield return SceneManager.UnloadSceneAsync(sceneB);
        }

        [UnityTest]
        public IEnumerator SceneScopedServices_AreIsolatedPerScene()
        {
            // Arrange
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<SingletonTestService0Register>();
            settings.sceneServiceRegister = new System.Collections.Generic.List<SceneServiceMapping>
            {
                new()
                {
                    scenePath = testScene1Path,
                    serviceRegister = ScriptableObject.CreateInstance<ScopeTestService1Register>()
                },
                new()
                {
                    scenePath = testScene2Path,
                    serviceRegister = ScriptableObject.CreateInstance<ScopeTestService1Register>()
                }
            };
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load first scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var sceneA = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(sceneA.isLoaded, "Scene A should be loaded");

            var providerA = UnityHost.Instance.GetSceneServiceProvider(sceneA);
            Assert.IsNotNull(providerA, "Provider for scene A should not be null");
            var sceneServiceA = providerA.GetRequiredService<ITestService1>();

            Assert.IsNotNull(sceneServiceA, "Scene service should resolve in scene A");

            // Load second scene additively
            yield return LoadScene(testScene2Path, LoadSceneMode.Additive);
            var sceneB = SceneManager.GetSceneByPath(testScene2Path);
            Assert.IsTrue(sceneB.isLoaded, "Scene B should be loaded");

            var providerB = UnityHost.Instance.GetSceneServiceProvider(sceneB);
            Assert.IsNotNull(providerB, "Provider for scene B should not be null");
            var sceneServiceB = providerB.GetRequiredService<ITestService1>();

            Assert.IsNotNull(sceneServiceB, "Scene service should resolve in scene B");

            // Scene-specific services are registered per-scene container -> should be different instances
            Assert.IsFalse(ReferenceEquals(sceneServiceA, sceneServiceB), "Scene services should be different instances across scenes");

            // Cleanup: unload scenes
            yield return SceneManager.UnloadSceneAsync(sceneA);
            yield return SceneManager.UnloadSceneAsync(sceneB);
        }

        [UnityTest]
        public IEnumerator SingletonDisposableServices_AreDisposedWhenSceneUnloads()
        {
            // Arrange
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<SingletonTestService0Register>();
            settings.sceneServiceRegister = new System.Collections.Generic.List<SceneServiceMapping>
            {
                new()
                {
                    scenePath = testScene1Path,
                    serviceRegister = ScriptableObject.CreateInstance<ScopedDisposableTestServiceRegister>()
                }
            };
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load a scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(scene.isLoaded, "Scene should be loaded");

            var provider = UnityHost.Instance.GetSceneServiceProvider(scene);
            Assert.IsNotNull(provider, "Provider should not be null");
            var disposableService = provider.GetRequiredService<IDisposableTestService>();

            Assert.IsNotNull(disposableService, "Disposable service should resolve");
            Assert.IsFalse(disposableService.IsDisposed, "Service should not be disposed yet");
            Assert.AreEqual(1, DisposableTestService.GetUndisposedCount(), "Should have 1 undisposed instance");

            // Act: Unload the scene
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return null; // Wait one frame for cleanup to complete

            // Assert: Service should be disposed
            Assert.IsTrue(disposableService.IsDisposed, "Service should be disposed after scene unload");
            Assert.AreEqual(0, DisposableTestService.GetUndisposedCount(), "Should have 0 undisposed instances");
        }

        [UnityTest]
        public IEnumerator ScopedDisposableServices_AreDisposedWhenSceneUnloads()
        {
            // Arrange
            DisposableTestService.ResetTracking();
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<ScopedDisposableTestServiceRegister>();
            settings.sceneServiceRegister = new();
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load a scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(scene.isLoaded, "Scene should be loaded");

            var provider = UnityHost.Instance.GetSceneServiceProvider(scene);
            Assert.IsNotNull(provider, "Provider should not be null");
            var disposableService = provider.GetRequiredService<IDisposableTestService>();

            Assert.IsNotNull(disposableService, "Disposable service should resolve");
            Assert.IsFalse(disposableService.IsDisposed, "Service should not be disposed yet");
            Assert.AreEqual(1, DisposableTestService.GetUndisposedCount(), "Should have 1 undisposed instance");

            // Act: Unload the scene
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return null; // Wait one frame for cleanup to complete

            // Assert: Service should be disposed
            Assert.IsTrue(disposableService.IsDisposed, "Service should be disposed after scene unload");
            Assert.AreEqual(0, DisposableTestService.GetUndisposedCount(), "Should have 0 undisposed instances");
        }

        [UnityTest]
        public IEnumerator MultipleDisposableServices_AreAllDisposedWhenSceneUnloads()
        {
            // Arrange
            DisposableTestService.ResetTracking();
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<ScopedDisposableTestServiceRegister>();
            settings.sceneServiceRegister = new();
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load first scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var sceneA = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(sceneA.isLoaded, "Scene A should be loaded");

            var providerA = UnityHost.Instance.GetSceneServiceProvider(sceneA);
            var serviceA = providerA.GetRequiredService<IDisposableTestService>();

            Assert.IsNotNull(serviceA, "Service A should resolve");
            Assert.AreEqual(1, DisposableTestService.GetUndisposedCount(), "Should have 1 undisposed instance");

            // Load second scene additively
            yield return LoadScene(testScene2Path, LoadSceneMode.Additive);
            var sceneB = SceneManager.GetSceneByPath(testScene2Path);
            Assert.IsTrue(sceneB.isLoaded, "Scene B should be loaded");

            var providerB = UnityHost.Instance.GetSceneServiceProvider(sceneB);
            var serviceB = providerB.GetRequiredService<IDisposableTestService>();

            Assert.IsNotNull(serviceB, "Service B should resolve");
            Assert.AreEqual(2, DisposableTestService.GetUndisposedCount(), "Should have 2 undisposed instances");

            // Act: Unload scene A
            yield return SceneManager.UnloadSceneAsync(sceneA);
            yield return null;

            // Assert: Service A should be disposed
            Assert.IsTrue(serviceA.IsDisposed, "Service A should be disposed");
            Assert.IsFalse(serviceB.IsDisposed, "Service B should still be active");
            Assert.AreEqual(1, DisposableTestService.GetUndisposedCount(), "Should have 1 undisposed instance");

            // Act: Unload scene B
            yield return SceneManager.UnloadSceneAsync(sceneB);
            yield return null;

            // Assert: Service B should also be disposed
            Assert.IsTrue(serviceB.IsDisposed, "Service B should be disposed");
            Assert.AreEqual(0, DisposableTestService.GetUndisposedCount(), "Should have 0 undisposed instances");
        }

        private static void StartHost(MSDISettings settings)
        {
            UnityHostBuilder builder = UnityHost.CreateUnityHostBuilder() as UnityHostBuilder;
            builder.ConfigureServices(services =>
            {
                settings.GlobalServiceRegister.RegisterServices(services);
                services.AddSingleton(sp => new SceneServiceScopeManager(
                    sp,
                    services,
                    settings,
                    sp.GetRequiredService<ILogger<SceneServiceScopeManager>>())
                );
            });
#if MSDI_LOGGING
            builder.AddLogging();
#endif

            UnityHost.Instance = builder.Build();
            UnityHost.Instance.Start();
        }

        [UnityTest]
        public IEnumerator TransientDisposableServices_AreDisposedWhenSceneUnloads()
        {
            // Arrange
            DisposableTestService.ResetTracking();
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<TransientDisposableTestServiceRegister>();
            settings.sceneServiceRegister = new();
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load a scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(scene.isLoaded, "Scene should be loaded");

            var provider = UnityHost.Instance.GetSceneServiceProvider(scene);
            Assert.IsNotNull(provider, "Provider should not be null");

            // Get multiple transient instances
            var disposableService1 = provider.GetRequiredService<IDisposableTestService>();
            var disposableService2 = provider.GetRequiredService<IDisposableTestService>();

            Assert.IsNotNull(disposableService1, "Disposable service 1 should resolve");
            Assert.IsNotNull(disposableService2, "Disposable service 2 should resolve");
            Assert.IsFalse(ReferenceEquals(disposableService1, disposableService2), "Transient services should be different instances");
            Assert.AreEqual(2, DisposableTestService.GetUndisposedCount(), "Should have 2 undisposed instances");

            // Act: Unload the scene
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return null; // Wait one frame for cleanup to complete

            // Assert: Both services should be disposed
            Assert.IsTrue(disposableService1.IsDisposed, "Service 1 should be disposed after scene unload");
            Assert.IsTrue(disposableService2.IsDisposed, "Service 2 should be disposed after scene unload");
            Assert.AreEqual(0, DisposableTestService.GetUndisposedCount(), "Should have 0 undisposed instances");
        }

        [UnityTest]
        public IEnumerator SceneScopedDisposableService_CanBeUsedBeforeDisposal()
        {
            // Arrange
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<SingletonTestService0Register>();
            settings.sceneServiceRegister = new System.Collections.Generic.List<SceneServiceMapping>
            {
                new()
                {
                    scenePath = testScene1Path,
                    serviceRegister = ScriptableObject.CreateInstance<ScopedDisposableTestServiceRegister>()
                }
            };
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load a scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(testScene1Path);
            var provider = UnityHost.Instance.GetSceneServiceProvider(scene);
            var disposableService = provider.GetRequiredService<IDisposableTestService>();

            // Act & Assert: Service should be usable before disposal
            Assert.DoesNotThrow(() => disposableService.DoSomething(), "Service should be usable before disposal");
            Assert.IsFalse(disposableService.IsDisposed, "Service should not be disposed after usage");

            // Unload scene
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return null;

            // Act & Assert: Service should throw after disposal
            var ex = Assert.Throws<ObjectDisposedException>(() => disposableService.DoSomething());
            Assert.IsNotNull(ex, "Should throw ObjectDisposedException when using disposed service");
        }

        [UnityTest]
        public IEnumerator SceneScopedDisposableServices_AreDisposedWhenSceneUnloads()
        {
            // Arrange
            DisposableTestService.ResetTracking();
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<SingletonTestService0Register>();
            settings.sceneServiceRegister = new System.Collections.Generic.List<SceneServiceMapping>
            {
                new()
                {
                    scenePath = testScene1Path,
                    serviceRegister = ScriptableObject.CreateInstance<ScopedDisposableTestServiceRegister>()
                }
            };
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load scene with scoped disposable services
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(testScene1Path);
            Assert.IsTrue(scene.isLoaded, "Scene should be loaded");

            var provider = UnityHost.Instance.GetSceneServiceProvider(scene);
            Assert.IsNotNull(provider, "Provider should not be null");
            var sceneDisposableService = provider.GetRequiredService<IDisposableTestService>();

            Assert.IsNotNull(sceneDisposableService, "Scene-scoped disposable service should resolve");
            Assert.IsFalse(sceneDisposableService.IsDisposed, "Service should not be disposed yet");
            Assert.AreEqual(1, DisposableTestService.GetUndisposedCount(), "Should have 1 undisposed instance");

            // Act: Unload the scene
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return null; // Wait one frame for cleanup to complete

            // Assert: Scene-scoped service should be disposed
            Assert.IsTrue(sceneDisposableService.IsDisposed, "Scene-scoped service should be disposed after scene unload");
            Assert.AreEqual(0, DisposableTestService.GetUndisposedCount(), "Should have 0 undisposed instances");
        }

        private static IEnumerator LoadScene(string scenePath, LoadSceneMode mode)
        {
            var op = SceneManager.LoadSceneAsync(scenePath, mode);
            if (op == null)
            {
                Assert.Fail($"Failed to start loading scene at '{scenePath}'. Make sure the scene path is included in Build Settings.");
                yield break;
            }

            while (!op.isDone)
            {
                yield return null;
            }

            // Wait one frame to ensure scene loaded callbacks executed and MSDI created scene provider
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllDisposableServices_AreDisposedOnHostShutdown()
        {
            // Arrange
            DisposableTestService.ResetTracking();
            MSDISettings settings = ScriptableObject.CreateInstance<MSDISettings>();
            settings.globalServiceRegister = ScriptableObject.CreateInstance<SingletonDisposableTestServiceRegister>();
            settings.sceneServiceRegister = new System.Collections.Generic.List<SceneServiceMapping>
            {
                new()
                {
                    scenePath = testScene1Path,
                    serviceRegister = ScriptableObject.CreateInstance<ScopedDisposableTestServiceRegister>()
                }
            };
            yield return LoadScene(bootstrapScenePath, LoadSceneMode.Single);
            StartHost(settings);

            // Load a scene additively
            yield return LoadScene(testScene1Path, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(testScene1Path);
            var provider = UnityHost.Instance.GetSceneServiceProvider(scene);
            var disposableService1 = provider.GetRequiredService<IDisposableTestService>();

            // Load another scene
            yield return LoadScene(testScene2Path, LoadSceneMode.Additive);
            var scene2 = SceneManager.GetSceneByPath(testScene2Path);
            var provider2 = UnityHost.Instance.GetSceneServiceProvider(scene2);
            var disposableService2 = provider2.GetRequiredService<IDisposableTestService>();

            Assert.AreEqual(2, DisposableTestService.GetUndisposedCount(), "Should have 2 undisposed instances");

            // Act: Dispose the host (simulating application shutdown)
            if (UnityHost.Instance is IAsyncDisposable asyncDisposable)
            {
                // If host supports async disposal
                yield return null;
            }
            else if (UnityHost.Instance is IDisposable hostDisposable)
            {
                hostDisposable.Dispose();
            }

            // Also unload scenes to trigger disposal cleanup
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return SceneManager.UnloadSceneAsync(scene2);
            yield return null;

            // Assert: All services should be disposed
            Assert.IsTrue(disposableService1.IsDisposed, "Service 1 should be disposed after host shutdown");
            Assert.IsTrue(disposableService2.IsDisposed, "Service 2 should be disposed after host shutdown");
            Assert.AreEqual(0, DisposableTestService.GetUndisposedCount(), "Should have 0 undisposed instances after shutdown");
        }
    }

}