# MSDI4Unity

This package allow Unity host project to work seamlessly with `Microsoft.Extensions.DependencyInjection` also known as MSDI.

## Installation

- Add the following line in your manifest.json
```
"com.parazite.msdi4unity": "https://github.com/parazite13/MSDI4Unity.git?path=/com.parazite.msdi4unity"
```

As this package relies on some `Microsoft.Extensions.*` packages, two methods can be used to pull dependencies :

### With OpenUPM
- Add OpenUPM scoped registry in your manifest.json to retrieve dependent packages nuget equivalent:
  - org.nuget.microsoft.extensions.dependencyinjection
  - org.nuget.microsoft.extensions.hosting.abstractions
  - *(optionnal)* org.nuget.microsoft.extensions.logging
  - *(optionnal)* org.nuget.microsoft.extensions.configuration
```
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "org.nuget"
      ]
    }
  ],
  "dependencies": {
    "org.nuget.microsoft.extensions.dependencyinjection": "10.0.0",
    "org.nuget.microsoft.extensions.hosting.abstractions": "10.0.0",
    ...
  }
}
```

### With Nuget4Unity

- Setup [Nuget4Unity](https://github.com/GlitchEnzo/NuGetForUnity)
- Add the following dependencies:
  - Microsoft.Extensions.DependencyInjection
  - Microsoft.Extensions.Hosting.Abstractions
  - *(optionnal)* Microsoft.Extensions.Logging
  - *(optionnal)* Microsoft.Extensions.Configuration
```
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Microsoft.Extensions.DependencyInjection" version="10.0.0" manuallyInstalled="true" />
  <package id="Microsoft.Extensions.Hosting.Abstractions" version="10.0.0" manuallyInstalled="true" />
</packages>
```

## Getting Started

- Create your project service collection script :
```csharp
[CreateAssetMenu]
public class ProjectServiceRegister : ServiceRegisterScriptableObject
{
    public override void RegisterServices(IServiceCollection services)
    {
        // Use services to register as many services as you want here
    }
}
```
- Create the associated scriptableObject asset : Assets > Create > ProjectServiceRegister
- Open Edit > Project Settings
- Select MSDI4Unity tab
- Drag and drop your previously created project service scriptable object in the GlobalServiceRegister
- Retrieve the services from your script using :

```csharp
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

    public interface IHelloWorldService
    {
        public void SayHello();
    }

    public class HelloWorldService : IHelloWorldService
    {
        public void SayHello()
        {
            Debug.Log("Hello World!");
        }
    }
}
```

## Inject services

Services can be injected in MonoBehaviour fields marked with `[Inject]` attribute. When it is done a call to the extensions method `Inject()` will automatically fill all the fields with concrete instance retrieved from the service provider.

```csharp
using MSDI4Unity.Attributes;
using MSDI4Unity.Extensions;
using MSDI4Unity.Sample.Services;
using UnityEngine;

namespace MSDI4Unity.Sample.Behaviours
{
    public class AttributeInjectBehaviour : MonoBehaviour
    {
        // Retrieve services from InjectAttribute
        [Inject]
        private readonly IHelloWorldService helloWorldService;

        private void Awake()
        {
            // Fill the fields with services retrieved from service provider
            this.Inject();

            // Use your services as you want
            helloWorldService.SayHello();
        }
    }
}
```

To retrieve services from `IServiceProvider` when running outside `MonoBehaviour`, you can use the `IUnityHost` interface and the static reference from `UnityHost` that expose both the global project service provider and a method to retrieve Scene scoped based service provider with respectively:
```csharp
public void Foo()
{
    IServiceProvider rootServiceProvider = UnityHost.Instance.Services; // Global project scope IServiceProvider
    IServiceProvider sceneServiceProvider = UnityHost.Instance.GetSceneServiceProvider(scene); // Scene scope IServiceProvider child of global project scope
}
```

## Logging

Logging support integrates directly with standard `Microsoft.Extensions.Logging` abstractions. 

> [!NOTE]
> Logging functionality is **optional**. It is automatically enabled when the `Microsoft.Extensions.Logging` assembly is detected in your project.
> The package can be optionnaly installed from OpenUPM `org.nuget.microsoft.extensions.logging` or Nuget `Microsoft.Extensions.Logging`

When the logging package is present, the framework automatically defines the `MSDI_LOGGING` symbol, enabling extension methods for configuring loggers on the host builder.

If enabled the global MSDISettings will have new slot for a `LoggingRegisterScriptableObject` that should be set. 
A default implementation is included by default for a simple forward to Unity's `Debug.LogXXX()` methods: `Runtime\ScriptableObjects\DefaultLoggingRegister.asset`.
For more advanced use case, any implementation of `LoggingRegisterScriptableObject` can be used to override `ILoggingBuilder`

```csharp
#if MSDI_LOGGING

using Microsoft.Extensions.Logging;
using UnityEngine;

namespace MSDI4Unity.Logging
{
    [CreateAssetMenu(fileName = "DefaultLoggingRegister", menuName = "MSDI/Logging/DefaultLoggingRegister")]
    public class DefaultLoggingRegisterScriptableObject : LoggingRegisterScriptableObject
    {
        public override void RegisterLogging(ILoggingBuilder builder)
        {
            builder.AddProvider(new UnityLoggerProvider());
        }
    }
}
#endif
```

When enabled `ILoggerFactory` is registered in DI and `ILogger` instance can be injected automatically like in standard dotnet.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSDI4Unity.Attributes;
using MSDI4Unity.Extensions;
using MSDI4Unity.Sample.Services;
using System;
using UnityEngine;

namespace MSDI4Unity.Sample.Behaviours
{
    public class LoggingBehaviour : MonoBehaviour
    {
#if MSDI_LOGGING
        [Inject]
        private readonly ILogger<LoggingBehaviour> logger;

        [Inject]
        private readonly ILoggerFactory loggerFactory;

        private void Awake()
        {
            this.Inject();

            logger.LogInformation("LoggingBehaviour Awake called.");
        }

        private void Start()
        {
            var anotherLogger = loggerFactory.CreateLogger("AnotherLogger");
            anotherLogger.LogInformation("LoggingBehaviour Start called.");
        }
#endif
    }
}
```

## Configuration

Configuration support integrates `Microsoft.Extensions.Configuration` into the Unity host lifecycle.

> [!NOTE]
> Configuration functionality is optional. It requires the `Microsoft.Extensions.Configuration` assembly to be present in the project.
> The package can be optionnaly installed from OpenUPM `org.nuget.microsoft.extensions.configuration` or Nuget `Microsoft.Extensions.Configuration`

When the configuration package is present, the framework automatically defines the `MSDI_CONFIGURATION` symbol, enabling extension methods for adding configuration on the host builder.

If enabled the global MSDISettings will have new slot for a `ConfigurationRegisterScriptableObject` that should be set. 
A default implementation is included by default for a simple key/value pair configuration asset: `Runtime\ScriptableObjects\DefaultConfigurationRegister.asset`.
For more advanced use case, any implementation of `ConfigurationRegisterScriptableObject` can be used to override `IConfigurationBuilder`

```csharp
#if MSDI_CONFIGURATION

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSDI4Unity.Configuration
{
    [CreateAssetMenu(fileName = "DefaultConfigurationRegister", menuName = "MSDI4Unity/Configuration/Default Configuration Register")]
    public class DefaultConfigurationRegisterScriptableObject : ConfigurationRegisterScriptableObject
    {
        [Serializable]
        public struct ConfigEntry
        {
            [Tooltip("Configuration Key (e.g. Api:Url or TargetFrameRate)")]
            public string Key;

            [Tooltip("Configuration Value")]
            public string Value;
        }

        [SerializeField]
        private List<ConfigEntry> entries = new();

        public IReadOnlyList<ConfigEntry> Entries => entries;

        public override void RegisterConfiguration(IConfigurationBuilder builder)
        {
            builder.AddInMemoryCollection(entries.ToDictionary(entry => entry.Key, entry => entry.Value));
        }
    }
}
#endif
```

> [!NOTE]
> The default delimiter for section in `Microsoft.Extensions.Configuration` package is ':'

Values from the IConfiguration can then be retrieved with default IConfiguration injection :

```csharp
using Microsoft.Extensions.Configuration;
using MSDI4Unity.Attributes;
using MSDI4Unity.Extensions;
using UnityEngine;

namespace MSDI4Unity.Sample.Behaviours
{
    public class ConfigurationInjectBehaviour : MonoBehaviour
    {
#if MSDI_CONFIGURATION
        [Inject]
        private readonly IConfiguration configuration;

        private void Awake()
        {
            this.Inject();
            foreach (var kvp in configuration.AsEnumerable())
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}");
            }
        }
#endif
    }
}
```

## Lifetime

In MSDI4Unity, service lifecycles are bound to scene scopes and the global project host lifecycle. Each loaded scene receives its own child service provider (`IServiceProvider`) created from the global project host container.

### Registration Scopes & Behaviour

| Service Registration | Behavior Across Scene Scopes | Lifecycle / Disposal Event |
| :--- | :--- | :--- |
| **Project Singleton** (`AddSingleton`) | **Shared instance** across all scenes. Resolving this service from different scene providers returns the identical instance (`ReferenceEquals`). | Disposed when the host shuts down or the application stops. |
| **Project Scoped** (`AddScoped`) | **Per-scene instance**. Each scene provider resolves its own unique instance across scene scopes. | Disposed automatically when its host scene is unloaded. |
| **Project Transient** (`AddTransient`) | **New instance per resolution**. Created fresh every time `GetService` or `GetRequiredService` is called across or within scene providers. | Disposed when the host scene scope is unloaded. |
| **Scene Specific** (`SceneServiceMapping`) | **Isolated per scene**. Registered specifically to individual scenes via `sceneServiceRegister`. The service resolves only within its mapped scene provider and resolves to `null` in unmapped scenes. | Disposed automatically when the corresponding scene is unloaded. |

### Disposal Mechanics

- **Automatic Scene Cleanup**: Any service implementing `IDisposable` (whether registered as Scoped, Transient, or Scene-Specific) that is resolved within a scene scope is tracked and automatically disposed (`Dispose()`) when that scene unloads (`SceneManager.UnloadSceneAsync`).
- **Host Shutdown**: Global Singleton services implementing `IDisposable` are cleaned up when the main `UnityHost` shuts down or is disposed.
