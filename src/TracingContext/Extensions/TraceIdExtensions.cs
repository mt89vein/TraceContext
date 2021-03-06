using Microsoft.AspNetCore.Builder;
using TracingContext.Middlewares;

namespace TracingContext.Extensions
{
    /// <summary>
    /// Методы расширения для <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class TraceIdExtensions
    {
        /// <summary>
        /// Добавляет TraceId в запросы.
        /// </summary>
        /// <param name="app">Конфигуратор приложения.</param>
        /// <returns>Конфигуратор приложения.</returns>
        public static IApplicationBuilder UseTraceId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TraceIdMiddleware>();
        }
    }
}
