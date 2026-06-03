namespace ClimateSentinel.Exceptions;

public sealed class AnaliseClimaticaException : Exception
{
    public AnaliseClimaticaException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
