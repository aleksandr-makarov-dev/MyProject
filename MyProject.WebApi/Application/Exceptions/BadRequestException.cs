using System.Net;

namespace MyProject.WebApi.Common.Exceptions;

public sealed class BadRequestException(string? message) : ApplicationException(message, HttpStatusCode.BadRequest);