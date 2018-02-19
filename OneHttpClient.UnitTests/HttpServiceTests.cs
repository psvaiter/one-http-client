using System;
using System.Collections.Generic;
using MockHttpServer;
using OneHttpClient;
using OneHttpClient.Models;
using Xunit;
using static Xunit.Assert;

namespace OneHttpClient.UnitTests
{
    public class OperationResponse
    {
        public bool Success { get; set; }
        public string ServerMessage { get; set; }
    }

    public class HttpServerFixture : IDisposable
    {
        public MockServer MockServer { get; private set; }
        public string BaseAddress { get; private set; }

        public readonly static string SampleText = "You made it!";
        public readonly static string SampleTextJson = "{\"success\":true,\"serverMessage\":\"You made it!\"}";
        public readonly static string SampleTextJsonSnakeCase = "{\"success\":true,\"server_message\":\"You made it!\"}";
        public readonly static OperationResponse SampleObject = new OperationResponse() { Success = true, ServerMessage = SampleText };

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

    public class HttpServiceTests : IClassFixture<HttpServerFixture>, IClassFixture<HttpClientFixture>
    {
        private HttpServerFixture _serverFixture;
        private HttpClientFixture _clientFixture;
        private HttpService _httpService;

        public HttpServiceTests(HttpServerFixture serverFixture, HttpClientFixture clientFixture)
        {
            _serverFixture = serverFixture;
            _clientFixture = clientFixture;

            _httpService = _clientFixture.HttpService;
        }

        [Fact]
        [Trait("Category", "GET")]
        public void SendJson_to_GET_string_should_return_200()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "text/plain";
            var expectedResponseBody = HttpServerFixture.SampleText;

            var response = _httpService.SendJson($"{_serverFixture.BaseAddress}/text", HttpMethodEnum.GET);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact]
        [Trait("Category", "GET")]
        public void SendJson_to_GET_json_should_return_200()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.SendJson($"{_serverFixture.BaseAddress}/json", HttpMethodEnum.GET);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact]
        [Trait("Category", "GET")]
        public void SendJson_to_GET_internal_error_should_return_500()
        {
            var expectedStatusCode = 500;
            var expectedResponseBody = string.Empty;

            var response = _httpService.SendJson($"{_serverFixture.BaseAddress}/internal-server-error", HttpMethodEnum.GET);

            //Assert
            NotNull(response);
            False(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            //Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact]
        [Trait("Category", "GET")]
        public void SendJson_to_GET_json_deserialized_should_return_200()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.SendJson<OperationResponse>($"{_serverFixture.BaseAddress}/json", HttpMethodEnum.GET);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equals(expectedStatusCode, (int) response.StatusCode);
            Equals(expectedResponseBody, response.ResponseBody);
            Contains(expectedContentType, response.Headers.Get("Content-Type"));

            NotNull(response.ResponseData);
            Equal(HttpServerFixture.SampleObject.Success, response.ResponseData.Success);
            Equal(HttpServerFixture.SampleObject.ServerMessage, response.ResponseData.ServerMessage);
        }

        [Fact]
        [Trait("Category", "GET")]
        public void SendJson_to_GET_invalid_json_deserialized_should_return_200_without_data_deserialized()
        {
            // Test: Get a response asking for deserialization. The invalid data format in response will make deserialization fail.
            // Result: Everything OK. There will be no data deserialized.

            var expectedStatusCode = 200;
            var expectedContentType = "application/json"; // Serialization will be attemped due to content type being json but will fail
            var expectedResponseBody = HttpServerFixture.SampleText;

            var response = _httpService.SendJson<OperationResponse>($"{_serverFixture.BaseAddress}/invalid-json", HttpMethodEnum.GET);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));

            Null(response.ResponseData);
        }

        [Fact]
        [Trait("Category", "GET")]
        public void SendJson_to_GET_data_deserialized_with_unsupported_content_type_deserialization_should_return_200_without_data_deserialized()
        {
            // Test: Get a response asking for deserialization with content-type not supported for deserialization.
            // Result: Everything OK. There will be no data deserialized.

            var expectedStatusCode = 200;
            var expectedContentType = "text/plain"; // Deserialization will not even be attemped due to content type being plain text.
            var expectedResponseBody = HttpServerFixture.SampleText;

            var response = _httpService.SendJson<OperationResponse>($"{_serverFixture.BaseAddress}/text", HttpMethodEnum.GET);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));

            Null(response.ResponseData);
        }

        [Fact]
        [Trait("Category", "POST")]
        public void SendJson_to_POST_data_should_return_200_with_data_sent()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.SendJson($"{_serverFixture.BaseAddress}/echo", HttpMethodEnum.POST, HttpServerFixture.SampleObject);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact]
        [Trait("Category", "PUT")]
        public void SendJson_to_PUT_data_should_return_200_with_data_sent()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.SendJson($"{_serverFixture.BaseAddress}/echo", HttpMethodEnum.PUT, HttpServerFixture.SampleObject);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact]
        [Trait("Category", "PATCH")]
        public void SendJson_to_PATCH_data_should_return_200_with_data_sent()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.SendJson($"{_serverFixture.BaseAddress}/echo", HttpMethodEnum.PATCH, HttpServerFixture.SampleObject);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact]
        [Trait("Category", "DELETE")]
        public void SendJson_to_DELETE_data_should_return_200()
        {
            var expectedStatusCode = 200;
            var expectedResponseBody = string.Empty;

            var response = _httpService.SendJson($"{_serverFixture.BaseAddress}/empty", HttpMethodEnum.DELETE);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
        }

        [Fact]
        [Trait("Category", "POST")]
        public void SendJson_in_snake_case_should_be_serialized_and_deserialized_correctly()
        {
            var expectedStatusCode = 200;
            var expectedResponseBody = HttpServerFixture.SampleTextJsonSnakeCase;
            var expectedResponseBodyObject = HttpServerFixture.SampleObject;

            var response = _httpService.SendJson<OperationResponse>($"{_serverFixture.BaseAddress}/echo", HttpMethodEnum.POST, HttpServerFixture.SampleObject, namingStrategy: NamingStrategyEnum.SnakeCase);

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);

            NotNull(response.ResponseData);
            True(response.ResponseData.Success);
            Equal(HttpServerFixture.SampleText, response.ResponseData.ServerMessage);
        }
    }
}