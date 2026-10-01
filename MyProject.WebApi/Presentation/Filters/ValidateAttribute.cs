using Microsoft.AspNetCore.Mvc;

namespace MyProject.WebApi.Common.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ValidateAttribute(Type type)
    : TypeFilterAttribute(typeof(ValidationFilter<>).MakeGenericType(type));