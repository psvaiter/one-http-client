using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OneHttpClient.Models;

namespace OneHttpClient
{
    /// <summary>
    /// Service responsible to handle HTTP requests.
    /// </summary>
    public class HttpService : IHttpService
    {
        /// <summary>
        /// Static instance of HttpClient used to make all HTTP requests.
        /// </summary>
        private static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// The amount of time in seconds to keep open an active connection.
        /// Set to -1 (Timeout.Infinite) to keep the connection open forever as long as the connection is active.
        /// </summary>
        private static int _connectionLeaseTimeout;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <remarks>
        /// Connection lease timeout can be set to <see cref="Timeout.Infinite"/>. It's important to note
        /// that doing this can lead to problems detecting DNS changes. This problem is likely to occur 
        /// when a connection tends to be open all the time due to activity or to being reused before idle 
        /// timeout is reached. That's because <see cref="HttpClient"/> will not perform a DNS lookup 
        /// while the connection is already established.
        /// </remarks>
        /// <param name="defaultRequestTimeout">
        /// Default timeout in seconds to use on all HTTP requests made by this service.
        /// </param>
        /// <param name="connectionLeaseTimeout">
        /// Number of seconds after which an active connection should be closed. Requests in progress when 
        /// timeout is reached are not affected.
        /// </param>
        public HttpService(int defaultRequestTimeout = 100, int connectionLeaseTimeout = 10 * 60)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(defaultRequestTimeout);
            _connectionLeaseTimeout = connectionLeaseTimeout;
        }

        /// <summary>
        /// Sends an HTTP request message as JSON, gets the reponse and tries to deserialize it to a given type.
        /// </summary>
        /// <remarks>
        /// When both request-specific timeout and default timeout are set, the shorter one will apply.
        /// In other words, if timeoutInSeconds is greater than default timeout, the default will be used.
        /// </remarks>
        /// <typeparam name="TResponse">Type to deserialize the response body.</typeparam>
        /// <param name="url">URL to send request message.</param>
        /// <param name="method">HTTP method to use when making the request.</param>
        /// <param name="data">Object containing data to be sent or null when a body is not required.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="timeoutInSeconds">Amount of time in seconds to wait before cancelling request. When 0 the default timeout will be used.</param>
        /// <param name="namingStrategy">The stategy to use when serializing property names. Default is <see cref="NamingStrategyEnum.CamelCase"/>.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public Response<TResponse> SendJson<TResponse>(string url, HttpMethodEnum method, object data = null, NameValueCollection headers = null, int timeoutInSeconds = 0, NamingStrategyEnum namingStrategy = NamingStrategyEnum.CamelCase)
        {
            var response = SendJson(url, method, data, headers, timeoutInSeconds, namingStrategy);
            var deserializedResponseBody = TryDeserializeResponseBody<TResponse>(response.Headers, response.ResponseBody, namingStrategy);

            return new Response<TResponse>(response, deserializedResponseBody);
        }

        /// <summary>
        /// Sends an HTTP request message as JSON and gets the reponse (as string).
        /// </summary>
        /// <remarks>
        /// When both request-specific timeout and default timeout are set, the shorter one will apply.
        /// In other words, if timeoutInSeconds is greater than default timeout, the default will be used.
        /// </remarks>
        /// <param name="url">URL to send request message.</param>
        /// <param name="method">HTTP method to use when making the request.</param>
        /// <param name="data">Object containing data to be sent or null when a body is not required.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="timeoutInSeconds">Amount of time in seconds to wait before cancelling request. When 0 the default timeout will be used.</param>
        /// <param name="namingStrategy">The stategy to use when serializing property names. Default is <see cref="NamingStrategyEnum.CamelCase"/>.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public Response SendJson(string url, HttpMethodEnum method, object data = null, NameValueCollection headers = null, int timeoutInSeconds = 0, NamingStrategyEnum namingStrategy = NamingStrategyEnum.CamelCase)
        {
            SetActiveConnectionTimeout(url);
            // This will allow a DNS lookup periodically since HttpClient is static.

            using (var requestMessage = BuildRequestMessage(url, method, data, headers, namingStrategy))
            {
                return SendRequest(requestMessage, timeoutInSeconds).Result;
            }
        }

        /// <summary>
        /// This method sets the timeout for active connections to a service point (endpoint).
        /// The value applied is that that was set in class constructor.
        /// </summary>
        /// <param name="url">URL that defines an endpoint.</param>
        private void SetActiveConnectionTimeout(string url)
        {
            // Note this cannot be set globally, only per service point.
            // But the timeout value used is global for this service.
            var servicePoint = ServicePointManager.FindServicePoint(url, null);
            servicePoint.ConnectionLeaseTimeout = (_connectionLeaseTimeout == Timeout.Infinite)
                ? Timeout.Infinite
                : (_connectionLeaseTimeout * 1000);
        }

        /// <summary>
        /// Builds the request message serializing data as JSON.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="method">HTTP method to use when making the request.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="namingStrategy">The stategy to use when serializing property names. Default is <see cref="NamingStrategyEnum.CamelCase"/>.</param>
        /// <returns>The <see cref="HttpRequestMessage"/> object.</returns>
        private HttpRequestMessage BuildRequestMessage(string url, HttpMethodEnum method, object data, NameValueCollection headers, NamingStrategyEnum namingStrategy = NamingStrategyEnum.CamelCase)
        {
            const string contentType = "application/json";
            // TODO: Null check on data?
            var serializerSettings = GetJsonSerializerSettings(namingStrategy);
            string serializedData = JsonConvert.SerializeObject(data, serializerSettings);

            var requestMessage = new HttpRequestMessage(new HttpMethod(method.ToString()), url)
            {
                Content = new StringContent(serializedData, Encoding.UTF8, contentType)
            };

            SetRequestHeaders(requestMessage, headers);
            return requestMessage;
        }

        /// <summary>
        /// Gets the JSON serializer settings and sets the naming strategy supplied.
        /// </summary>
        /// <param name="strategy">The naming strategy to be used during serialization.</param>
        /// <returns>The serializer settings of Newtonsoft.Json.</returns>
        private JsonSerializerSettings GetJsonSerializerSettings(NamingStrategyEnum strategy)
        {
            return new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = GetNamingStrategy(strategy)
                }
            };
        }

        /// <summary>
        /// Gets the naming strategy of Newtonsoft.Json from our <see cref="NamingStrategyEnum"/>.
        /// </summary>
        /// <param name="strategy">Enumeration value of naming strategy.</param>
        /// <returns>The naming strategy to be used by Newtonsoft serializer.</returns>
        private NamingStrategy GetNamingStrategy(NamingStrategyEnum strategy)
        {
            if (strategy == NamingStrategyEnum.CamelCase)
            {
                return new CamelCaseNamingStrategy(processDictionaryKeys: true, overrideSpecifiedNames: true);
            }

            if (strategy == NamingStrategyEnum.SnakeCase)
            {
                return new SnakeCaseNamingStrategy(processDictionaryKeys: true, overrideSpecifiedNames: true);
            }

            return new DefaultNamingStrategy()
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = true
            };
        }

        /// <summary>
        /// Sets request headers in <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="message">The message where headers should be set.</param>
        /// <param name="headers">Headers to set.</param>
        private void SetRequestHeaders(HttpRequestMessage message, NameValueCollection headers)
        {
            if (message != null && headers?.Count > 0)
            {
                foreach (string key in headers.Keys)
                {
                    message.Headers.TryAddWithoutValidation(key, headers[key]);
                }
            }
        }

        /// <summary>
        /// Does the work of making the HTTP request and reading the response from server.
        /// </summary>
        /// <param name="requestMessage">The request message to send.</param>
        /// <param name="requestTimeout">Amount of time in seconds to wait before cancelling request.
        /// When 0 the default timeout will be used.
        /// </param>
        /// <returns>The response from server with the time it took to complete.</returns>
        private async Task<Response> SendRequest(HttpRequestMessage requestMessage, int requestTimeout = 0)
        {
            var stopwatch = Stopwatch.StartNew();

            var response = await _httpClient.SendAsync(requestMessage, CreateCancellationToken(requestTimeout));
            string responseBody = await response.Content.ReadAsStringAsync();

            stopwatch.Stop();

            return new Response(response, responseBody, stopwatch.Elapsed);
        }

        /// <summary>
        /// Creates a cancellation token that can be used to cancel a task after a given amount of time.
        /// </summary>
        /// <param name="delayInSeconds">Amount of time in seconds. Default is 0, which results in CancellationToken.None.</param>
        /// <returns>The cancellation token.</returns>
        private CancellationToken CreateCancellationToken(int delayInSeconds = 0)
        {
            if (delayInSeconds == 0)
            {
                return CancellationToken.None;
            }

            return new CancellationTokenSource(delayInSeconds * 1000).Token;
        }

        /// <summary>
        /// Tries to deserializes a response body based on response content type.
        /// </summary>
        /// <remarks>
        /// Currently supports only JSON. If content type is not supported or if deserialization 
        /// fails the default value of type is returned.
        /// </remarks>
        /// <typeparam name="TResponse">Type to deserialize the response body.</typeparam>
        /// <param name="headers">Headers with information </param>
        /// <param name="responseBody">The string of response body to be deserialized.</param>
        /// <param name="namingStrategy">The stategy to use when serializing property names.
        /// Default is <see cref="NamingStrategyEnum.CamelCase"/>.
        /// </param>
        /// <returns>The deserialized object or default value of type.</returns>
        private TResponse TryDeserializeResponseBody<TResponse>(NameValueCollection headers, string responseBody, NamingStrategyEnum namingStrategy = NamingStrategyEnum.CamelCase)
        {
            if (headers != null && string.IsNullOrEmpty(responseBody) == false)
            {
                string contentType = headers.Get("Content-Type");

                if (contentType?.Contains("application/json") == true)
                {
                    return TryDeserializeJson<TResponse>(responseBody, GetJsonSerializerSettings(namingStrategy));
                }
            }

            return default(TResponse);
        }

        /// <summary>
        /// Deserializes a string in JSON format.
        /// </summary>
        /// <typeparam name="T">Type to deserialize JSON.</typeparam>
        /// <param name="json">String formatted as valid JSON.</param>
        /// <param name="serializerSettings">Settings to be used on deserialization. If this is null, the default serialization settings will be used.</param>
        /// <returns>The deserialized object when successful. The default value of type otherwise.</returns>
        private T TryDeserializeJson<T>(string json, JsonSerializerSettings serializerSettings = null)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, serializerSettings);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
