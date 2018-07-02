using System;

namespace OneHttpClient.UnitTests.Fixtures
{
    public class HttpClientFixture : IDisposable
    {
        public HttpService HttpService { get; private set; }

        public HttpClientFixture()
        {
            HttpService = new HttpService();
        }

        public void Dispose()
        {

        }
    }
}
