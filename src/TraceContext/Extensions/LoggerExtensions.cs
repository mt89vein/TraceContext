using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace TraceContext.Extensions
{
    /// <summary>
    /// Методы расширения для <see cref="ILogger"/>.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// LoggingScope с текущим TraceId и TraceIdSource.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <returns>Disposable logging scope.</returns>
        public static IDisposable WithTraceContext(this ILogger logger)
        {
            return logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = TraceContext.Current.TraceId!,
                ["TraceIdSource"] = TraceContext.Current.TraceIdSource!
            });
        }
    }
}
