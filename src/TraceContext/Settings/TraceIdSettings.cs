namespace TraceContext.Settings
{
    /// <summary>
    /// Настройки TraceId.
    /// </summary>
    public class TraceIdSettings
    {
        /// <summary>
        /// Название заголовка по-умолчанию.
        /// </summary>
        private const string DefaultHeader = "X-TRACE-ID";

        /// <summary>
        /// Название заголовка по-умолчанию.
        /// </summary>
        private const string DefaulSourceIdHeader = "X-TRACE-ID-SOURCE";

        /// <summary>
        /// Название заголовка для записи/чтения TraceId.
        /// </summary>
        public string Header { get; set; } = DefaultHeader;

        /// <summary>
        /// Название заголовка для записи/чтения TraceId.
        /// </summary>
        public string TraceIdSourceHeader { get; set; } = DefaulSourceIdHeader;

        /// <summary>
        /// Необходимо ли генерировать новый TraceId, в тех случаях, когда TraceId не удалось получить из заголовков запроса.
        /// <para> Default: false.</para>
        /// </summary>
        public bool GenerateIfNotPresent { get; set; }

        /// <summary>
        /// Вернуть 400 ошибку, если в заголовке не был представлен TraceId и не был сгенерирован, т.к. <see cref="GenerateIfNotPresent"/> был установлен в false.
        /// <para> Default: false.</para>
        /// </summary>
        public bool ThrowBadRequestIfNotPresent { get; set; }

        /// <summary>
        /// Обернуть общим скоупом логгера с TraceId или нет.
        /// </summary>
        public bool UseLoggerScope { get; set; } = true;

        /// <summary>
        /// Логировать, если TraceId был сгенерирован, т.к. не был представлен в заголовке.
        /// </summary>
        public bool LogIfTraceIdGenerated { get; set; }
    }
}
