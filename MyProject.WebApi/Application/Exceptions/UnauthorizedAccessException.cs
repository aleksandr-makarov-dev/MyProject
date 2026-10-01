using System.Net;

namespace MyProject.WebApi.Common.Exceptions;

public class UnauthorizedAccessException(string? message) : ApplicationException(message, HttpStatusCode.Unauthorized);