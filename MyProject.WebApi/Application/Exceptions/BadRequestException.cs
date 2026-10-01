namespace MyProject.WebApi.Application.Exceptions;

public sealed class BadRequestException(string? message) : ApplicationException(message);