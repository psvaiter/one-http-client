using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneHttpClient;
using OneHttpClient.Extensions;

namespace TestConsoleApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<IHttpService, HttpService>(svcProvider => new HttpService(connectionLeaseTimeout: TimeSpan.FromSeconds(30)));
            services.AddOneHttpClient(null, connectionLeaseTimeout: TimeSpan.FromSeconds(30));
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseMvc();
        }
    }
}
