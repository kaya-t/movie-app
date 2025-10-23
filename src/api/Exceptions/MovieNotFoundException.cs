namespace MovieApp.Api.Exceptions;

public class MovieNotFoundException : Exception
{
    public string MovieId { get; }

    public MovieNotFoundException(string movieId) : base($"Movie with ID '{movieId}' not found in any provider")
    {
        MovieId = movieId;
    }

    public MovieNotFoundException(string movieId, Exception innerException) 
        : base($"Movie with ID '{movieId}' not found in any provider", innerException)
    {
        MovieId = movieId;
    }
}

