namespace FourPrep.Common.Application.Exceptions;

using FourPrep.Common.Domain;

public sealed class ApplicationException : Exception
{
    public ApplicationException(string requestName, Error? error = default, Exception? innerException = default)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }

    public Error? Error { get; }
}
