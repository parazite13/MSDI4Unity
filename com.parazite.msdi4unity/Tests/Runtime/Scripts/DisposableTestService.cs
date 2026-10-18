using System;
using System.Collections.Generic;

namespace MSDI4Unity.Tests
{
    public interface IDisposableTestService : IDisposable
    {
        void DoSomething();
        bool IsDisposed { get; }
    }

    public class DisposableTestService : IDisposableTestService
    {
        private bool disposed = false;
        private static List<DisposableTestService> instances = new();

        public bool IsDisposed => disposed;

        public DisposableTestService()
        {
            instances.Add(this);
        }

        public void DoSomething()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(DisposableTestService));
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                instances.Remove(this);
            }
        }

        /// <summary>
        /// Vérifie combien d'instances ne sont pas disposées.
        /// </summary>
        public static int GetUndisposedCount()
        {
            return instances.Count;
        }

        /// <summary>
        /// Réinitialise les instances traquées (pour les tests).
        /// </summary>
        public static void ResetTracking()
        {
            instances.Clear();
        }
    }
}
