using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TraceContext.Settings;

namespace TraceContext.Extensions
{
    /// <summary>
    /// Методы расширения для <see cref="IServiceCollection"/>.
    /// </summary>
    public static class TraceIdServiceExtensions
    {
        /// <summary>
        /// Добавляет сервисы, нужные для функционирования механизма TraceId.
        /// </summary>
        /// <param name="services">Регистратор служб.</param>
        /// <param name="configureAction">Лямбда для настройки TraceId.</param>
        public static void AddTraceId(
            this IServiceCollection services,
            Action<IServiceProvider, TraceIdSettings>? configureAction = null
        )
        {
            services.AddOptions<TraceIdSettings>()
                .Configure<IServiceProvider, IConfiguration>((settings, sp, configuration) =>
                {
                    var section = configuration.GetSection(nameof(TraceIdSettings));
                    if (section.Exists())
                    {
                        section.Bind(settings);
                    }

                    configureAction?.Invoke(sp, settings);

                    if (string.IsNullOrWhiteSpace(settings.Header))
                    {
                        throw new ArgumentException("Название заголовка для передаче TraceId не может быть пустым",
                            nameof(settings.Header));
                    }

                    if (string.IsNullOrWhiteSpace(settings.TraceIdSourceHeader))
                    {
                        throw new ArgumentException("Название заголовка источника TraceId не может быть пустым",
                            nameof(settings.TraceIdSourceHeader));
                    }
                });
        }
    }
}
