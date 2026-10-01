using System.Net;

namespace MyProject.WebApi.Common.Exceptions;

public class ApplicationException : Exception
{
    public HttpStatusCode StatusCode { get; }

    protected ApplicationException(string? message = null,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
    {
        StatusCode = statusCode;
    }

    protected ApplicationException(string? message, Exception? innerException,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}