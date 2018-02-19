using System;
using System.Collections.Generic;
using MockHttpServer;

namespace OneHttpClient.UnitTests.Fixtures
{
    public class HttpServerFixture : IDisposable
    {
        public MockServer MockServer { get; private set; }
        public string BaseAddress { get; private set; }

        public readonly static string SampleText = "You made it!";
        public readonly static string SampleTextJson = "{\"success\":true,\"serverMessage\":\"You made it!\"}";
        public readonly static string SampleTextJsonSnakeCase = "{\"success\":true,\"server_message\":\"You made it!\"}";
        public readonly static OperationResponseFixture SampleObject = new OperationResponseFixture() { Success = true, ServerMessage = SampleText };

        public HttpServerFixture()
        {
            // Setup endpoint in mock server
            var requestHandlers = new List<MockHttpHandler>()
            {
                new MockHttpHandler("/empty", (req, resp, param) => resp.StatusCode(200).ContentType("text/plain").Content("")),
                new MockHttpHandler("/text", (req, resp, param) => resp.StatusCode(200).ContentType("text/plain").Content(SampleText)),
                new MockHttpHandler("/json", (req, resp, param) => resp.StatusCode(200).ContentType("application/json").Content(SampleTextJson)),
                new MockHttpHandler("/echo", (req, resp, param) => resp.StatusCode(200).ContentType("application/json").Content(req.Content())),
                new MockHttpHandler("/invalid-json", (req, resp, param) => resp.StatusCode(200).ContentType("application/json").Content(SampleText)),
                new MockHttpHandler("/internal-server-error", (req, resp, param) => resp.StatusCode(500).Content("")),
            };

            // Create a mock http server on one un-used port.
            MockServer = new MockServer(0, requestHandlers);

            // Set base address based on chosen port.
            BaseAddress = $"http://localhost:{MockServer.Port}";
        }

        public void Dispose()
        {
            MockServer.Dispose();
        }
    }
}
