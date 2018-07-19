using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OneHttpClient.Extensions
{
    /// <summary>
    /// Extension methods for IServiceCollection interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IHttpService"/> to a service collection with the official implementation.
        /// </summary>
        /// <param name="services">The service collection provided by application.</param>
        /// <returns>The same service collection provided in <paramref name="services"/>.</returns>
        /// <remarks>
        /// If an implemenatation of Microsoft Logging interface (ILogger) exists in service 
        /// collection it will be injected into OneHttpClient.
        /// </remarks>
        public static IServiceCollection AddOneHttpClient(this IServiceCollection services)
        {
            services.AddSingleton<IHttpService, HttpService>();
            return services;
        }

        /// <summary>
        /// Adds <see cref="IHttpService"/> to a service collection with the official implementation.
        /// </summary>
        /// <param name="services">The service collection provided by application.</param>
        /// <param name="defaultRequestTimeout">
        /// Default timeout to use on all HTTP requests made by this service. The default is 100 seconds.
        /// Lower timeouts informed via <see cref="HttpRequestOptions"/> has precedence over the default timeout.
        /// </param>
        /// <param name="connectionLeaseTimeout">
        /// Amount of time after which an active connection should be closed. Requests in progress when timeout is 
        /// reached are not affected. The default is 10 minutes.
        /// </param>
        /// <returns>The same service collection provided in <paramref name="services"/>.</returns>
        /// <remarks>
        /// If an implemenatation of Microsoft Logging interface (ILogger) exists in service 
        /// collection it will be injected into OneHttpClient.
        /// </remarks>
        public static IServiceCollection AddOneHttpClient(this IServiceCollection services, TimeSpan? defaultRequestTimeout, TimeSpan? connectionLeaseTimeout)
        {
            services.AddSingleton<IHttpService, HttpService>(svcProvider =>
            {
                var logger = svcProvider.GetService<ILogger<HttpService>>();
                return new HttpService(defaultRequestTimeout, connectionLeaseTimeout, logger);
            });
            return services;
        }
    }
}
