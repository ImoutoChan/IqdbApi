using System;

namespace IqdbApi.Exceptions
{
    public class InvalidFileFormatException : FormatException
    {
        public InvalidFileFormatException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
