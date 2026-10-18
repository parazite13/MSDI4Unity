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