using OneHttpClient.Models;
using OneHttpClient.UnitTests.Fixtures;
using Xunit;
using static Xunit.Assert;

namespace OneHttpClient.UnitTests
{
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

        [Fact(DisplayName = "GET string should work.")]
        [Trait("Category", "GET")]
        public void Send_GET_string_should_return_200()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "text/plain";
            var expectedResponseBody = HttpServerFixture.SampleText;

            var response = _httpService.Send(HttpMethodEnum.GET, $"{_serverFixture.BaseAddress}/text").Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact(DisplayName = "GET JSON should work.")]
        [Trait("Category", "GET")]
        public void Send_GET_json_should_return_200()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.Send(HttpMethodEnum.GET, $"{_serverFixture.BaseAddress}/json").Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact(DisplayName = "GET should receive status code 500 without body when that was returned by server.")]
        [Trait("Category", "GET")]
        public void Send_GET_internal_error_should_return_500()
        {
            var expectedStatusCode = 500;
            var expectedResponseBody = string.Empty;

            var response = _httpService.Send(HttpMethodEnum.GET, $"{_serverFixture.BaseAddress}/internal-server-error").Result;

            //Assert
            NotNull(response);
            False(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            //Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact(DisplayName = "GET JSON should have deserialized response successfully when requested.")]
        [Trait("Category", "GET")]
        public void Send_GET_json_deserialized_should_return_200()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.Send<OperationResponseFixture>(HttpMethodEnum.GET, $"{_serverFixture.BaseAddress}/json").Result;

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

        [Fact(DisplayName = "GET should return the response without data deserialized when JSON is invalid.")]
        [Trait("Category", "GET")]
        public void Send_GET_invalid_json_deserialized_should_return_200_without_data_deserialized()
        {
            // Test: Get a response asking for deserialization. The invalid data format in response will make deserialization fail.
            // Result: Everything OK. There will be no data deserialized.

            var expectedStatusCode = 200;
            var expectedContentType = "application/json"; // Serialization will be attemped due to content type being json but will fail
            var expectedResponseBody = HttpServerFixture.SampleText;

            var response = _httpService.Send<OperationResponseFixture>(HttpMethodEnum.GET, $"{_serverFixture.BaseAddress}/invalid-json").Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));

            Null(response.ResponseData);
        }

        [Fact(DisplayName = "GET should return the response without data deserialized when Content-Type is not supported for deserialization by HttpService.")]
        [Trait("Category", "GET")]
        public void Send_GET_data_deserialized_with_unsupported_content_type_deserialization_should_return_200_without_data_deserialized()
        {
            // Test: Get a response asking for deserialization with content-type not supported for deserialization.
            // Result: Everything OK. There will be no data deserialized.

            var expectedStatusCode = 200;
            var expectedContentType = "text/plain"; // Deserialization will not even be attemped due to content type being plain text.
            var expectedResponseBody = HttpServerFixture.SampleText;

            var response = _httpService.Send<OperationResponseFixture>(HttpMethodEnum.GET, $"{_serverFixture.BaseAddress}/text").Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));

            Null(response.ResponseData);
        }

        [Fact(DisplayName = "POST should work. Return the same data sent with status code 200 when /echo is requested.")]
        [Trait("Category", "POST")]
        public void Send_POST_data_should_return_200_with_data_sent()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.Send(HttpMethodEnum.POST, $"{_serverFixture.BaseAddress}/echo", HttpServerFixture.SampleObject).Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact(DisplayName = "POST without content should work. Return empty body with status code 200 when /echo is requested.")]
        [Trait("Category", "POST")]
        public void Send_POST_wihtout_data_should_return_200_with_data_sent()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = string.Empty;

            var response = _httpService.Send(HttpMethodEnum.POST, $"{_serverFixture.BaseAddress}/echo", null).Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact(DisplayName = "PUT should work. Return the same data sent with status code 200 when /echo is requested.")]
        [Trait("Category", "PUT")]
        public void Send_PUT_data_should_return_200_with_data_sent()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.Send(HttpMethodEnum.PUT, $"{_serverFixture.BaseAddress}/echo", HttpServerFixture.SampleObject).Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact(DisplayName = "PACTH should work. Return the same data sent with status code 200 when /echo is requested.")]
        [Trait("Category", "PATCH")]
        public void Send_PATCH_data_should_return_200_with_data_sent()
        {
            var expectedStatusCode = 200;
            var expectedContentType = "application/json";
            var expectedResponseBody = HttpServerFixture.SampleTextJson;

            var response = _httpService.Send(HttpMethodEnum.PATCH, $"{_serverFixture.BaseAddress}/echo", HttpServerFixture.SampleObject).Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
            Equal(expectedContentType, response.Headers.Get("Content-Type"));
        }

        [Fact(DisplayName = "DELETE should work.")]
        [Trait("Category", "DELETE")]
        public void Send_DELETE_data_should_return_200()
        {
            var expectedStatusCode = 200;
            var expectedResponseBody = string.Empty;

            var response = _httpService.Send(HttpMethodEnum.DELETE, $"{_serverFixture.BaseAddress}/empty").Result;

            //Assert
            NotNull(response);
            True(response.IsSuccessStatusCode);
            Equal(expectedStatusCode, (int) response.StatusCode);
            Equal(expectedResponseBody, response.ResponseBody);
        }

        [Fact(DisplayName = "Serialization/Deserialization in snake_case should work when this naming strategy is chosen.")]
        [Trait("Category", "POST")]
        public void Send_in_snake_case_should_be_serialized_and_deserialized_correctly()
        {
            var expectedStatusCode = 200;
            var expectedResponseBody = HttpServerFixture.SampleTextJsonSnakeCase;
            var expectedResponseBodyObject = HttpServerFixture.SampleObject;

            var response = _httpService.Send<OperationResponseFixture>(HttpMethodEnum.POST, $"{_serverFixture.BaseAddress}/echo", HttpServerFixture.SampleObject, options: new HttpRequestOptions() { NamingStrategy = NamingStrategyEnum.SnakeCase }).Result;

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