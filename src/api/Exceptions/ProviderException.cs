namespace MovieApp.Api.Exceptions;

public class ProviderException : Exception
{
    public string ProviderName { get; }

    public ProviderException(string providerName, string message) : base(message)
    {
        ProviderName = providerName;
    }

    public ProviderException(string providerName, string message, Exception innerException) 
        : base(message, innerException)
    {
        ProviderName = providerName;
    }
}

