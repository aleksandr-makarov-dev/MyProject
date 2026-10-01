namespace MyProject.WebApi.Application.Abstract.Exceptions;

public sealed class NotFoundException(string identifier, object key)
    : ApplicationException($"{identifier} with id {key} not found");