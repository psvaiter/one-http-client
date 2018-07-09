﻿using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace OneHttpClient.Extensions
{
    /// <summary>
    /// Extension methods to log info about HTTP requests.
    /// </summary>
    public static class LoggerExtensions
    {
        // Deletages created by LoggerMessage
        private static readonly Action<ILogger, string, string, Exception> _requestStarting;
        private static readonly Action<ILogger, double, int, Exception> _requestFinished;

        // Named format string (template)
        private static readonly string _requestStartingTemplate = "Request starting {HttpMethod} {RequestUri}";
        private static readonly string _requestFinishedTemplate = "Request finished in {ElapsedMilliseconds} ms [{StatusCode}]";

        static LoggerExtensions()
        {
            // Initialize delegates
            _requestStarting = LoggerMessage.Define<string, string>(LogLevel.Information, 0, _requestStartingTemplate);
            _requestFinished = LoggerMessage.Define<double, int>(LogLevel.Information, 0, _requestFinishedTemplate);
        }

        /// <summary>
        /// Logs that an HTTP request is starting.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="uri">The URI to put in log message.</param>
        public static void RequestStarting(this ILogger logger, HttpRequestMessage httpRequestMessage)
        {
            string httpMethod = httpRequestMessage.Method.ToString();
            string uri = httpRequestMessage.RequestUri.ToString();

            _requestStarting(logger, httpMethod, uri, null);
        }

        /// <summary>
        /// Logs that the HTTP request has finished.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="elapsedTime">The time taken to complete the request.
        /// It will be put in log message and will be available for semantic logging.
        /// </param>
        /// <param name="statusCode">The status code of the HTTP response.
        /// It will be put in log message and will be available for semantic logging.
        /// </param>
        public static void RequestFinished(this ILogger logger, TimeSpan elapsedTime, HttpStatusCode statusCode)
        {
            _requestFinished(logger, elapsedTime.TotalMilliseconds, (int) statusCode, null);
        }
    }
}
