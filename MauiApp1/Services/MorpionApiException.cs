namespace MauiApp1.Services;

public class MorpionApiException : Exception
{
    public MorpionApiException(string message, bool isUnauthorized = false) : base(message)
    {
        IsUnauthorized = isUnauthorized;
    }

    public bool IsUnauthorized { get; }
}
