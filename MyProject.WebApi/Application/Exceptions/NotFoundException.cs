using System.Net;

namespace MyProject.WebApi.Application.Exceptions;

public sealed class NotFoundException(string identifier, object key)
    : ApplicationException($"{identifier} with id {key} not found", HttpStatusCode.NotFound);