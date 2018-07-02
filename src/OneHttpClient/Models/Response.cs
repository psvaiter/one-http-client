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
        /// <param name="headers">Request headers as key-value collection.</param>
        /// <param name="responseBody">The content of the response as a string.</param>
        /// <param name="elapsedTime">The time the request took to complete.</param>
        public Response(HttpStatusCode statusCode, NameValueCollection headers, string responseBody, TimeSpan elapsedTime)
        {
            StatusCode = statusCode;
            Headers = headers ?? new NameValueCollection();
            ResponseBody = responseBody;
            ElapsedTime = elapsedTime;
        }

        /// <summary>
        /// Constructs a response from an <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="fullResponse">The original <see cref="HttpResponseMessage"/>.</param>
        /// <param name="responseBody">The content of the response as a string.</param>
        /// <param name="elapsedTime">The time the request took to complete.</param>
        /// <remarks>
        /// HTTP status and headers are automatically read from <paramref name="fullResponse"/> 
        /// and assigned to the properties <see cref="StatusCode"/> and <see cref="Headers"/> 
        /// respectively, but <see cref="HttpRequestMessage.Content"/> is not read from message 
        /// and must be passed in <paramref name="responseBody"/>.
        /// </remarks>
        public Response(HttpResponseMessage fullResponse, string responseBody, TimeSpan elapsedTime)
        {
            if (fullResponse == null)
            {
                throw new ArgumentNullException(nameof(fullResponse));
            }

            StatusCode = fullResponse.StatusCode;
            ResponseBody = responseBody;
            ElapsedTime = elapsedTime;

            Headers = new NameValueCollection();
            foreach (var header in fullResponse.Headers)
            {
                Headers.Add(header.Key, string.Join(",", header.Value));
            }

            foreach (var header in fullResponse.Content.Headers)
            {
                Headers.Add(header.Key, string.Join(",", header.Value));
            }

            FullResponse = fullResponse;
        }

        /// <summary>
        /// Gets the returned HttpStatusCode.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Indicates if the StatusCode represents a successful operation.
        /// </summary>
        public bool IsSuccessStatusCode => ((int) StatusCode >= 200) && ((int) StatusCode <= 299);

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
        /// Provided for flexibility and advanced use cases, it's the original <see cref="HttpResponseMessage"/> 
        /// returned by <see cref="HttpClient"/> request.
        /// </summary>
        public HttpResponseMessage FullResponse { get; }
    }

    /// <summary>
    /// Represents the response of a web request.
    /// </summary>
    /// <typeparam name="TData">Type of object that should hold the returned data.</typeparam>
    public sealed class Response<TData> : Response
    {
        /// <summary>
        /// Constructs a response from an <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="fullResponse">The original <see cref="HttpResponseMessage"/>.</param>
        /// <param name="responseBody">The content of the response as a string.</param>
        /// <param name="responseData">The deserialized content of response.</param>
        /// <param name="elapsedTime">The time the request took to complete.</param>
        /// <remarks>
        /// HTTP status and headers are automatically read from <paramref name="fullResponse"/> 
        /// and assigned to the properties StatusCode and Headers respectively, but 
        /// <see cref="HttpRequestMessage.Content"/> is not read from message and must be passed 
        /// in <paramref name="responseBody"/>.
        /// </remarks>
        public Response(HttpResponseMessage fullResponse, string responseBody, TData responseData, TimeSpan elapsedTime)
            : base(fullResponse, responseBody, elapsedTime)
        {
            ResponseData = responseData;
        }

        /// <summary>
        /// Constructs a response from another response.
        /// </summary>
        /// <param name="other">The other response from which data should be copied.</param>
        /// <param name="responseData">The deserialized content of response.</param>
        public Response(Response other, TData responseData)
            : base(other.FullResponse, other.ResponseBody, other.ElapsedTime)
        {
            ResponseData = responseData;
        }

        /// <summary>
        /// The returned data deserialized to type T.
        /// </summary>
        public TData ResponseData { get; }
    }
}
