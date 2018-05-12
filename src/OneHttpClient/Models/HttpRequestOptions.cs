using System.Collections.Specialized;
using OneHttpClient.Models;

public class HttpRequestOptions
{
    /// <summary>
    /// The timeout in seconds for the request.
    /// </summary>
    public int TimeoutInSeconds { get; set; }
    
    /// <summary>
    /// The stategy to use when (de)serializing property names.
    /// </summary>
    public NamingStrategyEnum NamingStrategy { get; set; } = NamingStrategyEnum.CamelCase;
}