using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

namespace MSDI4Unity.Host
{
    /// <summary>
    /// Listens to Unity's environment shutdown events (Play Mode exit / App Quit)
    /// and signals the IHostApplicationLifetime accordingly.
    /// </summary>
    public class UnityHostLifetime : IHostLifetime
    {
        private readonly IHostApplicationLifetime hostApplicationLifetime;

        public UnityHostLifetime(IHostApplicationLifetime hostApplicationLifetime)
        {
            this.hostApplicationLifetime = hostApplicationLifetime ??
                throw new ArgumentNullException(nameof(hostApplicationLifetime));
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            Application.quitting += OnApplicationQuit;
#endif
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#else
            Application.quitting -= OnApplicationQuit;
#endif
            return Task.CompletedTask;
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Trigger shutdown ONLY when exiting Play Mode
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                hostApplicationLifetime.StopApplication();
            }
        }
#else
        private void OnApplicationQuit()
        {
            hostApplicationLifetime.StopApplication();
        }
#endif
    }
}