using OneHttpClient.Models;

public class HttpRequestOptions
{
    /// <summary>
    /// The timeout in seconds for the request.
    /// </summary>
    public int TimeoutInSeconds { get; set; }

    /// <summary>
    /// Indicates the format in which body content should be sent.
    /// Default: JSON.
    /// </summary>
    public MediaTypeEnum MediaType { get; set; } = MediaTypeEnum.JSON;

    /// <summary>
    /// The stategy to use when (de)serializing property names.
    /// Only applicable to JSON media type. Default: CamelCase.
    /// </summary>
    public NamingStrategyEnum NamingStrategy { get; set; } = NamingStrategyEnum.CamelCase;
}