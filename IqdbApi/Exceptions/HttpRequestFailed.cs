using System;

namespace IqdbApi.Exceptions
{
    public class HttpRequestFailed : InvalidFileFormatException
    {
        public HttpRequestFailed(
            string message = "Incorrect url or service is unavailable.", 
            Exception innerException = null) 
            : base(message, innerException) { }
    }
}