using MSDI4Unity.Attributes;
using MSDI4Unity.Host;
using System;
using System.Reflection;
using UnityEngine;

namespace MSDI4Unity.Extensions
{
    public static class MonoBehaviourExtensions
    {
        private const BindingFlags InjectionBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Return IServiceProvider instance that match the current script running scene.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static IServiceProvider GetServiceProvider(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null)
            {
                throw new ArgumentNullException(nameof(monoBehaviour));
            }
            if(UnityHost.Instance == null)
            {
                throw new InvalidOperationException("UnityHost is not initialized. Ensure that MSDI4Unity configuration is set up correctly.");
            }
            return UnityHost.Instance.GetSceneServiceProvider(monoBehaviour.gameObject.scene);
        }

        /// <summary>
        /// Resolves and assigns all fields marked with [Inject]
        /// </summary>
        public static void Inject(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));

            // Get your IServiceProvider using extension method
            IServiceProvider provider = monoBehaviour.GetServiceProvider();
            Type targetType = monoBehaviour.GetType();

            // Inject Fields (including private & backing fields)
            FieldInfo[] fields = targetType.GetFields(InjectionBindingFlags);
            foreach (FieldInfo field in fields)
            {
                if (field.IsDefined(typeof(InjectAttribute), inherit: true))
                {
                    object service = provider.GetService(field.FieldType);
                    if (service != null)
                    {
                        field.SetValue(monoBehaviour, service);
                    }
                }
            }
        }
    }
}