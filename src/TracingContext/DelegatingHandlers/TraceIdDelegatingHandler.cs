using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TracingContext.Settings;

namespace TracingContext.DelegatingHandlers
{
    /// <summary>
    /// Обработчик запроса, который прокидывает TraceId в заголовок запроса.
    /// </summary>
    internal class TraceIdDelegatingHandler : DelegatingHandler
    {
        #region Поля

        /// <summary>
        /// Настройки сквозного идентификатора.
        /// </summary>
        private readonly TraceIdSettings _settings;

        #endregion Поля

        #region Конструкторы

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="settings">Настройки сквозного идентификатора.</param>
        public TraceIdDelegatingHandler(IOptions<TraceIdSettings> settings)
        {
            _settings = settings.Value;
        }

        #endregion Конструкторы

        #region Методы (protected)

        /// <summary>
        /// Метод выполнения запроса.
        /// </summary>
        /// <param name="request">Запрос.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Результат запроса.</returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (!request.Headers.Contains(_settings.Header))
            {
                request.Headers.Add(
                    _settings.Header,
                    TraceContext.Current.TraceId?.ToString()
                );
                request.Headers.Add(
                    _settings.TraceIdSourceHeader,
                    TraceContext.Current.TraceIdSource
                );
            }

            return base.SendAsync(request, cancellationToken);
        }

        #endregion Методы (protected)
    }
}