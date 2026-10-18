#if MSDI_LOGGING

using Microsoft.Extensions.Logging;

namespace MSDI4Unity.Logging
{
    /// <summary>
    /// ILoggerProvider implementation that routes logs to UnityEngine.Debug.
    /// Compatible with Microsoft.Extensions.Logging so it can be used with ILoggerFactory.
    /// </summary>
    public class UnityLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider scopeProvider;

        public ILogger CreateLogger(string categoryName)
        {
            return new UnityLogger(categoryName, scopeProvider);
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            this.scopeProvider = scopeProvider;
        }
    }
}
#endif