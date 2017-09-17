using System;
using System.Net;

namespace BoardZ.API.Exceptions
{
    public class HttpStatusCodeException : Exception
    {
        public HttpStatusCodeException(HttpStatusCode statusCode) :
            this(statusCode, string.Empty)
        {

        }

        public HttpStatusCodeException(HttpStatusCode statusCode, string message) :
            this(statusCode, message, null)
        {

        }

        public HttpStatusCodeException(HttpStatusCode statusCode, string message, Exception innerException) :
            base(message, innerException) => StatusCode = statusCode;

        public HttpStatusCode StatusCode { get; set; }

        public int StatusCodeAsInt => (int)StatusCode;
    }
}
