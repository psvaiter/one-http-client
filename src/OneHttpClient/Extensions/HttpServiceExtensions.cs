using System.Collections.Specialized;
using System.Threading.Tasks;
using OneHttpClient.Models;

namespace OneHttpClient.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="HttpService"/>.
    /// </summary>
    public static class HttpServiceExtensions
    {
        // GET
        /// <summary>
        /// Sends HTTP GET request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response"/> with data from HTTP response.</returns>
        public static Task<Response> Get(this IHttpService httpService, string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send(HttpMethodEnum.GET, url, null, headers, options);
        }

        // GET (typed response)
        /// <summary>
        /// Sends HTTP GET request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public static Task<Response<TResponse>> Get<TResponse>(this IHttpService httpService, string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send<TResponse>(HttpMethodEnum.GET, url, null, headers, options);
        }

        // POST
        /// <summary>
        /// Sends HTTP POST request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response"/> with data from HTTP response.</returns>
        public static Task<Response> Post(this IHttpService httpService, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send(HttpMethodEnum.POST, url, data, headers, options);
        }

        // POST (typed response)
        /// <summary>
        /// Sends HTTP POST request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public static Task<Response<TResponse>> Post<TResponse>(this IHttpService httpService, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send<TResponse>(HttpMethodEnum.POST, url, data, headers, options);
        }

        // PUT
        /// <summary>
        /// Sends HTTP PUT request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response"/> with data from HTTP response.</returns>
        public static Task<Response> Put(this IHttpService httpService, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send(HttpMethodEnum.PUT, url, data, headers, options);
        }

        // PUT (typed response)
        /// <summary>
        /// Sends HTTP PUT request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public static Task<Response<TResponse>> Put<TResponse>(this IHttpService httpService, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send<TResponse>(HttpMethodEnum.PUT, url, data, headers, options);
        }

        // PATCH
        /// <summary>
        /// Sends HTTP PATCH request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response"/> with data from HTTP response.</returns>
        public static Task<Response> Patch(this IHttpService httpService, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send(HttpMethodEnum.PATCH, url, data, headers, options);
        }

        // PATCH (typed response)
        /// <summary>
        /// Sends HTTP PATCH request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public static Task<Response<TResponse>> Patch<TResponse>(this IHttpService httpService, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send<TResponse>(HttpMethodEnum.PATCH, url, data, headers, options);
        }

        // DELETE
        /// <summary>
        /// Sends HTTP DELETE request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response"/> with data from HTTP response.</returns>
        public static Task<Response> Delete(this IHttpService httpService, string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send(HttpMethodEnum.DELETE, url, null, headers, options);
        }

        // DELETE (typed response)
        /// <summary>
        /// Sends HTTP DELETE request message.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="options">(Optional) Additional request configurations. If not informed, the default will be used.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        public static Task<Response<TResponse>> Delete<TResponse>(this IHttpService httpService, string url, NameValueCollection headers = null, HttpRequestOptions options = null)
        {
            return httpService.Send<TResponse>(HttpMethodEnum.DELETE, url, null, headers, options);
        }
    }
}
