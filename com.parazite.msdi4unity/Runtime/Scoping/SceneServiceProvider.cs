using Microsoft.Extensions.DependencyInjection;
using System;

namespace MSDI4Unity.Scoping
{
    internal class SceneServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceProvider rootProvider;
        private readonly IServiceProvider sceneProvider;

        internal SceneServiceProvider(
            IServiceProvider rootProvider,
            IServiceCollection rootServicesDescriptors,
            Action<IServiceCollection> sceneServiceRegister)
        {
            this.rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));

            IServiceCollection sceneServiceCollection = new ServiceCollection();

            // Register IServiceProvider as the current class
            sceneServiceCollection.AddSingleton<IServiceProvider>(this);

            // Using root services to create new scope that can resolve service from the project service provider
            if (rootServicesDescriptors != null)
            {
                foreach (var descriptor in rootServicesDescriptors)
                {
                    if (descriptor.ServiceType == typeof(IServiceProvider)) continue;

                    // Open generic type can be added as is
                    if (descriptor.ServiceType.IsGenericTypeDefinition)
                    {
                        sceneServiceCollection.Add(descriptor);
                    }
                    // Other types
                    else
                    {
                        if (descriptor.Lifetime == ServiceLifetime.Singleton)
                        {
                            // For singleton service we resolve the instance from the root provider and register it in the scene provider
                            var rootInstance = rootProvider.GetService(descriptor.ServiceType);
                            if (rootInstance != null)
                            {
                                sceneServiceCollection.AddSingleton(descriptor.ServiceType, rootInstance);
                            }
                        }
                        else
                        {
                            // For scoped and transient services we can register the descriptor as is
                            sceneServiceCollection.Add(descriptor);
                        }
                    }
                }
            }

            // Invoke the scene-specific service registration
            sceneServiceRegister?.Invoke(sceneServiceCollection);

            sceneProvider = sceneServiceCollection.BuildServiceProvider();
        }

        public object GetService(Type serviceType)
        {
            // Trying to resolve with scene services first
            var service = sceneProvider?.GetService(serviceType);
            if (service != null)
            {
                return service;
            }

            // Fallback with root services
            service = rootProvider.GetService(serviceType);
            if(service != null)
            {
                return service;
            }

            throw new InvalidOperationException($"Service of type {serviceType.FullName} is not registered in either scene or root service provider.");
        }

        public void Dispose()
        {
            if (sceneProvider is IDisposable disposableScene)
            {
                disposableScene.Dispose();
            }
        }
    }
}