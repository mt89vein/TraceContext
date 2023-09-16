TraceContext
=============

> ⚠️ This project is a proof of concept and should not be used as a primary library for tracing (passing single Guid TraceId), consider using full-featured https://opentelemetry.io/ and it's [System.Diagnostics.Activity](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.activity?view=net-7.0) that available out of the box.

TraceContext class with ability to access, maintain and pass on TraceId guid anywhere in code.
Including http requests.
Useful for distributed tracing with structured logging.

[![NuGet version (TraceContext)](https://img.shields.io/nuget/v/TraceContext.svg?style=flat-square)](https://www.nuget.org/packages/TraceContext)
![UnitTest](https://github.com/mt89vein/TraceContext/workflows/UnitTest/badge.svg)

### Installing TraceContext

You should install [TraceContext with NuGet](https://www.nuget.org/packages/TraceContext):

    Install-Package TraceContext
    
Or via the .NET Core command line interface:

    dotnet add package TraceContext

###

# Setup

[в Startup.cs]

1.

```diff
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
+   services.AddTraceId(); // You can configure some tracing parameters for traceIdMiddleware via lambda.
}
```

2.

```diff
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
+   app.UseTraceId(); // enables TraceId middleware in http pipeline.

   // other middlewares
}
```

If you need to get current TraceId somewhere in the code (including async and multithreading code):

```cs

public class MyAwesomeClass
{
     public void MyAwesomeMethod()
     {
          var traceId = TraceContext.Current.TraceId;
     }
}
```

Useful extension methods:

### for ILogger
```cs

using (logger.WithTraceContext())
{
    // log messages in using will contain traceId in scopes
}

// it is shorthand for

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
```

### for IHttpClientBuilder

```cs

public static IHttpClientBuilder AddHttpClient(this IServiceCollection services)
{
    return services
        .AddHttpClient<IGithubClient, GitHubClient()
        .AddTracing();
        // this applies TraceIdDelegatingHandler to all requests,
        // that enrich request headers with current traceId and traceId source.
}

```

Possible configuration:


```cs

    /// <summary>
    /// TraceId middleware settings.
    /// </summary>
    public class TraceIdSettings
    {
        /// <summary>
        /// Default HTTP header name for traceId.
        /// </summary>
        private const string DefaultHeader = "X-TRACE-ID";

        /// <summary>
        /// Default HTTP header name for traceId source.
        /// </summary>
        private const string DefaulSourceIdHeader = "X-TRACE-ID-SOURCE";

        /// <summary>
        /// HTTP header name for traceId.
        /// </summary>
        public string Header { get; set; } = DefaultHeader;

        /// <summary>
        /// HTTP header name for traceId source.
        /// </summary>
        public string TraceIdSourceHeader { get; set; } = DefaulSourceIdHeader;

        /// <summary>
        /// Auto generate traceId if it is not present in incoming http request headers.
        /// <para> Default: false.</para>
        /// </summary>
        public bool GenerateIfNotPresent { get; set; }

        /// <summary>
        /// Should return 400 Bad request, if traceId not passed in incoming http request headers, and <see cref="GenerateIfNotPresent"/> was set to false.
        /// <para> Default: false.</para>
        /// </summary>
        public bool ThrowBadRequestIfNotPresent { get; set; }

        /// <summary>
        /// Begin scope for ILogger with current traceId and traceIdSource
        /// </summary>
        public bool UseLoggerScope { get; set; } = true;

        /// <summary>
        /// Log debug if traceId was generated.
        /// </summary>
        public bool LogIfTraceIdGenerated { get; set; }
    }

```

### Contribute

Feel free for creation issues, or PR :)

### License

Copyright © 2020 Shamil Sultanov

The MIT licence.
