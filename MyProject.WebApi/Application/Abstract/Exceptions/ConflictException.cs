namespace MyProject.WebApi.Application.Abstract.Exceptions;

public sealed class ConflictException(string? message) : ApplicationException(message);