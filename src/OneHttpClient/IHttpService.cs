﻿using System.Collections.Specialized;
using System.Threading.Tasks;
using OneHttpClient.Models;

namespace OneHttpClient
{
    /// <summary>
    /// Interface of service responsible to make HTTP requests.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Sends HTTP request message and gets the reponse.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="method">HTTP method to use when making the request.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="timeoutInSeconds">Amount of time in seconds to wait before cancelling request. When 0 the default timeout will be used.</param>
        /// <param name="namingStrategy">The stategy to use when serializing property names. Default is <see cref="NamingStrategyEnum.CamelCase"/>.</param>
        /// <returns><see cref="Response"/> with data from HTTP response.</returns>
        Task<Response> Send(HttpMethodEnum method, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null);

        /// <summary>
        /// Sends HTTP request message, gets the reponse and tries to deserialize it to a given type.
        /// </summary>
        /// <param name="url">URL to send request message.</param>
        /// <param name="method">HTTP method to use when making the request.</param>
        /// <param name="data">Object containing data to be sent.</param>
        /// <param name="headers">Headers of request message.</param>
        /// <param name="timeoutInSeconds">Amount of time in seconds to wait before cancelling request. When 0 the default timeout will be used.</param>
        /// <param name="namingStrategy">The stategy to use when serializing property names. Default is <see cref="NamingStrategyEnum.CamelCase"/>.</param>
        /// <returns><see cref="Response{TResponse}"/> with data from HTTP response.</returns>
        Task<Response<TResponse>> Send<TResponse>(HttpMethodEnum method, string url, object data = null, NameValueCollection headers = null, HttpRequestOptions options = null);
    }
}
