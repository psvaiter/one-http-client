using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneHttpClient.Extensions;
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
        /// Set to -1 (<see cref="Timeout.Infinite"/>) to keep the connection open forever as long as the connection is 
        /// active.
        /// </summary>
        private static int _connectionLeaseTimeout;

        /// <summary>
        /// Logger that will be used to log events of the service.
        /// </summary>
        private readonly ILogger<HttpService> _logger;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="defaultRequestTimeout">
        /// Default timeout in seconds to use on all HTTP requests made by this service.
        /// </param>
        /// <param name="connectionLeaseTimeout">
        /// Number of seconds after which an active connection should be closed. Requests in progress when timeout is 
        /// reached are not affected. The default is 10 minutes.
        /// </param>
        /// <param name="logger">
        /// Implementation of <see cref="ILogger"/> that will be used for logging events.
        /// </param>
        /// <remarks>
        /// Connection lease timeout can be set to <see cref="Timeout.Infinite"/>. It's important to note that doing 
        /// this can lead to problems detecting DNS changes. This problem is likely to occur when a connection tends 
        /// to be open all the time due to activity (like being reused before idle timeout is reached. That's because 
        /// <see cref="HttpClient"/> will not perform a DNS lookup while the connection is already established.
        /// </remarks>
        public HttpService(int defaultRequestTimeout = 100, int connectionLeaseTimeout = 10 * 60, ILogger<HttpService> logger = null)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(defaultRequestTimeout);
            _connectionLeaseTimeout = connectionLeaseTimeout;
            _logger = logger;
            
            ServicePointManager.DefaultConnectionLimit = 10;
        }

        /// <summary>
        /// Sends an HTTP request message, gets the reponse and tries to deserialize it to a given type.
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
        /// <param name="options">Advanced request options. See <see cref="HttpRequestOptions"/> for more details.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public async Task<Response<TResponse>> Send<TResponse>(HttpMethodEnum method, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            if (options == null)
            {
                options = new HttpRequestOptions();
            }

            var response = await Send(method, url, data, headers, options);
            var deserializedResponseBody = Utils.TryDeserializeResponseBody<TResponse>(response.Headers, response.ResponseBody, options.NamingStrategy, options.NullValueHandling);

            return new Response<TResponse>(response, deserializedResponseBody);
        }

        /// <summary>
        /// Sends an HTTP request message and gets the reponse (as string).
        /// </summary>
        /// <remarks>
        /// When both request-specific timeout and default timeout are set, the shorter one will apply.
        /// In other words, if timeoutInSeconds is greater than default timeout, the default will be used.
        /// </remarks>
        /// <param name="url">URL to send request message.</param>
        /// <param name="method">HTTP method to use when making the request.</param>
        /// <param name="data">Object containing data to be sent or null when a body is not required.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">Advanced request options. See <see cref="HttpRequestOptions"/> for more details.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public async Task<Response> Send(HttpMethodEnum method, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            SetActiveConnectionTimeout(url);
            // This will allow a DNS lookup periodically since HttpClient is static.

            if (options == null)
            {
                options = new HttpRequestOptions();
            }

            using (var requestMessage = BuildRequestMessage(method, url, data, headers, options))
            {
                return await SendRequest(requestMessage, options.TimeoutInSeconds);
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
        /// Builds the request message serializing data.
        /// </summary>
        /// <param name="method">HTTP method to use when making the request.</param>
        /// <param name="url">URL to send request message.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="namingStrategy">The stategy to use when serializing property names. Default is <see cref="NamingStrategyEnum.CamelCase"/>.</param>
        /// <returns>The <see cref="HttpRequestMessage"/> object.</returns>
        private HttpRequestMessage BuildRequestMessage(HttpMethodEnum method, string url, object data, NameValueCollection headers, HttpRequestOptions options)
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(method.ToString()), url)
            {
                Content = Utils.CreateHttpContent(data, options)
            };

            SetRequestHeaders(requestMessage, headers);
            return requestMessage;
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
        /// <param name="requestTimeout">Amount of time in seconds to wait before cancelling the request.
        /// </param>
        /// <returns>The response from server with the time it took to complete.</returns>
        /// <remarks>
        /// The <paramref name="requestTimeout"/> will only be effective if it's value is less than default timeout
        /// configured during initialization of <see cref="_httpClient"/>. Otherwise the default timeout will take 
        /// place. When <paramref name="requestTimeout"/> is zero the per-request basis timeout is not even configured.
        /// </remarks>
        private async Task<Response> SendRequest(HttpRequestMessage requestMessage, int requestTimeout = 0)
        {
            using (var cts = GetCancellationTokenSource(TimeSpan.FromSeconds(requestTimeout)))
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    _logger?.RequestStarting(requestMessage);

                    var httpResponseMessage = await _httpClient.SendAsync(requestMessage, cts?.Token ?? CancellationToken.None);
                    string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();

                    stopwatch.Stop();
                    _logger?.RequestFinished(stopwatch.Elapsed, httpResponseMessage.StatusCode);

                    return new Response(httpResponseMessage, responseBody, stopwatch.Elapsed);
                }
                catch (TaskCanceledException)
                {
                    stopwatch.Stop();

                    var timeoutException = new TimeoutException($"The operation has timed out after {stopwatch.Elapsed.TotalMilliseconds} ms.");
                    timeoutException.Data.Add("ElapsedMilliseconds", stopwatch.Elapsed.TotalMilliseconds);

                    throw timeoutException;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="CancellationTokenSource"/> to cancel a task after a given amount of time.
        /// </summary>
        /// <param name="delay">Amount of time to wait before cancelling a task.</param>
        /// <returns>The <see cref="CancellationTokenSource"/> when delay is greater than zero or null otherwise.</returns>
        private CancellationTokenSource GetCancellationTokenSource(TimeSpan delay)
        {
            if (delay <= TimeSpan.Zero)
            {
                return null;
            }

            return new CancellationTokenSource(delay);
        }

        // GET
        public Task<Response> Get(string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send(HttpMethodEnum.GET, url, null, headers, options);
        }

        // GET (typed response)
        public Task<Response<TResponse>> Get<TResponse>(string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send<TResponse>(HttpMethodEnum.GET, url, null, headers, options);
        }

        // POST
        public Task<Response> Post(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send(HttpMethodEnum.POST, url, data, headers, options);
        }

        // POST (typed response)
        public Task<Response<TResponse>> Post<TResponse>(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send<TResponse>(HttpMethodEnum.POST, url, data, headers, options);
        }

        // PUT
        public Task<Response> Put(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send(HttpMethodEnum.PUT, url, data, headers, options);
        }

        // PUT (typed response)
        public Task<Response<TResponse>> Put<TResponse>(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send<TResponse>(HttpMethodEnum.PUT, url, data, headers, options);
        }

        // PATCH
        public Task<Response> Patch(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send(HttpMethodEnum.PATCH, url, data, headers, options);
        }

        // PATCH (typed response)
        public Task<Response<TResponse>> Patch<TResponse>(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send<TResponse>(HttpMethodEnum.PATCH, url, data, headers, options);
        }

        // DELETE
        public Task<Response> Delete(string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send(HttpMethodEnum.DELETE, url, null, headers, options);
        }

        // DELETE (typed response)
        public Task<Response<TResponse>> Delete<TResponse>(string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return Send<TResponse>(HttpMethodEnum.DELETE, url, null, headers, options);
        }
    }
}
