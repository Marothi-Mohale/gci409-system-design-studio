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

public sealed class ValidationException(string message) : ApplicationExceptionBase(message)
{
}
