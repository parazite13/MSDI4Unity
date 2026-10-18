using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSDI4Unity.Extensions;
using MSDI4Unity.Host;
using NUnit.Framework;
using UnityEngine;

namespace MSDI4Unity.Tests
{
    public class MSDI4UnityEditorTests
    {
        /// <summary>
        /// Basic service resolution
        /// </summary>
        [Test]
        public void TestBasicServiceResolution()
        {
            var builder = new UnityHostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ITestService, TestService>();
            });

            var host = builder.Build();
            var service = host.Services.GetRequiredService<ITestService>();

            Assert.IsNotNull(service, "Service should not be null");
            Assert.IsTrue(service is TestService, "Service should be TestService");
        }

        /// <summary>
        /// Singleton lifetime (same instance)
        /// </summary>
        [Test]
        public void TestSingletonLifetime()
        {
            var builder = new UnityHostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ITestService, TestService>();
            });

            var host = builder.Build();
            var service1 = host.Services.GetRequiredService<ITestService>();
            var service2 = host.Services.GetRequiredService<ITestService>();

            Assert.IsTrue(ReferenceEquals(service1, service2), "Singleton instances should be the same");
        }

        /// <summary>
        /// Transient lifetime (different instances)
        /// </summary>
        [Test]
        public void TestTransientLifetime()
        {
            var builder = new UnityHostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddTransient<ITestService, TestService>();
            });

            var host = builder.Build();
            var service1 = host.Services.GetRequiredService<ITestService>();
            var service2 = host.Services.GetRequiredService<ITestService>();

            Assert.IsFalse(ReferenceEquals(service1, service2), "Transient instances should be different");
        }

        /// <summary>
        /// Scoped lifetime
        /// </summary>
        [Test]
        public void TestScopedLifetime()
        {
            var builder = new UnityHostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ITestService, TestService>();
            });

            var host = builder.Build();
            var scope1 = host.Services.CreateScope();
            var scope2 = host.Services.CreateScope();

            var service1a = scope1.ServiceProvider.GetRequiredService<ITestService>();
            var service1b = scope1.ServiceProvider.GetRequiredService<ITestService>();
            var service2 = scope2.ServiceProvider.GetRequiredService<ITestService>();

            Assert.IsTrue(ReferenceEquals(service1a, service1b), "Services in same scope should be same");
            Assert.IsFalse(ReferenceEquals(service1a, service2), "Services in different scopes should be different");
        }

        /// <summary>
        /// Dependency injection between services
        /// </summary>
        [Test]
        public void TestDependencyInjection()
        {
            var builder = new UnityHostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ITestDependency, TestDependency>();
                services.AddSingleton<ITestServiceWithDependency, TestServiceWithDependency>();
            });

            var host = builder.Build();
            var service = host.Services.GetRequiredService<ITestServiceWithDependency>();

            Assert.IsNotNull(service.Dependency, "Dependency should be injected");
            Assert.IsTrue(service.Dependency is TestDependency, "Dependency should be TestDependency");
        }

#if MSDI_LOGGING
        /// <summary>
        /// Unity logging integration
        /// </summary>
        [Test]
        public void TestUnityLogging()
        {
            var builder = new UnityHostBuilder();
            builder.AddLogging();

            var host = builder.Build();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            Assert.IsNotNull(loggerFactory, "LoggerFactory should not be null");

            var logger = loggerFactory.CreateLogger("TestLogger");
            Assert.IsNotNull(logger, "Logger should not be null");

            // This should output to Unity console
            var success = false;
            void LogCallback(string message, string stackTrace, LogType type)
            {
                if (message.Contains("This is a test log message from MSDI4Unity"))
                {
                    success = true;
                }
            }
            Application.logMessageReceived += LogCallback;
            logger.LogInformation("This is a test log message from MSDI4Unity");
            Assert.IsTrue(success, "Expected log message not found");
            Application.logMessageReceived -= LogCallback;
        }
#endif

        /// <summary>
        /// Full host builder test
        /// </summary>
        [Test]
        public void TestHostBuilder()
        {
            var builder = new UnityHostBuilder();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ITestService, TestService>();
                services.AddTransient<ITestDependency, TestDependency>();
            });

            var host = builder.Build();

            Assert.IsNotNull(host, "Host should not be null");
            Assert.IsNotNull(host.Services, "Host services should not be null");

            var service = host.Services.GetRequiredService<ITestService>();
            Assert.IsNotNull(service, "Service should be resolvable");

            host.Dispose();
        }
    }

    // ============ Test Interfaces and Implementations ============

    public interface ITestService
    {
        string GetTestValue();
    }

    public class TestService : ITestService
    {
        public TestService()
        {
            Debug.Log("[DI Tests] TestService created");
        }

        public string GetTestValue() => "Test Value";
    }

    public interface ITestDependency
    {
        string GetDependencyValue();
    }

    public class TestDependency : ITestDependency
    {
        public TestDependency()
        {
            Debug.Log("[DI Tests] TestDependency created");
        }

        public string GetDependencyValue() => "Dependency Value";
    }

    public interface ITestServiceWithDependency
    {
        ITestDependency Dependency { get; }
    }

    public class TestServiceWithDependency : ITestServiceWithDependency
    {
        public ITestDependency Dependency { get; }

        public TestServiceWithDependency(ITestDependency dependency)
        {
            Dependency = dependency;
            Debug.Log("[DI Tests] TestServiceWithDependency created with injected dependency");
        }
    }
}
