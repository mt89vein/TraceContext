using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using TracingContext.Middlewares;
using TracingContext.Settings;

namespace TracingContext.Test
{
    [TestFixture]
    public class TraceIdMiddlewareTests
    {
        #region Тесты

        [Test, Description("Тестирование корректности обработки запроса в TraceIdMiddleware.")]
        [TestCase(0, HttpStatusCode.OK, "54FEB2AD-239D-46D7-A02C-46C68222D325", true, true, true)]
        [TestCase(1, HttpStatusCode.OK, "54FEB2AD-239D-46D7-A02C-46C68222D325", true, false, true)]
        [TestCase(2, HttpStatusCode.OK, "54FEB2AD-239D-46D7-A02C-46C68222D325", false, true, true)]
        [TestCase(3, HttpStatusCode.OK, "54FEB2AD-239D-46D7-A02C-46C68222D325", false, false, true)]

        [TestCase(4, HttpStatusCode.BadRequest, "00000000-0000-0000-0000-000000000000", false, true, false)]
        [TestCase(5, HttpStatusCode.OK, "00000000-0000-0000-0000-000000000000", false, false, false)]
        [TestCase(6, HttpStatusCode.OK, "00000000-0000-0000-0000-000000000000", true, false, false)]
        [TestCase(7, HttpStatusCode.OK, "00000000-0000-0000-0000-000000000000", true, true, false)]
        public async Task CorrectHandleSettingsAsync(
            int caseNumber,
            HttpStatusCode expectedStatusCode,
            string initialTraceString,
            bool generateIfNotPresent,
            bool throwBadRequestIfNotPresent,
            bool initialAndFinalTraceIdMustBeEqual
        )
        {
            #region Arrange

            var initialTraceId = Guid.Parse(initialTraceString);
            var traceIdSettings = new TraceIdSettings
            {
                GenerateIfNotPresent = generateIfNotPresent,
                ThrowBadRequestIfNotPresent = throwBadRequestIfNotPresent
            };
            var traceContext1 = TraceContext.Create(initialTraceId);

            var context = GetMockedContextFor(traceIdSettings, out var feature);
            var traceIdMiddleware =
                new TraceIdMiddleware(_ => feature.CompleteAsync(), Options.Create(traceIdSettings));

            #endregion Arrange

            #region Act

            await traceIdMiddleware.Invoke(context, NullLogger<TraceIdMiddleware>.Instance);

            #endregion Act

            #region Assert

            var traceContext2 = TraceContext.Current;

            var actualTraceId = GetActualTraceId(context, traceIdSettings);

            Assert.Multiple(() =>
            {
                Assert.AreEqual((int) expectedStatusCode, context.Response.StatusCode,
                    "status code должен быть ожидаемым.");

                if (initialAndFinalTraceIdMustBeEqual)
                {
                    Assert.AreEqual(initialTraceId, actualTraceId ?? Guid.Empty, "traceId до и после выполнения должны быть одинаковыми.");
                }

                if (traceIdSettings.GenerateIfNotPresent)
                {
                    Assert.IsTrue(ReferenceEquals(traceContext1, traceContext2), "Инстансы должны быть одним и тем же.");
                    Assert.AreNotEqual(Guid.Empty, TraceContext.Current.TraceId,
                        "Если требуется генерировать при отсутствии TraceId, то TraceId не должен быть пустым.");

                    if (actualTraceId.HasValue)
                    {
                        Assert.IsTrue(actualTraceId != Guid.Empty);
                    }
                }
                else
                {
                    if (initialTraceId == Guid.Empty)
                    {
                        Assert.IsNull(actualTraceId);
                    }
                    else
                    {
                        Assert.AreEqual(initialTraceId, actualTraceId);
                    }
                }
            });

            #endregion Assert
        }

        #endregion Тесты

        #region Moq

        /// <summary>
        /// Мок HTTP контекста.
        /// </summary>
        /// <param name="traceIdSettings">Настройки TraceId.</param>
        /// <returns>Мок HTTP контекста.</returns>
        private static DefaultHttpContext GetMockedContextFor(
            TraceIdSettings traceIdSettings,
            out MockResponseFeature feature
        )
        {
            feature = new MockResponseFeature();
            var context = new DefaultHttpContext();
            context.Features.Set<IHttpResponseFeature>(feature);
            if (!string.IsNullOrWhiteSpace(traceIdSettings.Header))
            {
                context.Request.Headers.Add(traceIdSettings.Header, TraceContext.Current.TraceId?.ToString());
            }

            if (!string.IsNullOrWhiteSpace(traceIdSettings.TraceIdSourceHeader))
            {
                context.Request.Headers.Add(traceIdSettings.TraceIdSourceHeader, TraceContext.Current.TraceIdSource);
            }

            return context;
        }

        /// <summary>
        /// Пытается получить из заголовков запроса TraceId и конвертировать в <see cref="Guid"/>?.
        /// </summary>
        /// <param name="context">HTTP контекст.</param>
        /// <param name="settings">Настройки TraceId.</param>
        /// <returns>TraceId.</returns>
        private static Guid? GetActualTraceId(HttpContext context, TraceIdSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Header))
            {
                return null;
            }

            Guid? traceId = null;

            if (context.Response.Headers.TryGetValue(settings.Header, out var traceIdString) &&
                Guid.TryParse(traceIdString, out var traceGuid))
            {
                traceId = traceGuid;
            }

            return traceId;
        }

        private class MockResponseFeature : IHttpResponseFeature
        {
            #region Поля

            private Func<object, Task>? _callback;
            private object? _state;

            #endregion Поля

            #region Свойства

            public Stream Body { get; set; } = Stream.Null;

            public bool HasStarted { get; private set; }

            public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

            public string? ReasonPhrase { get; set; }

            public int StatusCode { get; set; } = (int) HttpStatusCode.OK;

            #endregion Свойства

            public void OnCompleted(Func<object, Task> callback, object state)
            {
                //...No-op
            }

            public void OnStarting(Func<object, Task> callback, object state)
            {
                _callback = callback;
                _state = state;
            }

            public Task CompleteAsync()
            {
                HasStarted = true;

                if (_callback == null || _state == null)
                {
                    return Task.CompletedTask;
                }

                return _callback(_state);
            }
        }

        #endregion Moq
    }
}