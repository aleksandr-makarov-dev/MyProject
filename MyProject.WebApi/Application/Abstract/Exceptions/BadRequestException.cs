namespace MyProject.WebApi.Application.Abstract.Exceptions;

public sealed class BadRequestException(string? message) : ApplicationException(message);