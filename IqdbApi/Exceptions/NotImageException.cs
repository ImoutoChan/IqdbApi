using System;

namespace IqdbApi.Exceptions
{
    public class NotImageException : InvalidFileFormatException
    {
        public NotImageException(
            string message = "Not an image or image format not supported.", 
            Exception innerException = null) 
            : base(message, innerException) { }
    }
}