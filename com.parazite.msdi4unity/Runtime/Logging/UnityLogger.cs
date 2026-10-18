#if MSDI_LOGGING

using Microsoft.Extensions.Logging;
using System;

namespace MSDI4Unity.Logging
{
    public class UnityLogger : ILogger
    {
        private readonly string category;
        private readonly IExternalScopeProvider scopeProvider;

        public UnityLogger(string category, IExternalScopeProvider scopeProvider)
        {
            this.category = category;
            this.scopeProvider = scopeProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (scopeProvider == null)
                return NullScope.Instance;
            return scopeProvider.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter != null ? formatter(state, exception) : state?.ToString();
            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            if (!string.IsNullOrEmpty(message))
                message = $"[{category}] {message}";

            if (exception != null)
            {
                var exText = exception.ToString();
                message = string.IsNullOrEmpty(message) ? exText : message + "\n" + exText;
            }

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(message);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                default:
                    UnityEngine.Debug.Log(message);
                    break;
            }
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }
    }
}
#endif