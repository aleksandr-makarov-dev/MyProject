namespace MyProject.WebApi.Application.Exceptions;

public class UnauthorizedAccessException(string? message) : ApplicationException(message);