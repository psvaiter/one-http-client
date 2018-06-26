using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OneHttpClient.Models;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace OneHttpClient
{
    public class Utils
    {
        /// <summary>
        /// Creates an <see cref="HttpContent"/> based on <see cref="HttpRequestOptions"/>.
        /// The media type will determine how the content will be created.
        /// </summary>
        /// <param name="data">The data object to be sent in request.</param>
        /// <param name="options">
        /// The request options with the media type and other info that can be used to create
        /// an HttpContent object automatically.
        /// </param>
        /// <returns></returns>
        public static HttpContent CreateHttpContent(object data, HttpRequestOptions options)
        {
            if (data == null)
            {
                return null;
            }

            if (options.MediaType == MediaTypeEnum.PlainText)
            {
                return new StringContent(data as string, Encoding.UTF8, "text/plain");
            }

            if (options.MediaType == MediaTypeEnum.JSON)
            {
                var serializerSettings = GetJsonSerializerSettings(options.NamingStrategy, options.NullValueHandling);
                string serializedData = JsonConvert.SerializeObject(data, serializerSettings);

                return new StringContent(serializedData, Encoding.UTF8, "application/json");
            }

            return new ByteArrayContent(CovnertToByteArray(data));
        }

        /// <summary>
        /// Converts an object to byte array.
        /// </summary>
        /// <param name="obj">The object to be converted.</param>
        /// <returns>The byte array.</returns>
        public static byte[] CovnertToByteArray(object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
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
        public static TResponse TryDeserializeResponseBody<TResponse>
            (
                NameValueCollection headers,
                string responseBody,
                NamingStrategyEnum namingStrategy = NamingStrategyEnum.CamelCase,
                NullValueHandling nullValueHandling = NullValueHandling.Include
            )
        {
            if (headers != null && string.IsNullOrEmpty(responseBody) == false)
            {
                string contentType = headers.Get("Content-Type");

                if (contentType?.Contains("application/json") == true)
                {
                    return TryDeserializeJson<TResponse>(responseBody, GetJsonSerializerSettings(namingStrategy, nullValueHandling));
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
        private static T TryDeserializeJson<T>(string json, JsonSerializerSettings serializerSettings = null)
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

        /// <summary>
        /// Gets the JSON serializer settings and sets the naming strategy supplied.
        /// </summary>
        /// <param name="strategy">The naming strategy to be used during serialization.</param>
        /// <returns>The serializer settings of Newtonsoft.Json.</returns>
        private static JsonSerializerSettings GetJsonSerializerSettings(NamingStrategyEnum strategy, NullValueHandling nullValueHandling)
        {
            return new JsonSerializerSettings()
            {
                NullValueHandling = nullValueHandling,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                // TODO: test DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
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
        private static NamingStrategy GetNamingStrategy(NamingStrategyEnum strategy)
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
    }
}
