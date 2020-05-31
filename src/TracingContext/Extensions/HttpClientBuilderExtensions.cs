using Microsoft.Extensions.DependencyInjection;
using TracingContext.DelegatingHandlers;

namespace TracingContext.Extensions
{
    /// <summary>
    /// Методы расширения для <see cref="IHttpClientBuilder"/>.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Добавить текущий TraceId в исходящие запросы этого HTTP клиента..
        /// </summary>
        /// <param name="httpClientBuilder">Конфигуратор HTTP клиента.</param>
        /// <returns>Конфигуратор HTTP клиента.</returns>
        public static IHttpClientBuilder AddTracing(this IHttpClientBuilder httpClientBuilder)
        {
            httpClientBuilder.Services.AddTransient<TraceIdDelegatingHandler>();

            return httpClientBuilder.AddHttpMessageHandler(
                sp => sp.GetRequiredService<TraceIdDelegatingHandler>()
            );
        }
    }
}
