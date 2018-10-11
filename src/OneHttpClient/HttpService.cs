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
        /// Indicates if the ctor has been called. Prevents side-effects from multiple calls to ctor.
        /// </summary>
        private static bool _initialized = false;
        
        /// <summary>
        /// Static instance of HttpClient used to make all HTTP requests.
        /// </summary>
        private static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// The amount of time to keep open an active connection.
        /// Set to <see cref="Timeout.InfiniteTimeSpan"/> to keep the connection open forever as long as the 
        /// connection is active.
        /// </summary>
        private static TimeSpan _connectionLeaseTimeout;

        /// <summary>
        /// Logger that will be used to log events of the service.
        /// </summary>
        private readonly ILogger<HttpService> _logger;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="defaultRequestTimeout">
        /// Default timeout to use on all HTTP requests made by this service. The default is 100 seconds.
        /// Lower timeouts informed via <see cref="HttpRequestOptions"/> has precedence over the default timeout.
        /// </param>
        /// <param name="connectionLeaseTimeout">
        /// Amount of time after which an active connection should be closed. Requests in progress when timeout is 
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
        /// 
        /// The HttpClient.Timeout value should not be changed after this initalization. That's because the client is
        /// static and changing this value could lead to runtime errors if a request is being made simultaneously.
        /// </remarks>
        public HttpService(TimeSpan? defaultRequestTimeout = null, TimeSpan? connectionLeaseTimeout = null, ILogger<HttpService> logger = null)
        {
            if (!_initialized)
            {
                _httpClient.Timeout = (defaultRequestTimeout == null)
                    ? Constants.DefaultRequestTimeout
                    : defaultRequestTimeout.Value;

                _connectionLeaseTimeout = (connectionLeaseTimeout == null)
                    ? Constants.DefaultConnectionLeaseTimeout
                    : connectionLeaseTimeout.Value;

                // Increase DefaultConnectionLimit if too low, else keep it high.
                if (ServicePointManager.DefaultConnectionLimit < Constants.DefaultConnectionLimit)
                {
                    ServicePointManager.DefaultConnectionLimit = Constants.DefaultConnectionLimit;
                }

                _logger = logger;
                _initialized = true;
            }
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
        public async Task<Response<TResponse>> SendAsync<TResponse>(HttpMethodEnum method, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            if (options == null)
            {
                options = new HttpRequestOptions();
            }

            var response = await SendAsync(method, url, data, headers, options);
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
        public async Task<Response> SendAsync(HttpMethodEnum method, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            SetActiveConnectionTimeout(url);
            // This will allow a DNS lookup periodically since HttpClient is static.

            if (options == null)
            {
                options = new HttpRequestOptions();
            }

            using (var requestMessage = BuildRequestMessage(method, url, data, headers, options))
            {
                return await SendRequestAsync(requestMessage, options.TimeoutInSeconds);
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
            // However the timeout value used is the same for all service points.
            var servicePoint = ServicePointManager.FindServicePoint(url, null);
            servicePoint.ConnectionLeaseTimeout = (int) _connectionLeaseTimeout.TotalMilliseconds;
        }

        /// <summary>
        /// Builds the request message serializing data.
        /// </summary>
        /// <param name="method">HTTP method to use when making the request.</param>
        /// <param name="url">URL to send request message.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">Advanced request options. See <see cref="HttpRequestOptions"/> for more details.</param>
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
                    var added = message.Headers.TryAddWithoutValidation(key, headers[key]);
                    if (!added && message.Content != null)
                    {
                        // If could not add to request headers, then it may be a content header.
                        // Skip when it's been already set by this client (via Content creation)
                        if (message.Content.Headers.ContentType == null)
                        {
                            message.Content.Headers.TryAddWithoutValidation(key, headers[key]);
                        }
                    }
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
        /// <para>
        /// The <paramref name="requestTimeout"/> will only be effective if it's value is less than default timeout
        /// configured during initialization of <see cref="_httpClient"/>. Otherwise the default timeout will take 
        /// place. When <paramref name="requestTimeout"/> is zero the per-request basis timeout is not even configured.
        /// </para>
        /// <para>
        /// Currently it's not possible to cancel a request by any means other than timeout.
        /// </para>
        /// </remarks>
        private async Task<Response> SendRequestAsync(HttpRequestMessage requestMessage, int requestTimeout = 0)
        {
            using (var cts = GetCancellationTokenSource(TimeSpan.FromSeconds(requestTimeout)))
            {
                string guideNumber = GenerateGuideNumber();
                var stopwatch = new Stopwatch();

                try
                {
                    _logger?.RequestStarting(guideNumber, requestMessage);
                    stopwatch.Start();

                    var httpResponseMessage = await _httpClient.SendAsync(requestMessage, cts?.Token ?? CancellationToken.None);
                    string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();

                    stopwatch.Stop();
                    _logger?.RequestFinished(guideNumber, stopwatch.Elapsed, httpResponseMessage.StatusCode);

                    return new Response(httpResponseMessage, responseBody, stopwatch.Elapsed);
                }
                catch (TaskCanceledException)
                {
                    stopwatch.Stop();

                    // The task could have been canceled by either a native HttpClient timeout or a 
                    // timeout from cancellation token if set.

                    // This exception will be replaced by the one created below because:
                    //   - HttpClient timeout is not clear enough. It just says "A task was canceled".
                    //   - The exception should be the same regardless of the source of timeout.
                    throw CreateTimeoutException(guideNumber, stopwatch);
                }
            }
        }

        private static TimeoutException CreateTimeoutException(string guideNumber, Stopwatch stopwatch)
        {
            var timeoutException = new TimeoutException($"Request {guideNumber} timed out after {stopwatch.Elapsed.TotalMilliseconds} ms.");
            timeoutException.Data.Add("GuideNumber", guideNumber);
            timeoutException.Data.Add("ElapsedMilliseconds", stopwatch.Elapsed.TotalMilliseconds);

            return timeoutException;
        }

        /// <summary>
        /// Generates a random number that will be used as a guide to correlate a request to a response.
        /// With this number it's easier to see to which request the elapsed time corresponds.
        /// </summary>
        /// <returns>The generated number as string.</returns>
        private static string GenerateGuideNumber()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
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
        public Task<Response> GetAsync(string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync(HttpMethodEnum.GET, url, null, headers, options);
        }

        // GET (typed response)
        public Task<Response<TResponse>> GetAsync<TResponse>(string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync<TResponse>(HttpMethodEnum.GET, url, null, headers, options);
        }

        // POST
        public Task<Response> PostAsync(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync(HttpMethodEnum.POST, url, data, headers, options);
        }

        // POST (typed response)
        public Task<Response<TResponse>> PostAsync<TResponse>(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync<TResponse>(HttpMethodEnum.POST, url, data, headers, options);
        }

        // PUT
        public Task<Response> PutAsync(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync(HttpMethodEnum.PUT, url, data, headers, options);
        }

        // PUT (typed response)
        public Task<Response<TResponse>> PutAsync<TResponse>(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync<TResponse>(HttpMethodEnum.PUT, url, data, headers, options);
        }

        // PATCH
        public Task<Response> PatchAsync(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync(HttpMethodEnum.PATCH, url, data, headers, options);
        }

        // PATCH (typed response)
        public Task<Response<TResponse>> PatchAsync<TResponse>(string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync<TResponse>(HttpMethodEnum.PATCH, url, data, headers, options);
        }

        // DELETE
        public Task<Response> DeleteAsync(string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync(HttpMethodEnum.DELETE, url, null, headers, options);
        }

        // DELETE (typed response)
        public Task<Response<TResponse>> DeleteAsync<TResponse>(string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return SendAsync<TResponse>(HttpMethodEnum.DELETE, url, null, headers, options);
        }
    }
}
