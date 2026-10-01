namespace MyProject.WebApi.Application.Abstract.Exceptions;

public class ApplicationException : Exception
{
    protected ApplicationException(string? message = null) : base(message)
    {
    }

    protected ApplicationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}