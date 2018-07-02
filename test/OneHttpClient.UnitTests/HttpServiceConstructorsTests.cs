using System;
using System.Collections.Specialized;
using System.Net;
using OneHttpClient.Models;
using OneHttpClient.UnitTests.Fixtures;
using Xunit;

namespace OneHttpClient.UnitTests
{
    public class HttpServiceConstructorsTests : IClassFixture<HttpServerFixture>, IClassFixture<HttpClientFixture>
    {
        private HttpServerFixture _serverFixture;
        private HttpClientFixture _clientFixture;
        private HttpService _httpService;

        public HttpServiceConstructorsTests(HttpServerFixture serverFixture, HttpClientFixture clientFixture)
        {
            _serverFixture = serverFixture;
            _clientFixture = clientFixture;

            _httpService = _clientFixture.HttpService;
        }

        public class Given_simple_response_OK_with_headers_and_body : HttpServiceConstructorsTests
        {
            private Response _response;
            public TimeSpan _expectedElapsedTime = TimeSpan.FromSeconds(1);
            public string _expectedResponseBody = HttpServerFixture.SampleTextJson;
            public NameValueCollection _expectedHeaders = new NameValueCollection
            {
                { "test key", "test value" }
            };

            public Given_simple_response_OK_with_headers_and_body(HttpServerFixture serverFixture, HttpClientFixture clientFixture) : base(serverFixture, clientFixture)
            {
                // Act
                _response = new Response(HttpStatusCode.OK, _expectedHeaders, _expectedResponseBody, _expectedElapsedTime);
            }

            [Fact(DisplayName = "When constructing response OK with headers and body Should StatusCode be OK.")]
            [Trait("Category", "Constructor")]
            public void StatusCode_should_be_OK() => Assert.Equal(HttpStatusCode.OK, _response.StatusCode);

            [Fact(DisplayName = "When constructing response OK with headers and body Should IsSuccessStatusCode be true.")]
            [Trait("Category", "Constructor")]
            public void IsSuccessStatusCode_should_be_true() => Assert.True(_response.IsSuccessStatusCode);

            [Fact(DisplayName = "When constructing response OK with headers and body Should ResponseBody be the same.")]
            [Trait("Category", "Constructor")]
            public void ResponseBody_should_be_the_same() => Assert.Equal(_expectedResponseBody, _response.ResponseBody);

            [Fact(DisplayName = "When constructing response OK with headers and body Should FullResponse be null.")]
            [Trait("Category", "Constructor")]
            public void FullResponse_should_be_null() => Assert.Null(_response.FullResponse);

            [Fact(DisplayName = "When constructing response OK with headers and body Should Headers be the same.")]
            [Trait("Category", "Constructor")]
            public void Headers_should_be_the_same()
            {
                Assert.Equal("test value", _response.Headers.GetValues("test key")[0]);
            }
        }

        public class Given_typed_response_OK_with_headers_and_body : HttpServiceConstructorsTests
        {
            private Response<OperationResponseFixture> _response;
            public TimeSpan _expectedElapsedTime = TimeSpan.FromSeconds(1);
            public string _expectedResponseBody = HttpServerFixture.SampleTextJson;
            public OperationResponseFixture _expectedResponseData = HttpServerFixture.SampleObject;
            public NameValueCollection _expectedHeaders = new NameValueCollection
            {
                { "test key", "test value" }
            };

            public Given_typed_response_OK_with_headers_and_body(HttpServerFixture serverFixture, HttpClientFixture clientFixture) : base(serverFixture, clientFixture)
            {
                // Act
                _response = new Response<OperationResponseFixture>(HttpStatusCode.OK, _expectedHeaders, _expectedResponseBody, _expectedResponseData, _expectedElapsedTime);
            }

            [Fact(DisplayName = "When constructing typed response OK with headers and body Should StatusCode be OK.")]
            [Trait("Category", "Constructor")]
            public void StatusCode_should_be_OK() => Assert.Equal(HttpStatusCode.OK, _response.StatusCode);

            [Fact(DisplayName = "When constructing typed response OK with headers and body Should IsSuccessStatusCode be true.")]
            [Trait("Category", "Constructor")]
            public void IsSuccessStatusCode_should_be_true() => Assert.True(_response.IsSuccessStatusCode);

            [Fact(DisplayName = "When constructing typed response OK with headers and body Should ResponseBody be the same.")]
            [Trait("Category", "Constructor")]
            public void ResponseBody_should_be_the_same() => Assert.Equal(_expectedResponseBody, _response.ResponseBody);

            [Fact(DisplayName = "When constructing typed response OK with headers and body Should ResponseData not be null.")]
            [Trait("Category", "Constructor")]
            public void ResponseData_should_not_be_null() => Assert.NotNull(_response.ResponseData);

            [Fact(DisplayName = "When constructing typed response OK with headers and body Should FullResponse be null.")]
            [Trait("Category", "Constructor")]
            public void FullResponse_should_be_null() => Assert.Null(_response.FullResponse);

            [Fact(DisplayName = "When constructing typed response OK with headers and body Should Headers be the same.")]
            [Trait("Category", "Constructor")]
            public void Headers_should_be_the_same()
            {
                Assert.Equal("test value", _response.Headers.GetValues("test key")[0]);
            }
        }

        public class Given_another_response : HttpServiceConstructorsTests
        {
            private Response<OperationResponseFixture> _response;
            private Response _expectedResponse;
            public TimeSpan _expectedElapsedTime = TimeSpan.FromSeconds(1);
            public string _expectedResponseBody = HttpServerFixture.SampleTextJson;
            public OperationResponseFixture _expectedResponseData = HttpServerFixture.SampleObject;
            public NameValueCollection _expectedHeaders = new NameValueCollection
            {
                { "test key", "test value" }
            };

            public Given_another_response(HttpServerFixture serverFixture, HttpClientFixture clientFixture) : base(serverFixture, clientFixture)
            {
                // Arrange
                _expectedResponse = new Response(HttpStatusCode.OK, _expectedHeaders, _expectedResponseBody, _expectedElapsedTime);
                
                // Act
                _response = new Response<OperationResponseFixture>(_expectedResponse, _expectedResponseData);
            }

            [Fact(DisplayName = "When constructing typed response from another response Should StatusCode be OK.")]
            [Trait("Category", "Constructor")]
            public void StatusCode_should_be_OK() => Assert.Equal(_expectedResponse.StatusCode, _response.StatusCode);

            [Fact(DisplayName = "When constructing typed response from another response Should IsSuccessStatusCode be true.")]
            [Trait("Category", "Constructor")]
            public void IsSuccessStatusCode_should_be_true() => Assert.True(_response.IsSuccessStatusCode);

            [Fact(DisplayName = "When constructing typed response from another response Should ResponseBody be the same.")]
            [Trait("Category", "Constructor")]
            public void ResponseBody_should_be_the_same() => Assert.Equal(_expectedResponseBody, _response.ResponseBody);

            [Fact(DisplayName = "When constructing typed response OK with headers and body Should ResponseData not be null.")]
            [Trait("Category", "Constructor")]
            public void ResponseData_should_not_be_null() => Assert.NotNull(_response.ResponseData);

            [Fact(DisplayName = "When constructing typed response from another response Should FullResponse be null.")]
            [Trait("Category", "Constructor")]
            public void FullResponse_should_be_null() => Assert.Null(_response.FullResponse);

            [Fact(DisplayName = "When constructing typed response from another response Should Headers be the same.")]
            [Trait("Category", "Constructor")]
            public void Headers_should_be_the_same()
            {
                Assert.Equal("test value", _response.Headers.GetValues("test key")[0]);
            }
        }
    }
}