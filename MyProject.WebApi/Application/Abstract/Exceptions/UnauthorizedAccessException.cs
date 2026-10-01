namespace MyProject.WebApi.Application.Abstract.Exceptions;

public class UnauthorizedAccessException(string? message) : ApplicationException(message);