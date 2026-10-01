using System.Net;

namespace MyProject.WebApi.Common.Exceptions;

public sealed class NotFoundException(string identifier, object key)
    : ApplicationException($"{identifier} with id {key} not found", HttpStatusCode.NotFound);