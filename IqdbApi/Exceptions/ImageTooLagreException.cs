using System;

namespace IqdbApi.Exceptions
{
    public class ImageTooLargeException : InvalidFileFormatException
    {
        public ImageTooLargeException(
            string message = "Input image is too large. Maximum image resolution is 7500x7500. Maximum file size is 8192 KB.", 
            Exception innerException = null) 
            : base(message, innerException) { }
    }
}