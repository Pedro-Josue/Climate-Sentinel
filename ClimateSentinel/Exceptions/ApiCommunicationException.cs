namespace ClimateSentinel.Exceptions;

public sealed class ApiCommunicationException : Exception
{
    public ApiCommunicationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
