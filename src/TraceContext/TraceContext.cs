using System;
using System.Threading;

namespace TraceContext
{
    /// <summary>
    /// Контекст трейсинга.
    /// </summary>
    public class TraceContext
    {
        #region Поля

        /// <summary>
        /// Объект блокировки.
        /// </summary>
        private static readonly object Lock = new object();

        /// <summary>
        /// Текущий контекст трейсинга.
        /// </summary>
        private static readonly AsyncLocal<TraceContext> CurrentTraceContext = new AsyncLocal<TraceContext>();

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Сквозной идентификатор.
        /// </summary>
        public Guid? TraceId { get; private set; }

        /// <summary>
        /// Источник TraceId.
        /// </summary>
        public string? TraceIdSource { get; }

        /// <summary>
        /// Возвращает текущий контекст трейсинга.
        /// </summary>
        public static TraceContext Current
        {
            get
            {
                lock (Lock)
                {
                    var tracingContext = CurrentTraceContext.Value;
                    if (tracingContext == null)
                    {
                        CurrentTraceContext.Value = tracingContext = Create();
                    }
                    return tracingContext;
                }
            }
        }

        #endregion Свойства

        #region Конструктор

        /// <summary>
        /// Создаёт экземпляр класса <see cref="TraceContext"/>.
        /// </summary>
        /// <param name="traceId">Сквозной идентификатор.</param>
        /// <param name="traceSource">Источник идентификатора.</param>
        private TraceContext(Guid? traceId = null, string? traceSource = null)
        {
            if (traceId.HasValue)

            {
                TraceId = traceId;
                TraceIdSource = !string.IsNullOrWhiteSpace(traceSource)
                    ? traceSource
                    : "Unknown";
            }
            else
            {
                // чтобы указать что не был передан TraceId.
                // строкой - чтобы не вызывали из Enum в другом месте.
                TraceIdSource = "SetEmpty";
            }
        }

        #endregion Конструктор

        #region Методы (public)

        /// <summary>
        /// Создать контекст трейсинга.
        /// </summary>
        /// <param name="traceId">Сквозной идентификатор.</param>
        /// <param name="traceSource">Источник идентификатора.</param>
        /// <returns>Контекст трейсинга.</returns>
        public static TraceContext Create(Guid? traceId = null, string? traceSource = "Unknown")
        {
            lock (Lock)
            {
                if (CurrentTraceContext.Value == null)
                {
                    CurrentTraceContext.Value = new TraceContext(traceId, traceSource);
                }
                else
                {
                    CurrentTraceContext.Value.TraceId = traceId;
                }

                return CurrentTraceContext.Value;
            }
        }

        #endregion Методы (public)
    }
}