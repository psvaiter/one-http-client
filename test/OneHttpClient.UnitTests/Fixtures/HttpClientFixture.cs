using System;
using Microsoft.Extensions.Logging;

namespace OneHttpClient.UnitTests.Fixtures
{
    public class HttpClientFixture : IDisposable
    {
        public HttpService HttpService { get; }
        public ILogger<HttpService> Logger { get; }

        public HttpClientFixture()
        {
            //Logger = Substitute.For<ILogger<HttpService>>();
            HttpService = new HttpService(logger: Logger);
        }

        public void Dispose()
        {

        }
    }
}
