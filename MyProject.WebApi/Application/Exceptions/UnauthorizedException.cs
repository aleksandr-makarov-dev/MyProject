namespace MyProject.WebApi.Application.Exceptions;

public class UnauthorizedException(string? message) : ApplicationException(message);