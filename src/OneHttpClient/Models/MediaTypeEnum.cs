namespace OneHttpClient.Models
{
    /// <summary>
    /// Types of supported media.
    /// </summary>
    public enum MediaTypeEnum
    {
        /// <summary>
        /// Content is expected to be an array of bytes and will be sent as is.
        /// User must define the Content-Type.
        /// </summary>
        RawBytes = 0,

        /// <summary>
        /// Text formatted as JSON.
        /// When chosen, request object will be automatically serialized to JSON.
        /// Content-Type will be set to "application/json".
        /// </summary>
        JSON,

        /// <summary>
        /// Text formatted as XML.
        /// When chosen, request object will be automatically serialized to XML.
        /// Content-Type will be set to "application/xml".
        /// </summary>
        XML,

        /// <summary>
        /// Unformatted text. Content-Type will be set to "text/plain".
        /// </summary>
        PlainText,

        /// <summary>
        /// Unformatted text. User must define Content-Type.
        /// </summary>
        OtherText,

        //MessagePack
    }
}
