using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;

namespace OneHttpClient.Models
{
    /// <summary>
    /// Represents the response of a web request.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Creates a response with informed parameters.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="isSuccessStatusCode">True if status code indicates a successful response (2xx).</param>
        /// <param name="headers">Request headers as key-value collection.</param>
        /// <param name="responseBody">The content of the response as a string.</param>
        /// <param name="elapsedTime">The time the request took to complete.</param>
        public Response(HttpStatusCode statusCode, bool isSuccessStatusCode, NameValueCollection headers, string responseBody, TimeSpan elapsedTime)
        {
            StatusCode = statusCode;
            IsSuccessStatusCode = isSuccessStatusCode;
            Headers = headers ?? new NameValueCollection();
            ResponseBody = responseBody;
            ElapsedTime = elapsedTime;
        }

        /// <summary>
        /// Constructs a response from an <see cref="HttpResponseMessage"/>.
        /// HTTP status and headers are read automatically and assigned to appropriate properties, 
        /// but content is not read from message and must be passed in <paramref name="responseBody"/>.
        /// </summary>
        /// <param name="fullResponseMessage">The original <see cref="HttpResponseMessage"/>.</param>
        /// <param name="responseBody">The content of the response as a string.</param>
        /// <param name="elapsedTime">The time the request took to complete.</param>
        public Response(HttpResponseMessage fullResponseMessage, string responseBody, TimeSpan elapsedTime)
        {
            StatusCode = fullResponseMessage.StatusCode;
            IsSuccessStatusCode = fullResponseMessage.IsSuccessStatusCode;
            ResponseBody = responseBody;
            ElapsedTime = elapsedTime;

            Headers = new NameValueCollection();
            foreach (var header in fullResponseMessage.Headers)
            {
                Headers.Add(header.Key, string.Join(",", header.Value));
            }

            foreach (var header in fullResponseMessage.Content.Headers)
            {
                Headers.Add(header.Key, string.Join(",", header.Value));
            }
        }

        /// <summary>
        /// Gets the returned HttpStatusCode.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the flag that indicates whether the StatusCode represents a successful operation.
        /// </summary>
        public bool IsSuccessStatusCode { get; }

        /// <summary>
        /// Gets the response headers.
        /// </summary>
        public NameValueCollection Headers { get; }

        /// <summary>
        /// Gets the raw string returned in response.
        /// </summary>
        public string ResponseBody { get; }

        /// <summary>
        /// The elapsed time of the request.
        /// </summary>
        public TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Provided for flexibility and advanced use cases.
        /// </summary>
        public HttpResponseMessage FullResponse { get; }
    }

    /// <summary>
    /// Represents the response of a web request.
    /// </summary>
    /// <typeparam name="T">Type of object that should hold the returned data.</typeparam>
    public sealed class Response<T> : Response
    {
        /// <summary>
        /// Constructs a response from an <see cref="HttpResponseMessage"/>.
        /// HTTP status and headers are read automatically and assigned to appropriate properties, 
        /// but content is not read from message and must be passed in <paramref name="responseBody"/>.
        /// </summary>
        /// <param name="fullResponseMessage">The original <see cref="HttpResponseMessage"/>.</param>
        /// <param name="responseBody">The content of the response as a string.</param>
        /// <param name="responseData">The deserialized content of response.</param>
        /// <param name="elapsedTime">The time the request took to complete.</param>
        public Response(HttpResponseMessage fullResponseMessage, string responseBody, T responseData, TimeSpan elapsedTime)
            : base(fullResponseMessage, responseBody, elapsedTime)
        {
            ResponseData = responseData;
        }

        /// <summary>
        /// Constructs a response from another response.
        /// HTTP status and headers are read automatically and assigned to appropriate properties, 
        /// but content is not read from message and must be passed in <paramref name="responseBody"/>.
        /// </summary>
        /// <param name="other">The other response from which data should be copied.</param>
        /// <param name="responseData">The deserialized content of response.</param>
        public Response(Response other, T responseData)
            : base(other.FullResponse, other.ResponseBody, other.ElapsedTime)
        {
            ResponseData = responseData;
        }

        /// <summary>
        /// The returned data deserialized to type T.
        /// </summary>
        public T ResponseData { get; }
    }
}
