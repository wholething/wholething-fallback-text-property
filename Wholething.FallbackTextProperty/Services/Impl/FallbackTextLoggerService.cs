using System;
#if NET5_0_OR_GREATER
using Microsoft.Extensions.Logging;
#else
using Umbraco.Core.Logging;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class FallbackTextLoggerService : IFallbackTextLoggerService
    {
#if NET5_0_OR_GREATER
        private readonly ILogger<FallbackTextLoggerService> _logger;

        public FallbackTextLoggerService(ILogger<FallbackTextLoggerService> logger)
        {
            _logger = logger;
        }
#else
        private readonly ILogger _logger;
        public FallbackTextLoggerService(ILogger logger)
        {
            _logger = logger;
        }
#endif

        public void LogWarning(Exception exception, string message, params object[] args)
        {
#if NET5_0_OR_GREATER
            _logger.LogWarning(exception, message, args);
#else
            _logger.Warn(typeof(FallbackTextLoggerService), exception, message, args);
#endif
        }

        public void LogWarning(string message, params object[] args)
        {
#if NET5_0_OR_GREATER
            _logger.LogWarning(message, args);
#else
            _logger.Warn(typeof(FallbackTextLoggerService), message, args);
#endif
        }
    }
}
