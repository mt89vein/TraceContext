using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;
using TracingContext.Extensions;
using TracingContext.Settings;

namespace TracingContext.Middlewares
{
    /// <summary>
    /// Middleware, которая пытается прочитать/установить TraceId, которая может быть использована в
    /// логах и передана дальше в зависимые запросы.
    /// </summary>
    public class TraceIdMiddleware
    {
        #region Поля

        /// <summary>
        /// Следующая Middleware.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Настройки TraceId
        /// </summary>
        private readonly TraceIdSettings _settings;

        #endregion Поля

        #region Конструкторы

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="next">Следующая Middleware в конвейере.</param>
        /// <param name="settings">Настройки TraceId.</param>
        public TraceIdMiddleware(RequestDelegate next, IOptions<TraceIdSettings> settings)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        #endregion Конструкторы

        #region Методы (public)

        /// <summary>
        /// Обрабатывает запрос для установки/чтения TraceId в заголовках и устанавливает в <see cref="TraceContext"/>
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> для текущего запроса.</param>
        /// <param name="logger">Логгер.</param>
        #pragma warning disable IDE1006
        public async Task Invoke(HttpContext context, ILogger<TraceIdMiddleware> logger)
        #pragma warning restore IDE1006
        {
            var (traceId, traceIdSource) = GetTraceIdFromHeaders(context, logger);

            if (!traceId.HasValue)
            {
                if (_settings.ThrowBadRequestIfNotPresent)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync($"{_settings.Header} должен быть установлен.");

                    return;
                }

                TraceContext.Create(null, traceIdSource);
                await _next(context);

                return;
            }

            context.Response.OnStarting(() =>
            {
                UpdateHeaders(context.Response.Headers, _settings.Header, traceId?.ToString());
                UpdateHeaders(context.Response.Headers, _settings.TraceIdSourceHeader, traceIdSource);

                return Task.CompletedTask;
            });

            if (_settings.UseLoggerScope)
            {
                TraceContext.Create(traceId, traceIdSource);
                using (logger.WithTraceContext())
                {
                    await _next(context);
                }
            }
            else
            {
                TraceContext.Create(traceId, traceIdSource);
                await _next(context);
            }
        }

        #endregion Методы (public)

        #region Методы (private)

        /// <summary>
        /// Обноыляет заголовки.
        /// </summary>
        /// <param name="headers">Заголовки.</param>
        /// <param name="headerName">Имя заголовка.</param>
        /// <param name="headerValue">Значение.</param>
        private static void UpdateHeaders(IHeaderDictionary headers, string headerName, string? headerValue)
        {
            if (string.IsNullOrEmpty(headerValue))
            {
                return;
            }

            if (headers.ContainsKey(headerName))
            {
                headers.Remove(headerName);
            }
            headers.Add(headerName, headerValue);
        }

        /// <summary>
        /// Получить сквозной идентификатор.
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> для текущего запроса.</param>
        /// <param name="logger">Логгер.</param>
        /// <returns>Сквозной идентификатор.</returns>
        private (Guid? TraceId, string? TraceIdSource) GetTraceIdFromHeaders(HttpContext context, ILogger logger)
        {
            var logMessage = string.Empty;
            if (TryParseTraceIdFromHeaders(context, out var traceId, out var traceIdSource))
            {
                if (string.IsNullOrEmpty(traceIdSource))
                {
                    traceIdSource = "FromHttpHeader";
                }
            }
            else
            {
                logMessage += $"TraceId не указан в заголовке запроса {_settings.Header}.";
            }


            // Если не удалось получить из заголовков запроса TraceId и распарсить его в Guid и при
            // этом нужно сгенерировать новый
            if (!traceId.HasValue && _settings.GenerateIfNotPresent)
            {
                traceId = Guid.NewGuid();
                logMessage += $" Сгенерирован новый: {traceId}";
                traceIdSource = "HttpGenerated";
            }

            if (_settings.LogIfTraceIdGenerated && logger.IsEnabled(LogLevel.Debug) && !string.IsNullOrEmpty(logMessage))
            {
                logger.LogDebug(logMessage);
            }

            return (traceId, traceIdSource);
        }

        /// <summary>
        /// Пытается получить из заголовков запроса TraceId и конвертировать в <see cref="Guid"/>?.
        /// </summary>
        /// <param name="context">HTTP контекст.</param>
        /// <param name="traceId">Сквозной идентификатор.</param>
        /// <param name="traceSource">Источник сквозного идентификатора.</param>
        private bool TryParseTraceIdFromHeaders(HttpContext context, out Guid? traceId, out string? traceSource)
        {
            traceId = null;
            traceSource = null;

            var headers = context.Request.Headers;

            // проверяем есть ли TraceId
            if (headers.TryGetValue(_settings.Header, out var traceIdString) &&
                Guid.TryParse(traceIdString, out var traceGuid)  &&
                traceGuid != Guid.Empty)
            {
                traceId = traceGuid;
                // если есть TraceId, то пытаемся достать его исчточник
                if (headers.TryGetValue(_settings.TraceIdSourceHeader, out var traceIdSourceString))
                {
                    traceSource = traceIdSourceString;
                }

                return true;
            }

            return false;
        }

        #endregion Методы (private)
    }
}