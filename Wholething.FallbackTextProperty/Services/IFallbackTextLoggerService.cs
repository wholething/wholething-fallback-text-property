using System;

namespace Wholething.FallbackTextProperty.Services
{
    public interface IFallbackTextLoggerService
    {
        void LogWarning(Exception exception, string message, params object[] args);
        void LogWarning(string message, params object[] args);
    }
}
