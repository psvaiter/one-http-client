using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OneHttpClient.Models;

namespace OneHttpClient
{
    /// <summary>
    /// Utility methods.
    /// </summary>
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
            var encoding = Encoding.UTF8;

            if (data == null)
            {
                return null;
            }

            if (options.MediaType == MediaTypeEnum.JSON)
            {
                string serializedData = SerializeToJson(data, options);
                return new StringContent(serializedData, encoding, "application/json");
            }

            if (options.MediaType == MediaTypeEnum.XML)
            {
                string serializedData = SerializeToXml(data, encoding);
                return new StringContent(serializedData, encoding, "application/xml");
            }

            if (options.MediaType == MediaTypeEnum.PlainText)
            {
                return new StringContent(data as string, encoding, "text/plain");
            }

            if (options.MediaType == MediaTypeEnum.UnknownText)
            {
                return new ByteArrayContent(encoding.GetBytes(data as string));
            }

            // RawBytes
            return new ByteArrayContent((byte[]) data);
        }

        /// <summary>
        /// Serializes data to JSON.
        /// </summary>
        /// <param name="data">Object to be serialized.</param>
        /// <param name="options">Serialization options for request.</param>
        /// <returns>A string formatted as JSON.</returns>
        private static string SerializeToJson(object data, HttpRequestOptions options)
        {
            var serializerSettings = GetJsonSerializerSettings(options.NamingStrategy, options.NullValueHandling);
            string serializedData = JsonConvert.SerializeObject(data, serializerSettings);
            return serializedData;
        }

        /// <summary>
        /// Tries to deserializes a response body based on response <c>Content-Type</c> header.
        /// </summary>
        /// <typeparam name="TResponse">Type to deserialize the response body.</typeparam>
        /// <param name="headers">Headers with information </param>
        /// <param name="responseBody">The string of response body to be deserialized.</param>
        /// <param name="namingStrategy">The stategy to use when serializing property names.
        /// Default is <see cref="NamingStrategyEnum.CamelCase"/>.
        /// </param>
        /// <param name="nullValueHandling">Tells whether to include or ignore null values when (de)serializing.</param>
        /// <returns>The deserialized object or default value of type.</returns>
        /// <remarks>
        /// Currently supports JSON and XML. If Content-Type is not supported or if deserialization 
        /// fails the default value of type is returned.
        /// </remarks>
        public static TResponse TryDeserializeResponseBody<TResponse>(
            NameValueCollection headers, 
            string responseBody, 
            NamingStrategyEnum namingStrategy = NamingStrategyEnum.CamelCase, 
            NullValueHandling nullValueHandling = NullValueHandling.Include)
        {
            if (headers != null && string.IsNullOrEmpty(responseBody) == false)
            {
                string contentType = headers.Get("Content-Type");

                if (contentType?.Contains("application/json") == true)
                {
                    return TryDeserializeJson<TResponse>(responseBody, GetJsonSerializerSettings(namingStrategy, nullValueHandling));
                }

                if (contentType?.Contains("application/xml") == true)
                {
                    return TryDeserializeXml<TResponse>(responseBody, Encoding.UTF8);
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
        /// <param name="nullValueHandling">Tells whether to include or ignore null values when (de)serializing.</param>
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

        /// <summary>
        /// Serializes an object to a XML string.
        /// </summary>
        /// <param name="data">Object to serialize.</param>
        /// <param name="encoding">The encoding of result XML string.</param>
        /// <returns>A valid XML string.</returns>
        private static string SerializeToXml(object data, Encoding encoding)
        {
            var xmlSerializer = new XmlSerializer(data.GetType());
            var xmlSettings = new XmlWriterSettings()
            {
                Encoding = encoding,
                Indent = false
            };

            using (var memoryStream = new MemoryStream())
            {
                using (var textWriter = new StreamWriter(memoryStream, encoding))
                {
                    using (var xmlWriter = XmlWriter.Create(textWriter, xmlSettings))
                    {
                        xmlSerializer.Serialize(xmlWriter, data);
                    }
                }

                return encoding.GetString(memoryStream.ToArray());
            }

            // Note: Microsoft recommends using XmlWriter instead of XmlTextWriter. That's why it's being used here.
            // XmlTextWriter would turn StreamWriter + XmlWriter into a single step.
        }

        /// <summary>
        /// Deserializes XML to object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of target object.</typeparam>
        /// <param name="xml">XML string to be deserialized.</param>
        /// <param name="encoding">The encoding of text in <paramref name="xml"/> parameter.</param>
        /// <returns>The deserialized object when successful. The default value of type otherwise.</returns>
        private static T TryDeserializeXml<T>(string xml, Encoding encoding)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));

                using (var memoryStream = new MemoryStream(encoding.GetBytes(xml)))
                {
                    using (var textReader = new StreamReader(memoryStream, encoding))
                    {
                        return (T) xmlSerializer.Deserialize(textReader);
                    }
                }
            }
            catch
            {
                return default(T);
            }
        }
    }
}
