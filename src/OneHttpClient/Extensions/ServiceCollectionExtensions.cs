using Microsoft.Extensions.DependencyInjection;

namespace OneHttpClient.Extensions
{
    /// <summary>
    /// Extension methods for IServiceCollection interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IHttpService"/> to service collection with the official implementation.
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
    }
}
