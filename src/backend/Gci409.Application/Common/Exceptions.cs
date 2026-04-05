namespace Gci409.Application.Common;

public abstract class ApplicationExceptionBase(string message) : Exception(message)
{
}

public sealed class NotFoundException(string message) : ApplicationExceptionBase(message)
{
}

public sealed class ForbiddenException(string message) : ApplicationExceptionBase(message)
{
}

public sealed class ValidationException : ApplicationExceptionBase
{
    public ValidationException(string message)
        : base(message)
    {
    }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
